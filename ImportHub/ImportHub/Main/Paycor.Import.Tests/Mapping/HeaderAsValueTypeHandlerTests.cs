using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Mapping;

namespace Paycor.Import.Tests.Mapping
{
    [TestClass]
    public class HeaderAsValueTypeHandlerTests
    {
        [TestClass]
        public class ResolveTests
        {
            private readonly MappingFieldDefinition _mappingFieldDefinition = new MappingFieldDefinition
            {
                Source = nameof(MappingFieldDefinition.Source),
                Destination = nameof(MappingFieldDefinition.Destination),
                EndPoint = nameof(MappingFieldDefinition.EndPoint),
                HeadingDestination = nameof(MappingFieldDefinition.HeadingDestination),
                SourceType = SourceTypeEnum.HeaderAsValue,
                ExceptionMessage = nameof(MappingFieldDefinition.ExceptionMessage),
                GlobalLookupType = nameof(MappingFieldDefinition.GlobalLookupType),
                IsRequiredForPayload = false,
                Required = false,
                Type = nameof(MappingFieldDefinition.Type),
                ValuePath = nameof(MappingFieldDefinition.ValuePath)
            };

            private readonly Dictionary<string, string> _records = new Dictionary<string, string>
            {
                { nameof(MappingFieldDefinition.Source), "mySource" },
                { nameof(MappingFieldDefinition.Destination), "myDest" },
                { nameof(MappingFieldDefinition.EndPoint), "myEndpoint" },
                { nameof(MappingFieldDefinition.HeadingDestination), "myHeadingDest" }
            };
            private readonly HeaderAsValueTypeHandler _typeHandlerUnderTest = new HeaderAsValueTypeHandler();

            [TestMethod]
            public void EnsureNullOnNullField()
            {
                var actual = _typeHandlerUnderTest.Resolve(_mappingFieldDefinition, null, _records);
                Assert.IsNull(actual);
            }

            [TestMethod]
            public void EnsureNullOnWhiteField()
            {
                var actual = _typeHandlerUnderTest.Resolve(_mappingFieldDefinition, "    ", _records);
                Assert.IsNull(actual);
            }

            [TestMethod]
            public void EnsureNullOnOrdinalField()
            {
                var actual = _typeHandlerUnderTest.Resolve(_mappingFieldDefinition, "4", _records);
                Assert.IsNull(actual);
            }

            [TestMethod]
            public void EnsureReturnsSource()
            {
                var expected = "mySource";

                var actual = _typeHandlerUnderTest.Resolve(_mappingFieldDefinition,
                    nameof(MappingFieldDefinition.Source), _records);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
