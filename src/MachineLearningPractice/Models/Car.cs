using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MachineLearningPractice.Models
{

    public class Car
    {
        public const int Size = 30;

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
                    End = new Point(0, -0.5m)
                };

                return line.RotateAround(new Point(), TurnAngle);
            }
        }

        public Car()
        {
            const int size = Size;

            SpeedVelocity = 5;

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
            var previousAngleVelocity = TurnAngleVelocity;
            TurnAngleVelocity += deltaAngle;

            const int threshold = 5;

            if (TurnAngleVelocity < -threshold) { 
                TurnAngleVelocity = -threshold;
            } else if (TurnAngleVelocity > threshold) { 
                TurnAngleVelocity = threshold;
            }

            var delta = TurnAngleVelocity - previousAngleVelocity;
            Accelerate(-Math.Abs(delta) / 1000m);

            return delta;
        }

        public decimal Accelerate(decimal deltaVelocity)
        {
            var previousSpeedVelocity = SpeedVelocity;
            SpeedVelocity += deltaVelocity;

            EnsureSpeedWithinBounds();

            return SpeedVelocity - previousSpeedVelocity;
        }

        private void EnsureSpeedWithinBounds()
        {
            const decimal highThreshold = 10;
            const decimal lowThreshold = 1m;

            if (SpeedVelocity < lowThreshold)
                SpeedVelocity = lowThreshold;

            if (SpeedVelocity > highThreshold)
                SpeedVelocity = highThreshold;
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
