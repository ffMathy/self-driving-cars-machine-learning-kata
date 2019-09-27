using Microsoft.VisualStudio.TestTools.UnitTesting;
using MachineLearningPractice.Models;

namespace MachineLearningPractice.Tests.Models
{
    [TestClass]
    public class CarTest
    {
        [TestMethod]
        public void ForwardDirectionWorks()
        {
            var car = new Car(
                new Map(),
                100,
                100);

            var direction = car.ForwardDirectionLine;

            Assert.AreEqual(
                new Point(50, 100),
                direction.End);

            Assert.AreEqual(
                new Point(50, 0),
                direction.Start);
        }
    }
}
