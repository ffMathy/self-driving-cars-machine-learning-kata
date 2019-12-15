using Microsoft.VisualStudio.TestTools.UnitTesting;
using MachineLearningPractice.Models;
using MachineLearningPractice.Services;
using MachineLearningPractice.Helpers;
using System;
using System.Linq;

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
        public void Fitness_GoodSimulationIsFurtherOnMap_GoodSimulationHasSmallerFitness()
        {
            var map = GenerateCircularMap();

            var badSimulation = new CarSimulation(map);
            badSimulation.CurrentProgressLine = map.Nodes
                .SelectMany(x => x.ProgressLines)
                .Skip(2)
                .First();

            var goodSimulation = new CarSimulation(map);
            goodSimulation.CurrentProgressLine = map.Nodes
                .SelectMany(x => x.ProgressLines)
                .Skip(4)
                .First();

            Assert.IsTrue(goodSimulation.Fitness < badSimulation.Fitness);
        }

        [TestMethod]
        public void Fitness_GoodSimulationHasSurvivedLonger_GoodSimulationHasSmallerFitness()
        {
            var map = GenerateCircularMap();

            var badSimulation = new CarSimulation(map);
            badSimulation.TicksSurvived = 10;

            var goodSimulation = new CarSimulation(map);
            goodSimulation.TicksSurvived = 20;

            Assert.IsTrue(goodSimulation.Fitness < badSimulation.Fitness);
        }
    }
}
