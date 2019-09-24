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

        }

        private CarSensorReading GetSensorReading(Map map)
        {

        }

        public void Tick()
        {

        }
    }
}
