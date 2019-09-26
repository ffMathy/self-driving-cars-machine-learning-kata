using Microsoft.VisualStudio.TestTools.UnitTesting;
using MachineLearningPractice.Models;

namespace MachineLearningPractice.Tests.Models
{
    [TestClass]
    public class LineTest
    {
        [TestMethod]
        public void GetIntersectionPointWorks()
        {
            var line1 = new Line() {
                Start = new Point(-10, 7),
                End = new Point(10, 7)
            };

            var line2 = new Line() {
                Start = new Point(5, -10),
                End = new Point(5, 10)
            };

            var intersectionPoint = line1.GetIntersectionPointWith(line2);

            Assert.AreEqual(
                new Point(5, 7),
                intersectionPoint);
        }

        [TestMethod]
        public void RotateAroundZeroCenterWorks()
        {
            //TODO: make more tests that try to rotate along a different center point - perhaps with a car involved? perhaps even a car test?

            var line = new Line() {
                Start = new Point(-1, -1),
                End = new Point(1, 1)
            };

            var rotatedLine = line.Rotate(-45);

            Assert.AreEqual(
                new Line() {
                    Start = new Point(0, -1),
                    End = new Point(0, 1)
                },
                rotatedLine);
        }

        [TestMethod]
        public void RotateAroundComplexCenterWorks()
        {
            //TODO: make more tests that try to rotate along a different center point - perhaps with a car involved? perhaps even a car test?

            var line = new Line()
            {
                Start = new Point(-2, -4),
                End = new Point(1, -1)
            };

            var rotatedLine = line.Rotate(-90);

            Assert.AreEqual(
                new Line()
                {
                    Start = new Point(-2, -1),
                    End = new Point(1, -4)
                },
                rotatedLine);
        }
    }
}