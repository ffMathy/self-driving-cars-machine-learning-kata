﻿using System;
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
        public BoundingBox BoundingBox { get; set; }

        public double Velocity { get; private set; }
        public double Angle { get; private set; }

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
            return new CarSensorReading()
            {
                LeftSensorDistanceToWall = GetSensorReading(map, -45),
                CenterSensorDistanceToWall = GetSensorReading(map, 0),
                RightSensorDistanceToWall = GetSensorReading(map, 45)
            };
        }

        public void Tick()
        {
            var directionalVector = GetRotatedOnePixelLine(Angle);

            BoundingBox.Location.X += directionalVector.End.X * Velocity;
            BoundingBox.Location.Y += directionalVector.End.Y * Velocity;
        }

        private double GetSensorReading(Map map, double angleInDegrees)
        {
            var sensorLine = GetRotatedOnePixelLine(angleInDegrees);

            foreach (var node in map.Nodes)
            {
                foreach (var line in node.Lines)
                {
                    var intersectionPoint = sensorLine.GetIntersectionPointWith(line);
                    if (intersectionPoint == null)
                        continue;

                    var distance = BoundingBox.Center.GetDistanceTo(intersectionPoint);
                    return distance;
                }
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
