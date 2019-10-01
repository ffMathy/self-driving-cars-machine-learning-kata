using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MachineLearningPractice.Models
{

    public class Car
    {
        public const int Size = 20;

        public BoundingBox BoundingBox { get; }

        public decimal SpeedVelocity { get; private set; }

        public decimal TurnAngle { get; private set; }
        public decimal TurnAngleVelocity { get; private set; }

        public Line ForwardDirectionLine
        {
            get
            {
                var line = new Line()
                {
                    Start = new Point(0, 0),
                    End = new Point(0, 0.5m)
                };

                return line.Rotate(TurnAngle);
            }
        }

        public Car()
        {
            const int size = Size;

            BoundingBox = new BoundingBox()
            {
                Size = new Size()
                {
                    Width = size,
                    Height = size
                },
                Location = new Point()
                {
                    X = -size / 2,
                    Y = -size / 2
                }
            };
        }

        public decimal Turn(decimal deltaAngle)
        {
            TurnAngleVelocity += deltaAngle;

            const int threshold = 10;

            if (TurnAngleVelocity < -threshold)
                TurnAngleVelocity = -threshold;

            if (TurnAngleVelocity > threshold)
                TurnAngleVelocity = threshold;

            return TurnAngleVelocity;
        }

        public decimal Accelerate(decimal deltaVelocity)
        {
            SpeedVelocity += deltaVelocity;
            
            const int highThreshold = 10;
            const int lowThreshold = 1;

            if (SpeedVelocity < lowThreshold)
                SpeedVelocity = lowThreshold;

            if (SpeedVelocity > highThreshold)
                SpeedVelocity = highThreshold;

            return SpeedVelocity;
        }

        public void Tick()
        {
            TurnAngle += TurnAngleVelocity;

            BoundingBox.Location = new Point(
                BoundingBox.Location.X + (ForwardDirectionLine.End.X * 2 * SpeedVelocity),
                BoundingBox.Location.Y + (ForwardDirectionLine.End.Y * 2 * SpeedVelocity));
        }
    }
}
