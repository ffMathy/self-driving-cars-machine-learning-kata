using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningPractice.Models
{
    public struct Point
    {
        public decimal X { get; set; }
        public decimal Y { get; set; }

        public Point(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }

        public Point(decimal factor, decimal x, decimal y) : this(x * factor, y * factor)
        {
        }

        public Point RotateAround(Point centerPoint, decimal angleInDegrees)
        {
            if(angleInDegrees == 0)
                return new Point(X, Y);

            var pointToRotate = this;

            var angleInRadians = (double)angleInDegrees * (Math.PI / 180);
            var cosTheta = (decimal)Math.Cos(angleInRadians);
            var sinTheta = (decimal)Math.Sin(angleInRadians);

            return new Point
            {
                X = (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y = (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        public double GetDistanceTo(Point other)
        {
            var a = (double)Math.Abs(other.X - this.X);
            var b = (double)Math.Abs(other.Y - this.Y);

            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }

        public override string ToString()
        {
            return "(" + X + "|" + Y + ")";
        }

        public static Point operator +(Point a, int b)
        {
            return new Point(a.X + b, a.Y + b);
        }

        public static Point operator -(Point a, int b)
        {
            return new Point(a.X - b, a.Y - b);
        }

        public static Point operator *(Point a, int b)
        {
            return new Point(a.X * b, a.Y * b);
        }

        public static Point operator /(Point a, int b)
        {
            return new Point(a.X / b, a.Y / b);
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }
    }
}
