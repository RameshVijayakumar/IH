using System;
using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;

namespace Paycor.Import.Registration
{
    public abstract class BaseMappingGenerator
    {
        protected abstract bool AreFieldsNeededFromDefinitionSchema();
        protected abstract bool AreFieldsNeededFromOperation();

        public IEnumerable<GeneratedMapping> GetMappings(SwaggerDocumentDefinition swaggerDocument, Operation operation, MappingEndpoints mappingEndpoints)
        {
            if (operation.parameters == null)
                return null;

            var mappings = new List<GeneratedMapping>();
            var fieldDefinitionsList = new List<MappingFieldDefinition>();

            AddFieldsFromOperation(operation, fieldDefinitionsList);

            var schema = operation.parameters.Where(t => t.schema != null).Select(t => t.schema).FirstOrDefault();
            AddFieldsFromSchema(swaggerDocument, schema, fieldDefinitionsList);
            AddFieldDefinitionsFromMappingEndPointsRouteParameters(fieldDefinitionsList, mappingEndpoints);
            if (!fieldDefinitionsList.Any())
            {
                return null;
            }

            var mappingTypeName = MappingTypeName(swaggerDocument, operation);
            var description = GetDescription(swaggerDocument, schema);
            var category = GetCategory(swaggerDocument, schema);
            var apiMapping = GetApiMapping(fieldDefinitionsList, mappingTypeName, mappingEndpoints, description, category);
            apiMapping.IsBatchSupported = IsBatchSupported(operation);
            apiMapping.ChunkSize = GetChunkSize(swaggerDocument, schema);
            if (apiMapping.IsBatchSupported)
            {
                apiMapping.IsBatchChunkingSupported = operation.BatchChunkingSupported;
                apiMapping.PreferredBatchChunkSize = operation.PreferredBatchChunkSize;
            }
            mappings.Add(apiMapping);
            return mappings;
        }

        private static int GetChunkSize(SwaggerDocumentDefinition swaggerDocument, Schema schema)
        {
            var definitionSchema = GetSchemaFromRef(schema, swaggerDocument);
            return definitionSchema?.chunkSize ?? 0;
        }

        private static string GetDescription(SwaggerDocumentDefinition swaggerDocument, Schema schema) =>
            GetSchemaFromRef(schema, swaggerDocument)?.mapDescription ?? "";

        private static string GetCategory(SwaggerDocumentDefinition swaggerDocument, Schema schema) =>
            GetSchemaFromRef(schema, swaggerDocument)?.mapCategory ?? "";

        private static string MappingTypeName(SwaggerDocumentDefinition swaggerDocumentDefinition, Operation operation)
        {
            if (operation != null && operation.parameters == null)
                return null;

            var schema = operation?.parameters.Where(t => t.schema != null).Select(t => t.schema).FirstOrDefault();
            if (schema == null)
                return null;

            var definitionSchema = GetSchemaFromRef(schema, swaggerDocumentDefinition);
            return definitionSchema?.mapType;
        }

        private static void AddFieldDefinitionsFromSchemaLookupRoutes(
            SwaggerDocumentDefinition swaggerDocumentDefinition,
            Schema definitionSchema, List<MappingFieldDefinition> fieldDefinitionsList,
           IEnumerable<string> requiredProperties)
        {
            if (definitionSchema.lookupRoutes == null)
            {
                return;
            }

            var exceptionMessage = $"{definitionSchema.mapType} not found";
            var lookupMappingFieldDefinitions = definitionSchema.lookupRoutes.Select(property => new MappingFieldDefinition
            {
                EndPoint = swaggerDocumentDefinition.FormatEndPointIgnoringBasePath(property.Key),
                Source = property.Key.ParseFirstQueryStringParam(),
                Destination = property.Value.Property,
                ValuePath = property.Value.ValuePath,
                Type = "string",
                ExceptionMessage = property.Value.ExceptionMessage ?? exceptionMessage,
                Required =
                                requiredProperties != null &&
                                requiredProperties.Contains(property.Value.Property.RemoveBraces().ToLower()),
                IsRequiredForPayload = property.Value.IsRequiredForPayload
            }).ToList();
            fieldDefinitionsList.AddRange(lookupMappingFieldDefinitions);
            AddLookupFieldToFieldDefinationList(lookupMappingFieldDefinitions, fieldDefinitionsList);
        }

        private static void AddLookupFieldToFieldDefinationList(IList<MappingFieldDefinition> lookupMappingFieldDefinitions, List<MappingFieldDefinition> fieldDefinitionsList)
        {
            if (!lookupMappingFieldDefinitions.Any()) return;
            foreach(var lookupMappingFieldDefinition in lookupMappingFieldDefinitions)
            {
                AddToListOfMappingFieldDefinitionsIfLookupFieldDoesNotExists(lookupMappingFieldDefinition.Source.GetFieldsFromBraces(), fieldDefinitionsList, lookupMappingFieldDefinition.Required);
            }
        }

