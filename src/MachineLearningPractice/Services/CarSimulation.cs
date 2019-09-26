using MachineLearningPractice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningPractice.Services
{
    public class CarSimulation
    {
        private readonly Car car;
        private readonly Random random;
        private readonly Map map;
        private readonly CarNeuralNetwork carNeuralNetwork;

        private readonly List<CarSimulationTick> pendingTrainingInstructions;

        private readonly double randomnessFactor;

        private CarSensorReading? sensorReadingCached;

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
                map,
                Map.TileSize / 2,
                Map.TileSize / 2);

            this.pendingTrainingInstructions = new List<CarSimulationTick>();

            this.random = random;
            this.map = map;
            this.carNeuralNetwork = carNeuralNetwork;
            this.randomnessFactor = randomnessFactor;
        }

        public CarSensorReading GetSensorReadings()
        {
            if(sensorReadingCached != null)
                return sensorReadingCached.Value;

            var mapLinesOrderedByProximity = map
                .Nodes
                .SelectMany(x => x.Lines)
                .OrderBy(GetCarProximityToLine);

            sensorReadingCached = new CarSensorReading()
            {
                LeftSensorDistanceToWall = GetSensorReading(mapLinesOrderedByProximity, -45),
                CenterSensorDistanceToWall = GetSensorReading(mapLinesOrderedByProximity, 0),
                RightSensorDistanceToWall = GetSensorReading(mapLinesOrderedByProximity, 45)
            };

            return sensorReadingCached.Value;
        }

        public bool Tick()
        {
            sensorReadingCached = null;

            var sensorReadings = GetSensorReadings();
            var neuralNetCarResponse = this.carNeuralNetwork.Ask(sensorReadings);

            var adjustedCarResponse = new CarResponse()
            {
                AccelerationDeltaVelocity = neuralNetCarResponse.AccelerationDeltaVelocity + GetRandomnessFactor(1),
                TurnDeltaAngle = neuralNetCarResponse.TurnDeltaAngle + GetRandomnessFactor(5)
            };

            car.Accelerate(adjustedCarResponse.AccelerationDeltaVelocity);
            car.Turn(adjustedCarResponse.TurnDeltaAngle);

            car.Tick();

            pendingTrainingInstructions.Add(new CarSimulationTick()
            {
                CarResponse = adjustedCarResponse,
                CarSensorReading = sensorReadings
            });

            ticksSurvived++;

            return true;
        }

        private double GetCarProximityToLine(Line line)
        {
            var intersectionPoint = car.ForwardDirectionLine.GetIntersectionPointWith(line);
            if (intersectionPoint == null)
                return double.MaxValue;

            return intersectionPoint.Value.GetDistanceTo(car.BoundingBox.Center);
        }

        private double GetSensorReading(IEnumerable<Line> linesOrderedByProximity, double angleInDegrees)
        {
            var sensorLine = car.ForwardDirectionLine.Rotate(angleInDegrees);

            foreach (var line in linesOrderedByProximity)
            {
                var intersectionPoint = sensorLine.GetIntersectionPointWith(line);
                if (intersectionPoint == null)
                    continue;

                var distance = car.BoundingBox.Center.GetDistanceTo(intersectionPoint.Value);
                return distance;
            }

            throw new InvalidOperationException("Did not find any intersection points.");
        }

        private double GetRandomnessFactor(double multiplier)
        {
            return ((random.NextDouble() * randomnessFactor * 2) - randomnessFactor) * multiplier;
        }
    }
}
