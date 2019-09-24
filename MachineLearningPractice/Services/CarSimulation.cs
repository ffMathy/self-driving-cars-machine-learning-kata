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
        private readonly CarNeuralNetwork carNeuralNetwork;

        private readonly LinkedList<CarResponse> responseHistory;

        private readonly double randomnessFactor;

        public Car Car => car;

        public CarSimulation(
            Random random,
            CarNeuralNetwork carNeuralNetwork,
            double randomnessFactor)
        {
            this.car = new Car();
            this.responseHistory = new LinkedList<CarResponse>();

            this.random = random;
            this.carNeuralNetwork = carNeuralNetwork;
            this.randomnessFactor = randomnessFactor;
        }

        public void Tick()
        {
            //var neuralNetTick = carNeuralNetwork.Ask()

            //var carResponse = new CarResponse()
            //{
            //    AccelerationDeltaVelocity = tick.AccelerationDeltaVelocity + GetRandomnessFactor(),
            //    TurnDeltaAngle = tick.TurnDeltaAngle + GetRandomnessFactor()
            //};

            //car.Accelerate(carResponse.AccelerationDeltaVelocity);
            //car.Turn(carResponse.TurnDeltaAngle);

            //responseHistory.AddLast(carResponse);
        }

        private double GetRandomnessFactor()
        {
            return random.NextDouble() * randomnessFactor;
        }
    }
}
