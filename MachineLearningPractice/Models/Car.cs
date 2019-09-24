using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MachineLearningPractice.Models
{
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

        public void Tick()
        {

        }
    }
}
