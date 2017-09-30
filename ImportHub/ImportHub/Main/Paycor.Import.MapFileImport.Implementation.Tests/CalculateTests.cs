using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.MapFileImport.Implementation.Shared;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class CalculateTests
    {
        [TestClass]
        public class Calculate_GetChunkStepPercent_Tests
        {

            [TestMethod]
            public void Calculate_OneChunkTest()
            {
                var numChunks = 1;
                var calc = new Calculate();
                var actual = calc.GetChunkStepPercent(StepNameEnum.Chunker, numChunks);
                Assert.AreEqual(10, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Builder, numChunks, actual);
                Assert.AreEqual(50, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Preparer, numChunks, actual);
                Assert.AreEqual(60, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Sender, numChunks, actual);
                Assert.AreEqual(100, actual);
            }

            [TestMethod]
            public void Caclulate_TwoChunkTest()
            {
                var numChunks = 2;
                var calc = new Calculate();
                var actual = calc.GetChunkStepPercent(StepNameEnum.Chunker, numChunks);
                Assert.AreEqual(10, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Builder, numChunks, actual);
                Assert.AreEqual(30, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Preparer, numChunks, actual);
                Assert.AreEqual(35, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Sender, numChunks, actual);
                Assert.AreEqual(55, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Builder, numChunks, actual);
                Assert.AreEqual(75, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Preparer, numChunks, actual);
                Assert.AreEqual(80, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Sender, numChunks, actual);
                Assert.AreEqual(100, actual);

                actual = calc.GetChunkStepPercent((StepNameEnum)4, numChunks, actual);
                Assert.AreEqual(100, actual);
            }

            [TestMethod]
            public void Caclulate_FourChunkTest()
            {
                var expecteds = new double[]
                {
                    10, 20, 22.5, 32.5, 42.5, 45, 55, 65, 67.5, 77.5, 87.5, 90, 100
                };
                var numChunks = 4;
                var calc = new Calculate();
                var actual = calc.GetChunkStepPercent(StepNameEnum.Chunker, numChunks);
                Assert.AreEqual(10, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Builder, numChunks, actual);
                Assert.AreEqual(20, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Preparer, numChunks, actual);
                Assert.AreEqual(22.5, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Sender, numChunks, actual);
                Assert.AreEqual(32.5, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Builder, numChunks, actual);
                Assert.AreEqual(42.5, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Preparer, numChunks, actual);
                Assert.AreEqual(45, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Sender, numChunks, actual);
                Assert.AreEqual(55, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Builder, numChunks, actual);
                Assert.AreEqual(65, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Preparer, numChunks, actual);
                Assert.AreEqual(67.5, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Sender, numChunks, actual);
                Assert.AreEqual(77.5, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Builder, numChunks, actual);
                Assert.AreEqual(87.5, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Preparer, numChunks, actual);
                Assert.AreEqual(90, actual);

                actual = calc.GetChunkStepPercent(StepNameEnum.Sender, numChunks, actual);
                Assert.AreEqual(100, actual);
            }
        }
    }
}
