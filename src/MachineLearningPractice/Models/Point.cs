using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningPractice.Models
{
    class Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point()
        {

        }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Point(double factor, double x, double y) : this(x * factor, y * factor)
        {
        }

        public Point RotateAround(Point centerPoint, double angleInDegrees)
        {
            var pointToRotate = this; 
            
            var angleInRadians = angleInDegrees * (Math.PI / 180);
            var cosTheta = Math.Cos(angleInRadians);
            var sinTheta = Math.Sin(angleInRadians);

            return new Point
            {
                X = 
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        public double GetDistanceTo(Point other)
        {
            var a = Math.Abs(other.X - this.X);
            var b = Math.Abs(other.Y - this.Y);

            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }
    }
}
