using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLearningPractice.Helpers
{
    public class MathHelper
    {
        private readonly Random random;

        public MathHelper(
            Random random)
        {
            this.random = random;
        }

        public static bool IsEqualWithinRange(decimal a, decimal b, double delta)
        {
            return Math.Abs((double)a - (double)b) <= delta;
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * (180 / Math.PI);
        }
    }
}
