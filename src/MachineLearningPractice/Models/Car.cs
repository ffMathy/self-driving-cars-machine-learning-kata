using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MachineLearningPractice.Models
{
    struct CarResponse
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

    class Car
    {
        public BoundingBox BoundingBox { get; }

        public double Velocity { get; private set; }
        public double Angle { get; private set; }

        public Car()
        {
            BoundingBox = new BoundingBox();
        }

        public void Turn(double deltaAngle)
        {
            Angle += deltaAngle;
        }

        public void Accelerate(double deltaVelocity)
        {
            Velocity += deltaVelocity;
        }

        public CarSensorReading GetSensorReadings(Map map)
        {
            var mapLinesOrderedByProximity = map
                .Nodes
                .SelectMany(x => x.Lines)
                .OrderBy(GetProximityToLine);

            return new CarSensorReading()
            {
                LeftSensorDistanceToWall = GetSensorReading(mapLinesOrderedByProximity, -45),
                CenterSensorDistanceToWall = GetSensorReading(mapLinesOrderedByProximity, 0),
                RightSensorDistanceToWall = GetSensorReading(mapLinesOrderedByProximity, 45)
            };
        }

        private double GetProximityToLine(Line line)
        {
        }

        public void Tick()
        {
            var directionalVector = GetRotatedOnePixelLine(Angle);

            BoundingBox.Location.X += directionalVector.End.X * Velocity;
            BoundingBox.Location.Y += directionalVector.End.Y * Velocity;
        }

        private double GetSensorReading(IEnumerable<Line> linesOrderedByProximity, double angleInDegrees)
        {
            var sensorLine = GetRotatedOnePixelLine(angleInDegrees);

            foreach (var line in linesOrderedByProximity)
            {
                var intersectionPoint = sensorLine.GetIntersectionPointWith(line);
                if (intersectionPoint == null)
                    continue;

                var distance = BoundingBox.Center.GetDistanceTo(intersectionPoint);
                return distance;
            }

            throw new InvalidOperationException("Did not find any intersection points.");
        }

        private Line GetRotatedOnePixelLine(double angleInDegrees)
        {
            var sensorLine = new Line()
            {
                Start = new Point(0, 0),
                End = new Point(0, 1)
            };

            sensorLine.End = sensorLine.End.RotateAround(
                sensorLine.Start,
                angleInDegrees + Angle);

            return sensorLine;
        }
    }
}
