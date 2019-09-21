using MachineLearningPractice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningPractice.Services
{
    struct CarSimulationTick
    {
        public double AccelerationDeltaVelocity { get; set; }
        public double TurnDeltaAngle { get; set; }
    }

    struct CarSensorReading
    {
        public double LeftSensorDistanceToWall { get; set; }
        public double CenterSensorDistanceToWall { get; set; }
        public double RightSensorDistanceToWall { get; set; }
    }

    class CarSimulation
    {
        private readonly Car car;
        private readonly Random random;
        private readonly CarNeuralNetwork carNeuralNetwork;

        private readonly LinkedList<CarSimulationTick> tickHistory;

        private readonly double randomnessFactor;

        public Car Car => car;

        public CarSimulation(
            Random random,
            CarNeuralNetwork carNeuralNetwork,
            double randomnessFactor)
        {
            this.car = new Car();
            this.tickHistory = new LinkedList<CarSimulationTick>();

            this.random = random;
            this.carNeuralNetwork = carNeuralNetwork;
            this.randomnessFactor = randomnessFactor;
        }

        public void Tick()
        {
            var neuralNetTick = carNeuralNetwork.Ask()

            var tickWithRandomness = new CarSimulationTick()
            {
                AccelerationDeltaVelocity = tick.AccelerationDeltaVelocity + GetRandomnessFactor(),
                TurnDeltaAngle = tick.TurnDeltaAngle + GetRandomnessFactor()
            };

            car.Accelerate(tickWithRandomness.AccelerationDeltaVelocity);
            car.Turn(tickWithRandomness.TurnDeltaAngle);

            tickHistory.AddLast(tickWithRandomness);
        }

        private double GetRandomnessFactor()
        {
            return random.NextDouble() * randomnessFactor;
        }
    }
}
