using MachineLearningPractice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningPractice.Services
{
    class CarSimulation
    {
        private readonly Car car;
        private readonly Random random;
        private readonly Map map;
        private readonly CarNeuralNetwork carNeuralNetwork;

        private readonly List<CarSimulationTick> pendingTrainingInstructions;

        private readonly double randomnessFactor;

        private ulong ticksSurvived;

        public Car Car => car;

        public IReadOnlyList<CarSimulationTick> PendingTrainingInstructions => pendingTrainingInstructions;

        public CarSimulation(
            Random random,
            Map map,
            CarNeuralNetwork carNeuralNetwork,
            double randomnessFactor)
        {
            this.ticksSurvived = 0;

            this.car = new Car(
                Map.TileSize / 5,
                Map.TileSize / 3);

            this.pendingTrainingInstructions = new List<CarSimulationTick>();

            this.random = random;
            this.map = map;
            this.carNeuralNetwork = carNeuralNetwork;
            this.randomnessFactor = randomnessFactor;
        }

        public bool Tick()
        {
            var sensorReading = car.GetSensorReadings(map);

            var delta = 0.0001;
            if (sensorReading.CenterSensorDistanceToWall < delta || sensorReading.LeftSensorDistanceToWall < delta || sensorReading.RightSensorDistanceToWall < delta)
                return false;

            var neuralNetCarResponse = this.carNeuralNetwork.Ask(sensorReading);

            var adjustedCarResponse = new CarResponse()
            {
                AccelerationDeltaVelocity = neuralNetCarResponse.AccelerationDeltaVelocity + GetRandomnessFactor(5),
                TurnDeltaAngle = neuralNetCarResponse.TurnDeltaAngle + GetRandomnessFactor(25)
            };

            car.Accelerate(adjustedCarResponse.AccelerationDeltaVelocity);
            car.Turn(adjustedCarResponse.TurnDeltaAngle);

            car.Tick();

            pendingTrainingInstructions.Add(new CarSimulationTick()
            {
                CarResponse = adjustedCarResponse,
                CarSensorReading = sensorReading
            });

            ticksSurvived++;

            return true;
        }

        private double GetRandomnessFactor(double multiplier)
        {
            return ((random.NextDouble() * randomnessFactor * 2) - randomnessFactor) * multiplier;
        }
    }
}
