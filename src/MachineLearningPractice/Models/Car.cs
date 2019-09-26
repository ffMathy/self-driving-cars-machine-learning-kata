using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MachineLearningPractice.Models
{
    public struct CarResponse
    {
        public double AccelerationDeltaVelocity { get; set; }
        public double TurnDeltaAngle { get; set; }
    }

    public struct CarSensorReading
    {
        public double LeftSensorDistanceToWall { get; set; }
        public double CenterSensorDistanceToWall { get; set; }
        public double RightSensorDistanceToWall { get; set; }
    }

    public class Car
    {
        private readonly Map map;

        public BoundingBox BoundingBox { get; }

        public double Velocity { get; private set; }
        public double Angle { get; private set; }

        public Line ForwardDirectionLine
        {
            get
            {
                var line = new Line() {
                    Start = BoundingBox.Center - new Point(0, 0.5),
                    End = BoundingBox.Center + new Point(0, 0.5)
                };

                return line.Rotate(Angle);
            }
        }

        public Car(
            Map map,
            double width,
            double height)
        {
            this.map = map;

            BoundingBox = new BoundingBox()
            {
                Size = new Size()
                {
                    Width = width,
                    Height = height
                }
            };
        }

        public void Turn(double deltaAngle)
        {
            Angle += deltaAngle;
        }

        public void Accelerate(double deltaVelocity)
        {
            Velocity += deltaVelocity;
        }

        public void Tick()
        {
            var directionalVector = ForwardDirectionLine;

            BoundingBox.Location = new Point(
                BoundingBox.Location.X + (directionalVector.End.X * Velocity),
                BoundingBox.Location.Y + (directionalVector.End.Y * Velocity));
        }
    }
}
