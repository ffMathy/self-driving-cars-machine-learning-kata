using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MachineLearningPractice.Models
{
    class Car
    {
        public Point Location { get; set; }
        public Size Size { get; set; }

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
    }
}
