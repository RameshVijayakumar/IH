using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.MapFileImport.Implementation.Reporter;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [TestClass]
    public class ProvideClientDataTest

    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }
        [TestMethod]
        public void GetAllClientIds_ForSingleSheet_Test()
        {
            var providerForClientData = new ProvideClientData();
            var clientIds = providerForClientData.GetAllClientIds(new ChunkDataResponse()
            {
                Chunks = new List<List<IDictionary<string, string>>>
                {
                            new List<IDictionary<string, string>>
                            {
                                new Dictionary<string, string>
                                {
                                    {"clientId", "102"},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                },
                                new Dictionary<string, string>
                                {
                                   {"clientId", "103"},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"clientId", "102"},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                }
                            }
                        }
                });

            Assert.AreEqual("102,103,102",string.Join(",", clientIds));

            clientIds = providerForClientData.GetAllClientIds(new ChunkDataResponse
            {
                Chunks = new List<List<IDictionary<string, string>>>
                {
                            new List<IDictionary<string, string>>
                            {
                                new Dictionary<string, string>
                                {
                                    {"clientId", "102"},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                },
                                new Dictionary<string, string>
                                {
                                   {"clientId", null},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"clientId", "102"},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                }
                            }
                        }
            });

            Assert.AreEqual("102,102", string.Join(",", clientIds));

            clientIds = providerForClientData.GetAllClientIds(new ChunkDataResponse()
            {
                Chunks = new List<List<IDictionary<string, string>>>
                {
                            new List<IDictionary<string, string>>
                            {
                                new Dictionary<string, string>
                                {
                                    {"clientId", "102"},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                },
                                new Dictionary<string, string>
                                {
                                   {"clientId", ""},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                },
                                new Dictionary<string, string>
                                {
                                    {"clientId", "104"},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                }
                            }
                        }
            });

            Assert.AreEqual("102,104", string.Join(",", clientIds));

            clientIds = providerForClientData.GetAllClientIds(new BuildDataSourceResponse());

            Assert.AreEqual(0, clientIds.Count());
        }

        [TestMethod]
        public void GetAllClientIds_ForMultiSheet_Test()
        {
            var providerForClientData = new ProvideClientData();
            var mappings = new[]
               {
                    new GeneratedMapping
                    {
                        DocUrl = "SwaggerDocUrl",
                        IsBatchSupported = false,
                        IsBatchChunkingSupported = false,
                        MappingName = "Sheet 1",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        },
                        ObjectType = "Employee",
                        Mapping = new MappingDefinition()
                    },
                    new GeneratedMapping
                    {
                        DocUrl = "SwaggerDocUrl",
                        IsBatchSupported = false,
                        IsBatchChunkingSupported = false,
                        MappingName = "Sheet 2",
                        MappingEndpoints = new MappingEndpoints
                        {
                            Delete = "DeleteRoute",
                            Post = "PostRoute"
                        },
                        ObjectType = "EmployeeDeduction",
                        Mapping = new MappingDefinition()
                    }
                };

            var clientIds = providerForClientData.GetAllClientIds(
            
                new ChunkMultiDataResponse()
                    {
                        Status = Status.Success,
                        Chunks = new List<KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>>
                        {
                            new KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>(
                                mappings[0],
                                new List<IDictionary<string, string>>
                                {
                                    new Dictionary<string, string>
                                    {
                                          {"clientId", "102"},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                          {"clientId", "103"},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                        {"clientId", null},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                    }
                                }),
                            new KeyValuePair<ApiMapping, IList<IDictionary<string, string>>>(
                                mappings[1],
                                new List<IDictionary<string, string>>
                                {
                                    new Dictionary<string, string>
                                    {
                                         {"clientId", "104"},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                       {"clientId", "105"},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                    },
                                    new Dictionary<string, string>
                                    {
                                    {"clientId", "102"},
                                    {"empId", "12"},
                                    {"TaxId", "12"}
                                    }
                                })
                        }
                    });

            Assert.AreEqual("102,103,104,105,102", string.Join(",", clientIds));
            Assert.AreEqual(5, clientIds.Count());
        }
    }
}
