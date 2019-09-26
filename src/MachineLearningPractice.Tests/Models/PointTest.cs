using Microsoft.VisualStudio.TestTools.UnitTesting;
using MachineLearningPractice.Models;

namespace MachineLearningPractice.Tests.Models
{
    [TestClass]
    public class PointTest
    {
        [TestMethod]
        public void RotatePointWorks()
        {
            var point = new Point(-1, -1);
            var centerPoint = new Point(0, 0);
            var rotatedPoint = point.RotateAround(centerPoint, 90);

            var delta = new Point(1, -1) - rotatedPoint;
            Assert.IsTrue(delta.X < 0.001);
            Assert.IsTrue(delta.Y < 0.001);
        }
    }
}
