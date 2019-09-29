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

            Assert.IsTrue(MathHelper.IsEqualWithinRange(readings.LeftSensor.Value.IntersectionPoint.X, -50, 0.5));
            Assert.IsTrue(MathHelper.IsEqualWithinRange(readings.LeftSensor.Value.IntersectionPoint.Y, -50, 0.5));

            Assert.IsTrue(MathHelper.IsEqualWithinRange(readings.CenterSensor.Value.IntersectionPoint.X, 0, 0.5));
            Assert.IsTrue(MathHelper.IsEqualWithinRange(readings.CenterSensor.Value.IntersectionPoint.Y, -150, 0.5));

            Assert.IsTrue(MathHelper.IsEqualWithinRange(readings.RightSensor.Value.IntersectionPoint.X, 50, 0.5));
            Assert.IsTrue(MathHelper.IsEqualWithinRange(readings.RightSensor.Value.IntersectionPoint.Y, -50, 0.5));
        }

        [TestMethod]
        public void CarAboveCenteredBoxShowsCorrectSensorReadings()
        {
            var random = new Random();
            var carNeuralNetwork = new CarNeuralNetwork();

            var map = GenerateCircularMap();

            var carSimulation = new CarSimulation(
                random,
                map,
                carNeuralNetwork,
                0);

            carSimulation.Car.Accelerate(10);
            carSimulation.Car.Tick();

            var readings = carSimulation.GetSensorReadings();

            Assert.IsNotNull(readings.LeftSensor);
            Assert.IsNotNull(readings.CenterSensor);
            Assert.IsNotNull(readings.RightSensor);

            Assert.IsTrue(MathHelper.IsEqualWithinRange(readings.LeftSensor.Value.IntersectionPoint.X, -140, 0.5));
            Assert.IsTrue(MathHelper.IsEqualWithinRange(readings.LeftSensor.Value.IntersectionPoint.Y, -150, 0.5));

            Assert.IsTrue(MathHelper.IsEqualWithinRange(readings.CenterSensor.Value.IntersectionPoint.X, 0, 0.5));
            Assert.IsTrue(MathHelper.IsEqualWithinRange(readings.CenterSensor.Value.IntersectionPoint.Y, -150, 0.5));

            Assert.IsTrue(MathHelper.IsEqualWithinRange(readings.RightSensor.Value.IntersectionPoint.X, 50, 0.5));
            Assert.IsTrue(MathHelper.IsEqualWithinRange(readings.RightSensor.Value.IntersectionPoint.Y, -60, 0.5));
        }
    }
}
