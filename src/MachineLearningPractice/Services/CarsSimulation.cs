using MachineLearningPractice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineLearningPractice.Services
{
    class CarsSimulation
    {
        const int SimulationCount = 20;
        const int SimulationAmountToKeepEachGeneration = SimulationCount / 10;

        private readonly Map map;
        private readonly Random random;

        public int CurrentGeneration { get; private set; }

        public HashSet<CarSimulation> AllSimulations { get; }
        public HashSet<CarSimulation> AliveSimulations { get; }
        public HashSet<CarSimulation> CrashedSimulations { get; }

        public CarsSimulation(
            Random random,
            Map map)
        {
            AliveSimulations = new HashSet<CarSimulation>();
            AllSimulations = new HashSet<CarSimulation>();
            CrashedSimulations = new HashSet<CarSimulation>();

            this.map = map;
            this.random = random;

            PrepareInitialSimulations();
        }

        private void PrepareInitialSimulations()
        {
            AliveSimulations.Clear();
            CrashedSimulations.Clear();
            AllSimulations.Clear();

            for (var i = 0; i < SimulationCount; i++)
            {
                AddNewSimulation(new CarSimulation(random, map, new CarNeuralNetwork(random)));
            }
        }

        private void AddNewSimulation(CarSimulation simulation)
        {
            AliveSimulations.Add(simulation);
            AllSimulations.Add(simulation);
        }

        private void OnGenerationCompleted()
        {
            CurrentGeneration++;

            RemoveWorstPerformingSimulations();

            foreach (var simulation in AllSimulations)
                simulation.ApplyTraining();

            while (AllSimulations.Count < SimulationCount)
                GenerateNewSimulation();
        }

        private void GenerateNewSimulation()
        {
            var crossOverSimulation = CrossTwoRandomSimulations();
            crossOverSimulation.Mutate();

            AddNewSimulation(crossOverSimulation);
        }

        private CarSimulation CrossTwoRandomSimulations()
        {
            var randomSimulation1 = PickRandomSimulation();
            var randomSimulation2 = PickRandomSimulation();

            var crossOverSimulation = randomSimulation1.CrossWith(randomSimulation2);
            return crossOverSimulation;
        }

        private CarSimulation PickRandomSimulation()
        {
            return AllSimulations
                .Skip(random.Next(0, AllSimulations.Count))
                .First();
        }

        private void RemoveWorstPerformingSimulations()
        {
            var simulationsToRemove = AllSimulations
                .OrderBy(x => x.Fitness)
                .Skip(SimulationAmountToKeepEachGeneration);

            foreach (var simulation in simulationsToRemove)
                RemoveExistingSimulation(simulation);
        }

        private void RemoveExistingSimulation(CarSimulation simulation)
        {
            AllSimulations.Remove(simulation);
            CrashedSimulations.Remove(simulation);
            AliveSimulations.Remove(simulation);
        }

        public void SimulateTick()
        {
            var newlyCrashedSimulations = new HashSet<CarSimulation>();
            foreach (var simulation in AliveSimulations.AsParallel())
            {
                simulation.Tick();

                if (simulation.IsCrashed)
                    newlyCrashedSimulations.Add(simulation);
            }

            foreach(var simulation in newlyCrashedSimulations) { 
                AliveSimulations.Remove(simulation);
                CrashedSimulations.Add(simulation);
            }
        }

        public void SimulateWholeGeneration(Action onPostTick = null)
        {
            foreach (var simulation in AllSimulations) { 
                simulation.Reset();

                AliveSimulations.Add(simulation);
                CrashedSimulations.Remove(simulation);
            }

            while (AliveSimulations.Count > 0)
            {
                SimulateTick();
                onPostTick?.Invoke();
            }

            OnGenerationCompleted();
        }

        //private void TrainPendingInstructionsFromBestGeneration()
        //{
        //    var instructions = currentBestSimulationsInGeneration
        //        .SelectMany(x => x
        //            .PendingTrainingInstructions
        //            .Take(x.PendingTrainingInstructions.Count / currentBestSimulationsInGeneration.Count));

        //    foreach (var pendingTrainingInstruction in instructions)
        //    {
        //        carNeuralNetwork.Record(
        //            pendingTrainingInstruction.CarSensorReading,
        //            pendingTrainingInstruction.CarResponse);
        //    }

        //    carNeuralNetwork.Train();

        //    generation++;
        //}
    }
}
