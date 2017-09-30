using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Mapping;
using SourceFieldTransformer = Paycor.Import.MapFileImport.Implementation.Transformers.SourceFieldTransformer;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SourceFieldTransformerTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestClass]
        public class TransformRecordFieldsTests
        {
            [TestMethod]
            public void TransformRecordFields_NoIssues()
            {
                var mapping = new MappingDefinition()
                {
                    FieldDefinitions = new[]
                    {
                        new MappingFieldDefinition
                        {
                            Source = "Column A",
                            Destination = "FieldA",
                            SourceType = SourceTypeEnum.File,
                        },
                        new MappingFieldDefinition
                        {
                            Source = "Column B",
                            Destination = "FieldB",
                            SourceType = SourceTypeEnum.File
                        },
                        new MappingFieldDefinition
                        {
                            Source = "Constant Value",
                            Destination = "FieldC",
                            SourceType = SourceTypeEnum.Const
                        }
                    }
                };
                var masterSession = Guid.NewGuid().ToString();

                var record = new Dictionary<string, string>
                {
                    {"ColumnA", "Value A"},
                    {"ColumnB", "Value B"},
                    {"ColumnC", "BAD"},
                    {"ColumnD", "Not transformed"}
                };

                var transformer = new SourceFieldTransformer();
                var payload = transformer.TransformRecordFields(mapping, masterSession, record).ToList();
                Assert.AreEqual(payload[0].Key, "FieldA");
                Assert.AreEqual(payload[0].Value, "Value A");
                Assert.AreEqual(payload[1].Key, "FieldB");
                Assert.AreEqual(payload[1].Value, "Value B");
                Assert.AreEqual(payload[2].Key, "FieldC");
                Assert.AreEqual(payload[2].Value, "Constant Value");
                Assert.AreEqual(3, payload.Count);
            }

            [TestMethod]
            public void TransformRecordFields_ValidationFails()
            {
                var mapping = new MappingDefinition();
                var masterSession = Guid.NewGuid().ToString();

                var record = new Dictionary<string, string>
                {
                    {"Column A", "Value A"},
                    {"Column B", "Value B"},
                    {"Column C", "BAD"},
                    {"Column D", "Not transformed"}
                };

                var transformer = new SourceFieldTransformer();
                var payload = transformer.TransformRecordFields(mapping, masterSession, record).ToList();
                Assert.AreEqual(0, payload.Count);
            }

        }
    }
}
