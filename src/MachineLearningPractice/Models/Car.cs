using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MachineLearningPractice.Models
{

    public class Car
    {
        public BoundingBox BoundingBox { get; }

        public double SpeedVelocity { get; private set; }

        public double TurnAngle { get; private set; }
        public double TurnAngleVelocity { get; private set; }

        public Line ForwardDirectionLine
        {
            get
            {
                var line = new Line()
                {
                    Start = new Point(0, -0.5),
                    End = new Point(0, 0.5)
                };

                return line.Rotate(TurnAngle);
            }
        }

        public Car(
            double width,
            double height)
        {
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
            TurnAngle += TurnAngleVelocity;

            BoundingBox.Location = new Point(
                BoundingBox.Location.X - (ForwardDirectionLine.End.X * SpeedVelocity),
                BoundingBox.Location.Y - (ForwardDirectionLine.End.Y * SpeedVelocity));
        }
    }
}
