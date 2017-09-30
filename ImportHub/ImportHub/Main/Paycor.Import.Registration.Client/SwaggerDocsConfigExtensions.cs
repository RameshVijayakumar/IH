using Swashbuckle.Application;

namespace Paycor.Import.Registration.Client
{
    public static class SwaggerDocsConfigExtension
    {
        public static void ConfigureForImportHub(this SwaggerDocsConfig config)
        {
            config.DescribeAllEnumsAsStrings(true);
            config.SchemaFilter<AddMetaDescriptionToSchemaFilter>();
            config.SchemaFilter<ChunkSizeFilter>();
            config.SchemaFilter<FieldDescriptionSchemaFilter>();
            config.SchemaFilter<IgnorePropertySchemaFilter>();
            config.SchemaFilter<LookupRouteSchemaFilter>();
            config.OperationFilter<BatchChunkingSupportFilter>();
            config.OperationFilter<OptInFilter>();
        }
    }
}
