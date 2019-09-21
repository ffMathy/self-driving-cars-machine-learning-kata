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

    class CarSimulation
    {
        private readonly Car car;
        private readonly Random random;

        private readonly LinkedList<CarSimulationTick> tickHistory;

        private readonly double randomnessFactor;

        public Car Car => car;

        public CarSimulation(
            Random random,
            double randomnessFactor)
        {
            this.car = new Car();
            this.tickHistory = new LinkedList<CarSimulationTick>();

            this.random = random;
            this.randomnessFactor = randomnessFactor;
        }

        public void Tick(CarSimulationTick tick)
        {
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
