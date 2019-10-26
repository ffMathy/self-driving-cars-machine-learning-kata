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

        private readonly HashSet<CarSimulationTick> pendingTrainingInstructions;
        private readonly IList<ProgressLine> allProgressLines;

        private readonly decimal randomnessFactor;

        private ProgressLine currentProgressLine;

        private Car car;

        private long ticksSurvived;
        private decimal distanceTraveled;

        private bool isCrashed;

        private int laps;

        public long TicksSurvived => ticksSurvived;
        public decimal DistanceTraveled => distanceTraveled;

        public ProgressLine CurrentProgressLine => currentProgressLine;

        public decimal Fitness
        {
            get
            {
                var lastOffset = allProgressLines.Last().Offset;

                var progressPenalty = lastOffset - currentProgressLine.Offset;
                var lapPenalty = -(lastOffset * this.laps) * 2;

                var timePenalty = ticksSurvived;

                return timePenalty + ((progressPenalty + lapPenalty) * 30);
            }
        }

        public Car Car => car;

        public HashSet<CarSimulationTick> PendingTrainingInstructions => pendingTrainingInstructions;

        public bool IsCrashed => isCrashed;

        public CarSimulation(
            Random random,
            Map map,
            CarNeuralNetwork carNeuralNetwork,
            decimal randomnessFactor)
        {
            this.ticksSurvived = 0;

            this.allProgressLines = map.Nodes
                .SelectMany(x => x
                    .ProgressLines
                    .OrderBy(l => l.Offset))
                .ToArray();

            this.car = new Car();

            this.pendingTrainingInstructions = new HashSet<CarSimulationTick>();

            this.random = random;
            this.map = map;
            this.carNeuralNetwork = carNeuralNetwork;
            this.randomnessFactor = randomnessFactor;
        }

        public void Tick()
        {
            if (isCrashed)
                return;

            var sensorReadings = GetSensorReadings();
            var neuralNetCarResponse = this.carNeuralNetwork.Ask(sensorReadings);

            var deltaVelocity = (neuralNetCarResponse.AccelerationDeltaVelocity) + GetRandomnessFactor(1);
            var deltaAngle = (neuralNetCarResponse.TurnDeltaAngle) + GetRandomnessFactor(0.75m);

            deltaVelocity = car.Accelerate(deltaVelocity);
            deltaAngle = car.Turn(deltaAngle);

            car.Tick();

            var wallLines = map.Nodes.SelectMany(x => x.WallLines);
            var distanceToClosestWallPoint = GetDistanceToClosestWallLineIntersectionPoint(wallLines);
            if (distanceToClosestWallPoint < Car.Size / 2)
            {
                isCrashed = true;
                return;
            }

            var previousProgressLine = currentProgressLine;
            var newProgressLine = GetClosestIntersectionPointProgressLine(allProgressLines);
            if(previousProgressLine == null || Math.Abs(newProgressLine.Offset - previousProgressLine.Offset) < 3)
                currentProgressLine = newProgressLine;

            if (previousProgressLine != null)
            {
                var lastOffset = allProgressLines.Last().Offset;
                if (currentProgressLine.Offset == 0 && previousProgressLine.Offset == lastOffset)
                {
                    laps++;
                }
                else if (previousProgressLine.Offset == 0 && currentProgressLine.Offset == lastOffset)
                {
                    laps--;
                }
            }

            pendingTrainingInstructions.Add(new CarSimulationTick()
            {
                CarResponse = new CarResponse()
                {
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
            isCrashed = false;
            ticksSurvived = 0;
            laps = 0;
            currentProgressLine = null;
            pendingTrainingInstructions.Clear();

            car = new Car();
        }

        private ProgressLine GetClosestIntersectionPointProgressLine(IEnumerable<ProgressLine> lines)
        {
            return lines
                .OrderBy(x => DistanceHelper
                    .FindClosestPointOnLine(
                        car.BoundingBox.Center,
                        x.Line)
                    .GetDistanceTo(car.BoundingBox.Center))
                .First();
        }

        private double GetDistanceToClosestWallLineIntersectionPoint(IEnumerable<WallLine> lines)
        {
            return lines
                .Select(wallLine => DistanceHelper.FindClosestPointOnLine(
                    car.BoundingBox.Center,
                    wallLine.Line))
                .Select(x => x.GetDistanceTo(car.BoundingBox.Center))
                .OrderBy(x => x)
                .First();
        }

        public CarSensorReadingSnapshot GetSensorReadings()
        {
            var mapLines = map.Nodes
                .SelectMany(x => x.WallLines)
                .Select(x => x.Line)
                .ToArray();
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
