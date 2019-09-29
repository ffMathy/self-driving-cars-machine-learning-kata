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
                    new Point(0.5, 0.5)));

            Assert.IsTrue(DirectionHelper
                .IsPointInDirectionOfSensorLine(
                    new Line()
                    {
                        Start = new Point(0, 0),
                        End = new Point(0.1, 0.1)
                    },
                    new Point(0.5, 0.5)));
        }
    }
}
