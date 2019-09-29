using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLearningPractice.Helpers
{
    public class MathHelper
    {
        public static bool IsEqualWithinRange(double a, double b, double delta)
        {
            return Math.Abs(a - b) <= delta;
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * (180 / Math.PI);
        }
    }
}
