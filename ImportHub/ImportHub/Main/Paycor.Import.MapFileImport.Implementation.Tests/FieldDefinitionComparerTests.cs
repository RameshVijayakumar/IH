using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Mapping;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FieldDefinitionComparerTests
    {
        [TestMethod]
        public void FieldDefinitionComparer_Equals()
        {
            var md1 = new MappingFieldDefinition();
            var md2 = new MappingFieldDefinition();
            var comparer = new FieldDefinitionComparer();
            Assert.IsTrue(comparer.Equals(md1, md2));

            md1.Source = "X";
            md2.Source = "Y";
            Assert.IsTrue(comparer.Equals(md1, md2));

            md1.Destination = "A";
            md2.Destination = "B";
            Assert.IsFalse(comparer.Equals(md1, md2));

            md1.EndPoint = "Alpha";
            md2.EndPoint = "Beta";
            Assert.IsFalse(comparer.Equals(md1, md2));

            md1.Destination = "b";
            md1.EndPoint = "beta";
            Assert.IsTrue(comparer.Equals(md1, md2));

            Assert.IsFalse(comparer.Equals(null, md2));
            Assert.IsFalse(comparer.Equals(md1, null));
            Assert.IsTrue(comparer.Equals(null, null));

        }
    }
}
