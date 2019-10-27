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
        private readonly Map map;
        private readonly CarNeuralNetwork carNeuralNetwork;

        private readonly IDictionary<int, List<ProgressLine>> allProgressLinesByMapNodeOffset;
        private readonly IDictionary<int, List<WallLine>> allWallLinesByMapNodeOffset;

        private int laps;
        private int lastProgressLineOffset;

        private long lastProgressLineIncreaseTick;
        private int highestProgressLineOffset;

        public long TicksSurvived { get; private set; }

        public ProgressLine CurrentProgressLine { get; private set; }

        public MapNode CurrentMapNode => CurrentProgressLine?.MapNode;

        public CarSensorReadingSnapshot SensorReadings { get; private set; }

        public Car Car { get; private set; }

        private Random random;

        public bool IsCrashed { get; private set; }

        public decimal Fitness { get; private set; }

        public CarSimulation(
            Random random,
            Map map,
            CarNeuralNetwork carNeuralNetwork)
        {
            this.Car = new Car();

            this.carNeuralNetwork = carNeuralNetwork;
            this.map = map;
            this.random = random;

            var allProgressLines = this.map.Nodes.SelectMany(x => x.ProgressLines);
            this.allProgressLinesByMapNodeOffset = allProgressLines
                .GroupBy(x => x
                    .MapNode
                    .Offset)
                .ToDictionary(
                    x => x.Key,
                    x => x.ToList());

            this.allWallLinesByMapNodeOffset = this.map.Nodes
                .SelectMany(x => x.WallLines)
                .GroupBy(x => x
                    .MapNode
                    .Offset)
                .ToDictionary(
                    x => x.Key,
                    x => x.ToList());

            this.lastProgressLineOffset = allProgressLines.Last().Offset;

            Reset();
        }

        public void Tick()
        {
            if (IsCrashed)
                return;

            SensorReadings = GetSensorReadings();

            var neuralNetCarResponse = this.carNeuralNetwork.Ask(SensorReadings);

            var deltaVelocity = neuralNetCarResponse.AccelerationDeltaVelocity;
            var deltaAngle = neuralNetCarResponse.TurnDeltaAngle;

            deltaVelocity = Car.Accelerate(deltaVelocity);
            deltaAngle = Car.Turn(deltaAngle);

            Car.Tick();

            var distanceToClosestWallPoint = GetDistanceToClosestWallLineIntersectionPoint();
            if (distanceToClosestWallPoint < Car.Size / 2)
            {
                IsCrashed = true;
                return;
            }

            var previousProgressLine = CurrentProgressLine;
            var newProgressLine = GetClosestIntersectionPointProgressLine();
            if (previousProgressLine == null || Math.Abs(newProgressLine.Offset - previousProgressLine.Offset) < 3)
                CurrentProgressLine = newProgressLine;

            if (newProgressLine.Offset > highestProgressLineOffset)
            {
                lastProgressLineIncreaseTick = TicksSurvived;
                highestProgressLineOffset = newProgressLine.Offset;
            }

            if (previousProgressLine != null)
            {
                if (CurrentProgressLine.Offset == 0 && previousProgressLine.Offset == lastProgressLineOffset)
                {
                    laps++;
                }
                else if (previousProgressLine.Offset == 0 && CurrentProgressLine.Offset == lastProgressLineOffset)
                {
                    laps--;
                }
            }

            carNeuralNetwork.Record(SensorReadings, new CarResponse()
            {
                AccelerationDeltaVelocity = deltaVelocity,
                TurnDeltaAngle = deltaAngle
            });

            TicksSurvived++;

            CalculateFitness();

            if(lastProgressLineIncreaseTick != 0) { 
                var timeSinceLastProgressLineIncrease = TicksSurvived - lastProgressLineIncreaseTick;
                if(timeSinceLastProgressLineIncrease > 600)
                {
                    IsCrashed = true;
                    return;
                }
            }

            if (Fitness > 3000)
            {
                IsCrashed = true;
                return;
            }
        }

        public void ApplyTraining()
        {
            carNeuralNetwork.Train();
        }

        public CarSimulation CrossWith(CarSimulation other)
        {
            return new CarSimulation(
                random,
                map,
                carNeuralNetwork.CrossWith(
                    other.carNeuralNetwork));
        }

        public void Mutate()
        {
            carNeuralNetwork.Mutate();
        }

        private void CalculateFitness()
        {
            var lastOffset = map.Nodes.Length * 3;

            var progressPenalty = lastOffset - CurrentProgressLine.Offset;
            var lapPenalty = -(lastOffset * this.laps) * 2;

            var timePenalty = TicksSurvived;

            Fitness = timePenalty + ((progressPenalty + lapPenalty) * 30);
        }

        public void Reset()
        {
            IsCrashed = false;
            TicksSurvived = 0;
            laps = 0;

            CurrentProgressLine = this.map.Nodes.First().ProgressLines.First();

            Car = new Car();
        }

        private ProgressLine GetClosestIntersectionPointProgressLine()
        {
            var before = allProgressLinesByMapNodeOffset[CurrentMapNode.Previous.Offset];
            var current = allProgressLinesByMapNodeOffset[CurrentMapNode.Offset];
            var after = allProgressLinesByMapNodeOffset[CurrentMapNode.Next.Offset];

            var lines = before.Union(current).Union(after);

            return lines
                .OrderBy(progressLine => DistanceHelper
                    .FindClosestPointOnLine(
                        Car.BoundingBox.Center,
                        progressLine.Line)
                    .GetDistanceTo(Car.BoundingBox.Center))
                .First();
        }

        private double GetDistanceToClosestWallLineIntersectionPoint()
        {
            var before = allWallLinesByMapNodeOffset[CurrentMapNode.Previous.Offset];
            var current = allWallLinesByMapNodeOffset[CurrentMapNode.Offset];
            var after = allWallLinesByMapNodeOffset[CurrentMapNode.Next.Offset];

            var lines = before.Union(current).Union(after);

            return lines
                .Select(wallLine => DistanceHelper.FindClosestPointOnLine(
                    Car.BoundingBox.Center,
                    wallLine.Line))
                .Select(x => x.GetDistanceTo(Car.BoundingBox.Center))
                .OrderBy(x => x)
                .First();
        }

        private CarSensorReadingSnapshot GetSensorReadings()
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
                Start = Car.BoundingBox.Center,
                End = Car.ForwardDirectionLine.End * 2 + Car.BoundingBox.Center
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

                var distance = Car.BoundingBox.Center.GetDistanceTo(intersectionPoint);
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
    }
}
