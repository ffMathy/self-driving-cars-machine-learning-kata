using MachineLearningPractice.Helpers;
using MachineLearningPractice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningPractice.Services
{
    public struct CarResponse
    {
        public double AccelerationDeltaVelocity { get; set; }
        public double TurnDeltaAngle { get; set; }
    }

    public struct CarSensorReadingSnapshot
    {
        public CarSensorReading? LeftSensor { get; set; }
        public CarSensorReading? CenterSensor { get; set; }
        public CarSensorReading? RightSensor { get; set; }
    }

    public struct CarSensorReading
    {
        public Point IntersectionPoint { get; set; }
        public double Distance { get; set; }
    }

    public class CarSimulation
    {
        private readonly Car car;
        private readonly Random random;
        private readonly Map map;
        private readonly CarNeuralNetwork carNeuralNetwork;

        private readonly List<CarSimulationTick> pendingTrainingInstructions;

        private readonly double randomnessFactor;

        private ulong ticksSurvived;

        public ulong TicksSurvived => ticksSurvived;

        public Car Car => car;

        public IReadOnlyList<CarSimulationTick> PendingTrainingInstructions => pendingTrainingInstructions;

        public CarSimulation(
            Random random,
            Map map,
            CarNeuralNetwork carNeuralNetwork,
            double randomnessFactor)
        {
            this.ticksSurvived = 0;

            this.car = new Car();

            this.pendingTrainingInstructions = new List<CarSimulationTick>();

            this.random = random;
            this.map = map;
            this.carNeuralNetwork = carNeuralNetwork;
            this.randomnessFactor = randomnessFactor;
        }

        public bool Tick()
        {
            var sensorReadings = GetSensorReadings();
            var neuralNetCarResponse = this.carNeuralNetwork.Ask(sensorReadings);

            var adjustedCarResponse = new CarResponse()
            {
                AccelerationDeltaVelocity = 
                    neuralNetCarResponse.AccelerationDeltaVelocity + 
                    GetRandomnessFactor(10),
                TurnDeltaAngle = 
                    neuralNetCarResponse.TurnDeltaAngle + 
                    GetRandomnessFactor(10)
            };

            car.Accelerate(adjustedCarResponse.AccelerationDeltaVelocity);
            car.Turn(adjustedCarResponse.TurnDeltaAngle);

            car.Tick();

            var distanceToClosestIntersectionPoint = GetDistanceToClosestIntersectionPoint();
            if(distanceToClosestIntersectionPoint < Car.Size / 3)
                return false;

            pendingTrainingInstructions.Add(new CarSimulationTick()
            {
                CarResponse = adjustedCarResponse,
                CarSensorReading = sensorReadings
            });

            ticksSurvived++;

            return true;
        }

        private double GetDistanceToClosestIntersectionPoint()
        {
            return map.Nodes
                .SelectMany(x => x.Lines)
                .Select(line => DistanceHelper.FindClosestPointOnLine(
                    car.BoundingBox.Center, 
                    line))
                .Select(x => x.GetDistanceTo(car.BoundingBox.Center))
                .OrderBy(x => x)
                .First();
        }

        public CarSensorReadingSnapshot GetSensorReadings()
        {
            var mapLines = map.Nodes.SelectMany(x => x.Lines);
            var sensorReadingCached = new CarSensorReadingSnapshot()
            {
                LeftSensor = GetSensorReading(mapLines, -360 / 8),
                CenterSensor = GetSensorReading(mapLines, 0),
                RightSensor = GetSensorReading(mapLines, 360 / 8)
            };

            return sensorReadingCached;
        }

        private CarSensorReading? GetSensorReading(
            IEnumerable<Line> lines,
            double angleInDegrees)
        {
            var forwardDirectionLine = new Line()
            {
                Start = car.BoundingBox.Center,
                End = car.ForwardDirectionLine.End * 2 + car.BoundingBox.Center
            };
            var sensorLine = forwardDirectionLine.Rotate(angleInDegrees);

            var carSensorReadings = new HashSet<CarSensorReading>();
            foreach (var line in lines)
            {
                var intersectionPointNullable = sensorLine.GetIntersectionPointWith(line);
                if (intersectionPointNullable == null)
                    continue;

                var intersectionPoint = intersectionPointNullable.Value;
                if (DirectionHelper.IsPointOutsideLineBoundaries(line, intersectionPoint))
                    continue;

                if (!DirectionHelper.IsPointInDirectionOfSensorLine(sensorLine, intersectionPoint))
                    continue;

                var distance = car.BoundingBox.Center.GetDistanceTo(intersectionPoint);
                carSensorReadings.Add(new CarSensorReading()
                {
                    IntersectionPoint = intersectionPoint,
                    Distance = distance
                });
            }

            if (carSensorReadings.Count == 0)
                return null;

            return carSensorReadings
                .OrderBy(x => x.Distance)
                .First();
        }

        private double GetRandomnessFactor(double multiplier)
        {
            return ((random.NextDouble() * randomnessFactor * 2) - randomnessFactor) * multiplier;
        }
    }
}
