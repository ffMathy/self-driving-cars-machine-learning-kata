using MachineLearningPractice.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MachineLearningPractice.Models
{
    public struct LineFormula
    {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
    }

    public struct Line
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public Point Center => new Point(
            Start.X + (End.X - Start.X) / 2,
            Start.Y + (End.Y - Start.Y) / 2);

        public LineFormula Formula
        {
            get
            {
                var a = End.Y - Start.Y;
                var b = Start.X - End.X;

                return new LineFormula()
                {
                    A = a,
                    B = b,
                    C = (a * Start.X) + (b * Start.Y)
                };
            }
        }

        public Line Rotate(double angleInDegrees) {
            return new Line() {
                Start = Start.RotateAround(
                    Center,
                    angleInDegrees),
                End = End.RotateAround(
                    Center,
                    angleInDegrees)
            };
        }

        public double GetAngleTo(Line other)
        {
            var theta1 = Math.Atan2(
                this.Start.Y - this.End.Y, 
                this.Start.X - this.End.X);

            var theta2 = Math.Atan2(
                other.Start.Y - other.End.Y,
                other.Start.X - other.End.X);

            var difference = Math.Abs(theta1 - theta2);

            var angleRadians = Math.Min(difference, Math.Abs(180 - difference));
            var angleDegrees = MathHelper.RadiansToDegrees(angleRadians);

            return angleDegrees;
        }

        public Point? GetIntersectionPointWith(Line other)
        {
            var delta = (this.Formula.A * other.Formula.B) - (other.Formula.A * this.Formula.B);
            if(delta == 0)
                return null;

            var point = new Point()
            {
                X = ((other.Formula.B * this.Formula.C) - (this.Formula.B * other.Formula.C)) / delta,
                Y = ((this.Formula.A * other.Formula.C) - (other.Formula.A * this.Formula.C)) / delta
            };

            return point;
        }

        public static Line operator *(Line a, int b)
        {
            return new Line() {
                Start = a.Start * b,
                End = a.End * b
            };
        }

        public static Line operator /(Line a, int b)
        {
            return new Line()
            {
                Start = a.Start / b,
                End = a.End / b
            };
        }

        public override string ToString() {
            return "<" + Start + ":" + End + ">";
        }
    }
}
