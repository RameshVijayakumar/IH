using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Paycor.Import.Mapping;
using System.Collections.Generic;

// ReSharper disable PossibleMultipleEnumeration

namespace Paycor.Import.Registration.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SwaggerParserTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void ReadValidJsonWithRouteParameterForApiMappings()
        {
            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerParser = new SwaggerParser(mappingFactory);
            var apiMappings = swaggerParser.GetAllApiMappings(File.ReadAllText("json\\GamesSwaggerV3.json"));

            Assert.IsTrue(apiMappings.Count() == 6);
        }
        [TestMethod]
        public void ReadValidJsonForApiMappings()
        {
            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerApiMappingsParser = new SwaggerParser(mappingFactory);
            var apiMappings = swaggerApiMappingsParser.GetAllApiMappings(File.ReadAllText("json\\GamesSwagger.json"));

            Assert.IsTrue(apiMappings.Count() == 2);
        }

        [TestMethod]
        public void ValidateMapNameFromSwaggerExtension()
        {
            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerApiMappingsParser = new SwaggerParser(mappingFactory);

            var apiMappings = swaggerApiMappingsParser.GetAllApiMappings(File.ReadAllText("json\\MapTypeExample.json"));

            var actualNames = apiMappings.Select(x => x.MappingName).ToArray();
            var expectedNames = new[]
            {
                "Drivers",
                "Drivers",
                "Events",
                "Game",
                "Teams",
                "Venues"
            };
            for (int i = 0; i < expectedNames.Length; i++)
            {
                Assert.AreEqual(expectedNames[i], actualNames[i]);
            }
            Assert.IsTrue(expectedNames.Length == actualNames.Length);
        }

        [TestMethod]
        public void ReadInValidJsonForApiMappings()
        {
            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerApiMappingsParser = new SwaggerParser(mappingFactory);
            var apiMappings = swaggerApiMappingsParser.GetAllApiMappings(File.ReadAllText("json\\InvalidGamejson.json"));
            Assert.IsTrue(!apiMappings.Any());
        }
        [TestMethod]
        public void Read_Games_Venue_DriverForApiMappings()
        {
            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerApiMappingsParser = new SwaggerParser(mappingFactory);
            var apiMappings = swaggerApiMappingsParser.GetAllApiMappings(File.ReadAllText("json\\GamesVenueDriver.json"));
            Assert.IsTrue(apiMappings.Count() == 6);
        }

        [TestMethod]
        public void Read_Games_Venue_DriverForApiMappings_With_Required_lookupRoutes()
        {

            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerApiMappingsParser = new SwaggerParser(mappingFactory);
            var apiMappings = swaggerApiMappingsParser.GetAllApiMappings(File.ReadAllText("json\\GamesVenueDriverWithRequiredLookup.json"));

            Assert.IsTrue(apiMappings.Count() == 6);

            Assert.AreEqual(apiMappings.ElementAt(0).Mapping.FieldDefinitions.Where(t => t.Source == "driverId" && t.Destination
                    == "driverId").Select(t => t.Source).FirstOrDefault(), "driverId");

            Assert.AreEqual(apiMappings.ElementAt(0).Mapping.FieldDefinitions.Where(t => t.Source == "{lastName}" && t.Destination
                == "{driverId}").Select(t => t.Source).FirstOrDefault(), "{lastName}");
            Assert.AreEqual(apiMappings.ElementAt(0).Mapping.FieldDefinitions.Where(t => t.Source == "{lastName}" && t.Destination
                == "{driverId}").Select(t => t.Destination).FirstOrDefault(), "{driverId}");

            Assert.AreEqual(apiMappings.ElementAt(1).Mapping.FieldDefinitions.Where(t => t.Source == "{firstName}" && t.Destination
                == "{driverId}").Select(t => t.Source).FirstOrDefault(), "{firstName}");
            Assert.AreEqual(apiMappings.ElementAt(1).Mapping.FieldDefinitions.Where(t => t.Source == "{firstName}" && t.Destination
                == "{driverId}").Select(t => t.Destination).FirstOrDefault(), "{driverId}");

        }

        [TestMethod]
        public void Read_Games_Venue_DriverForApiMappings_With_Multiple_Required_lookupRoutes()
        {

            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerParser = new SwaggerParser(mappingFactory);
            var apiMappings = swaggerParser.GetAllApiMappings(File.ReadAllText("json\\GamesVenueDriverWithMultipleRequiredLookup.json"));

            Assert.IsTrue(apiMappings.Count() == 6);


            Assert.AreEqual(apiMappings.ElementAt(0).Mapping.FieldDefinitions.Where(t => t.Source == "driverId" && t.Destination
                    == "driverId").Select(t => t.Source).FirstOrDefault(), "driverId");
            Assert.AreEqual(apiMappings.ElementAt(0).Mapping.FieldDefinitions.Where(t => t.Source == "teamId" && t.Destination
                == "teamId").Select(t => t.Source).FirstOrDefault(), "teamId");

            Assert.AreEqual(apiMappings.ElementAt(1).Mapping.FieldDefinitions.Where(t => t.Source == "{lastName}" && t.Destination
                == "{driverId}").Select(t => t.Source).FirstOrDefault(), "{lastName}");
            Assert.AreEqual(apiMappings.ElementAt(1).Mapping.FieldDefinitions.Where(t => t.Source == "{firstName}" && t.Destination
            == "{driverId}").Select(t => t.Source).FirstOrDefault(), "{firstName}");

            Assert.AreEqual(apiMappings.ElementAt(1).Mapping.FieldDefinitions.Where(t => t.Source == "{lastName}" && t.Destination
                == "{driverId}").Select(t => t.Destination).FirstOrDefault(), "{driverId}");

            Assert.AreEqual(apiMappings.ElementAt(1).Mapping.FieldDefinitions.Where(t => t.Source == "{racingNumber}" && t.Destination
               == "{driverId}").Select(t => t.Destination).FirstOrDefault(), "{driverId}");

        }
        [TestMethod]
        public void Read_Array_Mapping_Json()
     {
            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerParser = new SwaggerParser(mappingFactory);
            var apiMappings = swaggerParser.GetAllApiMappings(File.ReadAllText("json\\ArrayMapping.json"));

            Assert.IsTrue(apiMappings.Count() == 3);
        }

        [TestMethod]
        public void ReadHugeJsonForApiMappings()
        {

            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerParser = new SwaggerParser(mappingFactory);

            var apiMappings = swaggerParser.GetAllApiMappings(File.ReadAllText("json\\time.json"));

            Assert.IsTrue(apiMappings.Count() == 31);
        }

        [TestMethod]
        public void ReadValidJsonWithLookupRouteParameterForApiMappings()
        {
            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerParser = new SwaggerParser(mappingFactory);

            var apiMappings = swaggerParser.GetAllApiMappings(File.ReadAllText("json\\AttributedSwagger.json"));

            Assert.IsTrue(apiMappings.FirstOrDefault().ChunkSize == 23);
            Assert.IsTrue(apiMappings.Count() == 1);
        }


        [TestMethod]
        public void ValidJsonWithLookupRouteParameterForApiMappings()
        {
            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerParser = new SwaggerParser(mappingFactory);

            var apiMappings = swaggerParser.GetAllApiMappings(File.ReadAllText("json\\GamesSwaggerV2.json"));

            Assert.IsTrue(apiMappings.FirstOrDefault().ChunkSize == 0);
            Assert.IsTrue(apiMappings.Count() == 6);
            Assert.IsTrue(apiMappings.First().Mapping.FieldDefinitions.Count() == 10);
            Assert.IsTrue(apiMappings.ElementAt(2).Mapping.FieldDefinitions.Count() == 5);
        }

        [TestMethod]
        public void SubArrayMappingWithThreeInstancesTest()
        {
            var mappingFactory = new RegistrationMappingGeneratorFactory();
            var swaggerParser = new SwaggerParser(mappingFactory);
           
            var apiMappings = swaggerParser.GetAllApiMappings(File.ReadAllText("json\\SubArrayWithThreeInstance.json"));

            var fieldDefinitions = apiMappings.ElementAt(0).Mapping.FieldDefinitions;
            var subArrayFieldDefinitions = fieldDefinitions.Where(t => t.Destination.Contains("[")).Select(t => t);
            Assert.IsTrue(apiMappings.Count() == 1);
            Assert.IsTrue(subArrayFieldDefinitions.Count() == 16);

        }

        [TestMethod]
        public void GetAllRegisteredDocUrls()
        {
            var text = File.ReadAllText("json\\RC-Maps.json");
            var allmappings = JsonConvert.DeserializeObject<GeneratedMapping[]>(text);
            var docUrls = allmappings.Where(t => t.DocUrl != null && !t.DocUrl.Contains("importhubrefservice/swagger/docs")).Select(t => t.DocUrl).Distinct().ToList();
        }

        [TestMethod]
        public void ParseGuidedMap()
        {
            var text = File.ReadAllText("json\\GuidedEmployeeMap.json");
            var guidedMap = JsonConvert.DeserializeObject<GeneratedMapping>(text);
            var dictionary = new Dictionary<string, string>();
            foreach (var item in guidedMap.Mapping.FieldDefinitions)
            {
                dictionary.Add(item.Source, "1");
                var handler = SourceTypeHandlerFactory.HandleSourceType(item);
                var output = handler.Resolve(item, item.Source, dictionary);

                Assert.AreEqual("1", output);
            }
        }
    }
}
