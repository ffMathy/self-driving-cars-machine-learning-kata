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

        public double SpeedVelocity { get; private set; }

        public double TurnAngle { get; private set; }
        public double TurnAngleVelocity { get; private set; }

        public Line ForwardDirectionLine
        {
            get
            {
                var line = new Line() {
                    Start = BoundingBox.Center - new Point(0, BoundingBox.Size.Width / 2),
                    End = BoundingBox.Center + new Point(0, BoundingBox.Size.Width / 2)
                };

                return line.Rotate(TurnAngle);
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
            TurnAngleVelocity += deltaAngle;
            TurnAngleVelocity = Math.Min(10, TurnAngleVelocity);
        }

        public void Accelerate(double deltaVelocity)
        {
            SpeedVelocity += deltaVelocity;
            SpeedVelocity = Math.Min(0.1, SpeedVelocity);
        }

        public void Tick()
        {
            var directionalVector = ForwardDirectionLine;

            TurnAngle += TurnAngleVelocity;

            BoundingBox.Location = new Point(
                BoundingBox.Location.X + (directionalVector.End.X * SpeedVelocity),
                BoundingBox.Location.Y + (directionalVector.End.Y * SpeedVelocity));
        }
    }
}
