using Microsoft.VisualStudio.TestTools.UnitTesting;
using MachineLearningPractice.Models;

namespace MachineLearningPractice.Tests.Models
{
    [TestClass]
    public class LineTest
    {
        [TestMethod]
        public void ZeroCenterWorks()
        {
            var line = new Line()
            {
                Start = new Point(-1, -1),
                End = new Point(1, 1)
            };

            Assert.AreEqual(
                new Point(0, 0),
                line.Center);
        }

        [TestMethod]
        public void GetAngleBetweenWorks()
        {
            var line1 = new Line()
            {
                Start = new Point(-1, -1),
                End = new Point(1, 1)
            };

            var line2 = new Line()
            {
                Start = new Point(-1, 1),
                End = new Point(1, -1)
            };

            Assert.AreEqual(
                90,
                line1.GetAngleTo(line2));
        }

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
            var line = new Line() {
                Start = new Point(-1, -1),
                End = new Point(1, 1)
            };

            var rotatedLine = line.Rotate(90);

            var destinationLine = new Line()
            {
                Start = new Point(1, -1),
                End = new Point(-1, 1)
            };

            var deltaStart = destinationLine.Start - rotatedLine.Start;
            Assert.IsTrue(deltaStart.X < 0.001);
            Assert.IsTrue(deltaStart.Y < 0.001);

            var deltaEnd = destinationLine.End - rotatedLine.End;
            Assert.IsTrue(deltaEnd.X < 0.001);
            Assert.IsTrue(deltaEnd.Y < 0.001);
        }

        [TestMethod]
        public void RotateAroundComplexCenterWorks()
        {
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
