using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;

namespace Paycor.Import.Tests.Extensions
{
    /// <summary>
    /// Summary description for EnumerableExtensionsTest
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class EnumerableExtensionsTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void TestEnumerableExtension_DestinationFieldsContainDuplicates_ForDups()
        {
            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {
                    new MappingFieldDefinition
                    {
                        Destination = "clientId",
                        Source = "clientId"
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "FirstName",
                        Source = "First",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "LastName",
                        Source = "Last",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "clientId",
                        Source = "clientId"
                    }
                }
            };

            var actual = mappingDefinition.FieldDefinitions.ContainDuplicates(o => o.Destination,
                StringComparer.OrdinalIgnoreCase);

            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void TestEnumerableExtension_DestinationFieldsContainDuplicates_ForDupsIgnoreCase()
        {
            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {
                    new MappingFieldDefinition
                    {
                        Destination = "clientId",
                        Source = "clientId"
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "FirstName",
                        Source = "First",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "LastName",
                        Source = "Last",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "firstname",
                        Source = "firstname",
                    },
                }
            };

            var actual = mappingDefinition.FieldDefinitions.ContainDuplicates(o => o.Destination,
                StringComparer.OrdinalIgnoreCase);

            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void TestEnumerableExtension_DestinationFieldsContainDuplicates_ForNoDups()
        {
            var mappingDefinition = new MappingDefinition
            {
                FieldDefinitions = new List<MappingFieldDefinition>
                {
                    new MappingFieldDefinition
                    {
                        Destination = "clientId",
                        Source = "clientId"
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "FirstName",
                        Source = "Firt",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "LastName",
                        Source = "Last",
                    },
                    new MappingFieldDefinition
                    {
                        Destination = "employeeNumber",
                        Source = "employeeNumber"
                    }
                }
            };

            var actual = mappingDefinition.FieldDefinitions.ContainDuplicates(o => o.Destination,
                StringComparer.OrdinalIgnoreCase);

            Assert.AreEqual(false, actual);
        }

    }
}