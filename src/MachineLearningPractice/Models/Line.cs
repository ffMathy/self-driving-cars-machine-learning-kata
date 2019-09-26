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
            End.X - Start.X,
            End.Y - Start.Y);

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

        public Point? GetIntersectionPointWith(Line other)
        {
            var delta = (this.Formula.A * other.Formula.B) - (other.Formula.A * this.Formula.B);
            if(delta == 0)
                return null;

            return new Point()
            {
                X = ((other.Formula.B * this.Formula.C) - (this.Formula.B * other.Formula.C)) / delta,
                Y = ((this.Formula.A * other.Formula.C) - (other.Formula.A * this.Formula.C)) / delta
            };
        }

        public override string ToString() {
            return "<" + Start + ":" + End + ">";
        }
    }
}