        private static void AddToListOfMappingFieldDefinitionsIfLookupFieldDoesNotExists(IEnumerable<string> listOfLookupFields, List<MappingFieldDefinition> fieldDefinitionsList, bool required)
        {
            foreach(var lookupField in listOfLookupFields)
            {
                var lookupFieldExists = fieldDefinitionsList.Any(t => string.Equals(t.Source, lookupField, StringComparison.CurrentCultureIgnoreCase));
                if (!lookupFieldExists)
                {  
                    fieldDefinitionsList.Add(new MappingFieldDefinition
                    {
                        Source = lookupField,
                        Destination = lookupField,
                        Type = "string",
                        Required = required,
                        EndPoint = null,
                        GlobalLookupType = null
                    });
                }
            }
        }

        private static string GetSourceName(KeyValuePair<string, Schema> property)
        {
            return string.IsNullOrEmpty(property.Value.sourceName) ? property.Key : property.Value.sourceName;
        }

        private static void AddFieldDefinitionsFromSchemaProperties(Schema definitionSchema, List<MappingFieldDefinition> fieldDefinitionsList,
            SwaggerDocumentDefinition swaggerDocumentDefinition, string refKey = null)

        {
            if (definitionSchema.properties == null) return;
            var requiredProperties = GetRequiredProperties(definitionSchema);
            var arrayTypeProperties = GetArrayTypeProperties(definitionSchema);

            var fields = definitionSchema.properties.Where(p => p.Value.@ref == null && p.Value.items == null &&
                                                                (p.Value.ignore == null || !p.Value.ignore.Value))
                .Select(property => new MappingFieldDefinition
                {
                    Source = GetSourceName(property),
                    Destination = refKey == null ? property.Key : $"{refKey}.{property.Key}",
                    Type = SwaggerTypeDictionary.GetDotNetType($"{property.Value.type}#{property.Value.format}"),
                    GlobalLookupType = property.Value.globalLookupTypeName,
                    Required = requiredProperties != null && requiredProperties.Contains(property.Key.ToLower())
                });

            fieldDefinitionsList.AddRange(fields);

            if (arrayTypeProperties.Count > 0)
            {
                foreach (var arrayTypeProperty in arrayTypeProperties)
                {
                    var schema = GetSchemaFromRef(arrayTypeProperty.Value, swaggerDocumentDefinition);
                    AddFieldDefinitionsFromTypeArray(schema, fieldDefinitionsList, arrayTypeProperty);
                }
            }

            var nestedProps = definitionSchema.properties.Where(p => p.Value.@ref != null);
            foreach (var nestedProp in nestedProps)
            {
                var schema = GetSchemaFromRef(nestedProp.Value, swaggerDocumentDefinition);
                AddFieldDefinitionsFromSchemaProperties(schema, fieldDefinitionsList, swaggerDocumentDefinition, nestedProp.Key);
            }   
        }

        private static void AddFieldDefinitionsFromTypeArray(Schema schema, List<MappingFieldDefinition> fieldDefinitionsList, KeyValuePair<string,Schema> arrayTypeProperty)
        {
            var refKey = arrayTypeProperty.Key;
          
            if (schema != null)
            {
                var requiredProperties = GetRequiredProperties(schema);
                foreach (var property in schema.properties)
                {
                    var field = new MappingFieldDefinition
                    {
                        Destination = refKey == null ? property.Key : $"{refKey}[].{property.Key}",
                        Source = refKey == null ? GetSourceName(property) : $"{refKey} {GetSourceName(property)}",
                        Type = SwaggerTypeDictionary.GetDotNetType($"{property.Value.type}#{property.Value.format}"),
                        GlobalLookupType = property.Value.globalLookupTypeName,
                        Required = requiredProperties != null && requiredProperties.Contains(property.Key.ToLower())
                    };
                    var filedsWithThreeInstances = field.GetSubArrayFieldsOfMultipleInstances(3, refKey, property.Key);
                    fieldDefinitionsList.AddRange(filedsWithThreeInstances);  
                }                
            }
            else
            {
                var field = new MappingFieldDefinition
                {
                    Destination = refKey == null ? null : $"{refKey}[]",
                    Source = refKey,
                    Type = SwaggerTypeDictionary.GetDotNetType($"{arrayTypeProperty.Value.items.type}#{arrayTypeProperty.Value.format}"),
                    GlobalLookupType = arrayTypeProperty.Value.globalLookupTypeName,
                    Required = false
                };
                var fieldWithThreeInst = field.GetSubArrayFieldsOfMultipleInstances(3, refKey);
                fieldDefinitionsList.AddRange(fieldWithThreeInst);
            }   
        }

