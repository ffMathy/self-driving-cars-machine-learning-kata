using Microsoft.VisualStudio.TestTools.UnitTesting;
using MachineLearningPractice.Models;
using MachineLearningPractice.Services;
using MachineLearningPractice.Helpers;
using System;

namespace MachineLearningPractice.Tests.Models
{
    [TestClass]
    public class CarTest
    {
        private Map GenerateCircularMap()
        {
            var mapBuilder = new MapBuilder();
            return mapBuilder
                .MoveInDirection(Direction.Top)
                .MoveInDirection(Direction.Left)
                .MoveInDirection(Direction.Bottom)
                .MoveInDirection(Direction.Bottom)
                .MoveInDirection(Direction.Right)
                .MoveInDirection(Direction.Top)
                .Build();
        }

        [TestMethod]
        public void CarInCenteredBoxShowsCorrectSensorReadings()
        {
            var random = new Random();
            var carNeuralNetwork = new CarNeuralNetwork();

            var map = GenerateCircularMap();

            var carSimulation = new CarSimulation(
                random,
                map,
                carNeuralNetwork,
                0);

            var readings = carSimulation.GetSensorReadings();
            Assert.IsNotNull(readings.LeftSensor);
            Assert.IsNotNull(readings.CenterSensor);
            Assert.IsNotNull(readings.RightSensor);
        }
    }
}
