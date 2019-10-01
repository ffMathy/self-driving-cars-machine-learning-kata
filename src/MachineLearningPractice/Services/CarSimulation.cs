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
        public decimal AccelerationDeltaVelocity { get; set; }
        public decimal TurnDeltaAngle { get; set; }
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
        private readonly Random random;
        private readonly Map map;
        private readonly CarNeuralNetwork carNeuralNetwork;

        private readonly List<CarSimulationTick> pendingTrainingInstructions;

        private readonly decimal randomnessFactor;

        private MapNode currentMapNode;
        private Car car;

        private ulong ticksSurvived;
        private decimal distanceTraveled;

        private bool isCrashed;

        public ulong TicksSurvived => ticksSurvived;
        public decimal DistanceTraveled => distanceTraveled;

        public Car Car => car;

        public MapNode CurrentMapNode => currentMapNode;

        public IReadOnlyList<CarSimulationTick> PendingTrainingInstructions => pendingTrainingInstructions;

        public bool IsCrashed => isCrashed;

        public CarSimulation(
            Random random,
            Map map,
            CarNeuralNetwork carNeuralNetwork,
            decimal randomnessFactor)
        {
            this.ticksSurvived = 0;

            this.car = new Car();

            this.pendingTrainingInstructions = new List<CarSimulationTick>();

            this.random = random;
            this.map = map;
            this.carNeuralNetwork = carNeuralNetwork;
            this.randomnessFactor = randomnessFactor;
        }

        public void Tick()
        {
            if(isCrashed)
                return;

            var sensorReadings = GetSensorReadings();
            var neuralNetCarResponse = this.carNeuralNetwork.Ask(sensorReadings);

            var deltaVelocity = (neuralNetCarResponse.AccelerationDeltaVelocity) + GetRandomnessFactor(5);
            var deltaAngle = (neuralNetCarResponse.TurnDeltaAngle) + GetRandomnessFactor(1);

            deltaVelocity = car.Accelerate(deltaVelocity);
            deltaAngle = car.Turn(deltaAngle);

            car.Tick();

            var distanceToClosestIntersectionPoint = GetDistanceToClosestIntersectionPoint();
            if(distanceToClosestIntersectionPoint < Car.Size / 3) {
                isCrashed = true;
                return;
            }

            currentMapNode = map.Nodes
                .OrderBy(x => x.Position.GetDistanceTo(car.BoundingBox.Center))
                .First();

            pendingTrainingInstructions.Add(new CarSimulationTick()
            {
                CarResponse = new CarResponse() {
                    AccelerationDeltaVelocity = deltaVelocity,
                    TurnDeltaAngle = deltaAngle
                },
                CarSensorReading = sensorReadings
            });

            distanceTraveled += car.SpeedVelocity;
            ticksSurvived++;
        }

        public void Reset()
        {
            currentMapNode = null;
            isCrashed = false;
            ticksSurvived = 0;
            
            car = new Car();
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
            decimal angleInDegrees)
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

        private decimal GetRandomnessFactor(decimal multiplier)
        {
            return (((decimal)random.NextDouble() * randomnessFactor * 2) - randomnessFactor) * multiplier;
        }
    }
}
