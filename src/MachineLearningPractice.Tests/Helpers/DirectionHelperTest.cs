using MachineLearningPractice.Helpers;
using MachineLearningPractice.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLearningPractice.Tests.Helpers
{
    [TestClass]
    public class DirectionHelperTest
    {
        [TestMethod]
        public void IsPointInDirectionOfSensorLineWorks()
        {
            Assert.IsTrue(DirectionHelper
                .IsPointInDirectionOfSensorLine(
                    new Line()
                    {
                        Start = new Point(-1, -1),
                        End = new Point(1, 1)
                    },
                    new Point(0.5m, 0.5m)));

            Assert.IsTrue(DirectionHelper
                .IsPointInDirectionOfSensorLine(
                    new Line()
                    {
                        Start = new Point(0, 0),
                        End = new Point(0.1m, 0.1m)
                    },
                    new Point(0.5m, 0.5m)));
        }

        [TestMethod]
        public void AngleBetweenDirectionsWorks()
        {
            Assert.AreEqual(45, DirectionHelper.GetClockwiseAngleBetweenDirections(Direction.Top, Direction.TopRight));
            Assert.AreEqual(90, DirectionHelper.GetClockwiseAngleBetweenDirections(Direction.Top, Direction.Right));
        }

        [TestMethod]
        public void CombinedDirectionWorks()
        {
            Assert.AreEqual(Direction.BottomRight, DirectionHelper.GetCombinedDirection(Direction.Bottom, Direction.Right));
            Assert.AreEqual(Direction.TopLeft, DirectionHelper.GetCombinedDirection(Direction.Left, Direction.Top));
        }
    }
}
