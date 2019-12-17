using FluffySpoon.Neuro.Evolution;
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

    public class CarSimulation : ISimulation
    {
        private readonly Map map;

        private readonly IDictionary<int, List<ProgressLine>> allProgressLinesByMapNodeOffset;

        private int laps;
        private int lastProgressLineOffset;

        private long lastProgressLineIncreaseTick;
        private int highestProgressLineOffset;

        public long TicksSurvived { get; set; }

        public ProgressLine CurrentProgressLine { get; set; }

        public MapNode CurrentMapNode => CurrentProgressLine?.MapNode;

        public CarSensorReadingSnapshot SensorReadings { get; private set; }

        public Car Car { get; private set; }

        public bool HasEnded { get; private set; }

        public double Fitness
        {
            get
            {
                var mapNodesLength = map.Nodes.Length;

                var progressPenalty = mapNodesLength - CurrentProgressLine.Offset;
                var lapPenalty = mapNodesLength * this.laps;

                var timePenalty = -TicksSurvived;

                return timePenalty + ((progressPenalty - lapPenalty) * 30);
            }
        }

        public CarSimulation(
            Map map)
        {
            Car = new Car();

            this.map = map;

            var allProgressLines = this.map.Nodes
                .SelectMany(x => x.ProgressLines)
                .OrderBy(x => x.Offset);
            this.allProgressLinesByMapNodeOffset = allProgressLines
                .GroupBy(x => x
                    .MapNode
                    .Offset)
                .ToDictionary(
                    x => x.Key,
                    x => x.ToList());

            this.lastProgressLineOffset = allProgressLines.Last().Offset;

            HasEnded = false;
            TicksSurvived = 0;
            laps = 0;

            CurrentProgressLine = this.map.Nodes.First().ProgressLines.First();
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

        private static double[] GetNeuralInputFromSensorReading(CarSensorReadingSnapshot sensorReading)
        {
            return new[] {
                sensorReading.CenterSensor?.Distance ?? 0,
                sensorReading.LeftSensor?.Distance ?? 0,
                sensorReading.RightSensor?.Distance ?? 0
            };
        }

        private static decimal NormalizeBinaryPrediction(double prediction)
        {
            if (prediction > 0.66)
            {
                return 1m;
            }
            else if (prediction < 0.33)
            {
                return -1m;
            }

            return 0;
        }

        public Task ResetAsync()
        {
            Car = new Car();
            return Task.CompletedTask;
        }

        public async Task<double[]> GetInputsAsync()
        {
            SensorReadings = GetSensorReadings();
            return GetNeuralInputFromSensorReading(SensorReadings);
        }

        public async Task TickAsync(double[] outputs)
        {
            var neuralNetCarResponse = new CarResponse()
            {
                AccelerationDeltaVelocity = NormalizeBinaryPrediction(outputs[0]),
                TurnDeltaAngle = NormalizeBinaryPrediction(outputs[1])
            };

            var deltaVelocity = neuralNetCarResponse.AccelerationDeltaVelocity;
            var deltaAngle = neuralNetCarResponse.TurnDeltaAngle;

            Car.Accelerate(deltaVelocity);
            Car.Turn(deltaAngle);

            Car.Tick();

            var previousProgressLine = CurrentProgressLine;
            var newProgressLine = GetClosestIntersectionPointProgressLine();
            if (previousProgressLine == null || Math.Abs(newProgressLine.Offset - previousProgressLine.Offset) < 3)
                CurrentProgressLine = newProgressLine;

            var mapNodeBoundingBoxes = new[]
            {
                CurrentMapNode.Previous.Previous.BoundingBox,
                CurrentMapNode.Previous.BoundingBox,
                CurrentMapNode.BoundingBox,
                CurrentMapNode.Next.BoundingBox,
                CurrentMapNode.Next.Next.BoundingBox
            };

            if (newProgressLine.Offset > highestProgressLineOffset || (newProgressLine.Offset == 0 && previousProgressLine.Offset == lastProgressLineOffset))
            {
                lastProgressLineIncreaseTick = TicksSurvived;
                highestProgressLineOffset = newProgressLine.Offset;
            }

            CheckForNewLapCount(previousProgressLine);

            var isWithinAnyNode = Car.BoundingBox.IsWithin(mapNodeBoundingBoxes);
            if (!isWithinAnyNode)
            {
                HasEnded = true;
                return;
            }

            TicksSurvived++;

            if (lastProgressLineIncreaseTick != 0)
            {
                var timeSinceLastProgressLineIncrease = TicksSurvived - lastProgressLineIncreaseTick;
                if (timeSinceLastProgressLineIncrease > 600)
                {
                    HasEnded = true;
                    return;
                }
            }

            if (Fitness > 3000)
            {
                HasEnded = true;
                return;
            }
        }

        private void CheckForNewLapCount(ProgressLine previousProgressLine)
        {
            if (previousProgressLine == null)
            {
                return;
            }

            if (CurrentProgressLine.Offset == 0 && previousProgressLine.Offset == lastProgressLineOffset)
            {
                laps++;
            }
            else if (previousProgressLine.Offset == 0 && CurrentProgressLine.Offset == lastProgressLineOffset)
            {
                laps--;
            }
        }
    }
}