        private static void AddFieldDefinitionsFromOperationParameters(Operation operation, List<MappingFieldDefinition> fieldDefinitionsList)
        {
            fieldDefinitionsList.AddRange(
                operation.parameters.Where(t => t.schema == null).Select(parameter => new MappingFieldDefinition
                {
                    Source = parameter.name,
                    Destination = parameter.name,
                    Required = Convert.ToBoolean(parameter.required),
                    Type = SwaggerTypeDictionary.GetDotNetType($"{parameter.type}#{parameter.format}"),
                }));
        }

        private void AddFieldsFromSchema(SwaggerDocumentDefinition swaggerDocument, Schema schema, List<MappingFieldDefinition> fieldDefinitionsList)
        {
            if (schema == null) return;

            var definitionSchema = GetSchemaFromRef(schema, swaggerDocument);
            if (definitionSchema == null) return;

            GetFieldDefinitionsFromDefinitionSchema(swaggerDocument, definitionSchema, fieldDefinitionsList);
        }

        private static IEnumerable<string> GetRequiredProperties(Schema schema)
        {
            return schema.required?.Select(t => t.ToLower());
        }

        private static List<KeyValuePair<string, Schema>> GetArrayTypeProperties(Schema schema)
        {
            var listOfArrayProperties = new List<KeyValuePair<string, Schema>>();            
            listOfArrayProperties.AddRange(schema.properties.Where(p => p.Value.items != null && p.Value.type == "array"));
            return listOfArrayProperties;
        }

        private static Schema GetSchemaFromRef(Schema schema, SwaggerDocumentDefinition swaggerDocument)
        {
            string definitionName = null;

            if (schema?.@ref != null) definitionName = schema.@ref;
            else if (schema != null && schema.type == "array")
            {
                definitionName = schema.items.@ref;
            }

            Schema definitionSchema;

            if (definitionName == null || !swaggerDocument.definitions.TryGetValue(
                definitionName.Substring(ImportConstants.DefinitionPattern.Length), out definitionSchema)) return null;
            return definitionSchema;
        }

        private void GetFieldDefinitionsFromDefinitionSchema(SwaggerDocumentDefinition swaggerDocument, Schema definitionSchema,
            List<MappingFieldDefinition> fieldDefinitionsList)
        {
            if (!AreFieldsNeededFromDefinitionSchema())
                return;
            AddFieldDefinitionsFromSchemaProperties(definitionSchema, fieldDefinitionsList, swaggerDocument);
            AddFieldDefinitionsFromSchemaLookupRoutes(swaggerDocument, definitionSchema, fieldDefinitionsList, GetRequiredProperties(definitionSchema));
        }

        private void AddFieldsFromOperation(Operation operation,
            List<MappingFieldDefinition> fieldDefinitionsList)
        {
            if (!AreFieldsNeededFromOperation())
                return;
            AddFieldDefinitionsFromOperationParameters(operation, fieldDefinitionsList);
        }

        private GeneratedMapping GetApiMapping(IEnumerable<MappingFieldDefinition> fieldDefinitionsList,
                                        string mappingName, MappingEndpoints mappingEndpoints, string description, string category)
        {
            var apiMapping = new GeneratedMapping
            {
                MappingEndpoints = mappingEndpoints,
                Mapping = new MappingDefinition()
                {
                    FieldDefinitions = fieldDefinitionsList
                },
                MappingName = mappingName,
                ObjectType = mappingName,
                MappingDescription = description,
                MappingCategory = category
            };

            return apiMapping;
        }

        private static bool IsBatchSupported(Operation operation)
        {
            if (operation?.parameters == null) return false;

            var parameter = operation.parameters.FirstOrDefault(t => t.@in == "body");
            return parameter?.schema.type != null && parameter.schema != null && (parameter.schema.type == "array");
        }

        private void AddFieldDefinitionsFromMappingEndPointsRouteParameters(
            List<MappingFieldDefinition> fieldDefinitionsList, MappingEndpoints mappingEndpoints)
        {
            var routeParams = new List<string>();
            var listOfEndpoints = mappingEndpoints.GetListOfAllMappingEndpoints();
            foreach (var endpoint in listOfEndpoints)
            {
                routeParams.AddRange(endpoint.GetFieldsFromBraces());
            }

            if (routeParams.Count > 0)
                AddFieldDefinationsForRouteParameters(routeParams.Distinct().ToList(), fieldDefinitionsList);
        }

        private void AddFieldDefinationsForRouteParameters(IEnumerable<string> routeParams,
            List<MappingFieldDefinition> fieldDefinitionsList)
        {
            foreach (var routeParam in routeParams)
            {

                if (fieldDefinitionsList.All(t => !string.Equals(t.Source, routeParam, StringComparison.CurrentCultureIgnoreCase)))
                {
                    fieldDefinitionsList.Add(new MappingFieldDefinition
                    {
                        Source = routeParam,
                        Destination = routeParam,
                        Type = "string",
                        Required = true,
                        EndPoint = null,
                        GlobalLookupType = null
                    });
                }
            }
        }
    }
}