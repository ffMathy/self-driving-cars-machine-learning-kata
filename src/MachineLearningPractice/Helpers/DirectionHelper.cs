﻿using MachineLearningPractice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MachineLearningPractice.Helpers
{
    public class DirectionHelper
    {
        private readonly Random random;

        public DirectionHelper(Random random)
        {
            this.random = random;
        }

        public Direction GetRandomDirectionOtherThan(
            params Direction[] directions)
        {
            while (true)
            {
                var direction = GetRandomDirection();
                if (!directions.Contains(direction))
                    return direction;
            }
        }

        public static Direction GetCombinedDirection(
            Direction a,
            Direction b)
        {
            if(b == GetOppositeDirection(a))
                return b;

            if (a == Direction.Left) { 
                switch(b)
                {
                    case Direction.Bottom:
                        return Direction.BottomRight;

                    case Direction.Top:
                        return Direction.TopRight;
                }
            }

            if(a == Direction.Right)
            {
                switch (b)
                {
                    case Direction.Bottom:
                        return Direction.BottomLeft;

                    case Direction.Top:
                        return Direction.TopLeft;
                }
            }

            if (a == Direction.Top)
            {
                switch (b)
                {
                    case Direction.Left:
                        return Direction.BottomLeft;

                    case Direction.Right:
                        return Direction.BottomRight;
                }
            }

            if (a == Direction.Bottom)
            {
                switch (b)
                {
                    case Direction.Left:
                        return Direction.TopLeft;

                    case Direction.Right:
                        return Direction.TopRight;
                }
            }

            return GetCombinedDirection(b, a);
        }

        public Direction GetRandomDirection()
        {
            return (Direction)random.Next(0, 4);
        }

        public static Direction GetOppositeDirection(Direction direction)
        {
            return direction switch
            {
                Direction.Bottom => Direction.Top,
                Direction.Left => Direction.Right,
                Direction.Top => Direction.Bottom,
                Direction.Right => Direction.Left,
                Direction.BottomLeft => Direction.TopRight,
                Direction.BottomRight => Direction.TopLeft,
                Direction.TopLeft => Direction.BottomRight,
                Direction.TopRight => Direction.BottomLeft,

                _ => throw new ArgumentOutOfRangeException(nameof(direction)),
            };
        }

        public static bool IsPointInDirectionOfSensorLine(Line sensorLine, Point intersectionPoint)
        {
            var lineToIntersectionPoint = new Line()
            {
                Start = sensorLine.Start,
                End = intersectionPoint
            };

            var angle = sensorLine.GetAngleTo(lineToIntersectionPoint);
            return angle < 90 || angle > 270;
        }

        public static bool IsPointOutsideLineBoundaries(Line line, Point intersectionPoint)
        {
            var lowestLineX = Math.Min(
                line.Start.X,
                line.End.X);

            var lowestLineY = Math.Min(
                line.Start.Y,
                line.End.Y);

            var largestLineX = Math.Max(
                line.Start.X,
                line.End.X);

            var largestLineY = Math.Max(
                line.Start.Y,
                line.End.Y);

            const int minimumDelta = 1;

            var isIntersectionOutsideLineBoundaries =
                intersectionPoint.X - lowestLineX < -minimumDelta ||
                intersectionPoint.Y - lowestLineY < -minimumDelta ||
                intersectionPoint.X - largestLineX > minimumDelta ||
                intersectionPoint.Y - largestLineY > minimumDelta;

            return isIntersectionOutsideLineBoundaries;
        }

        public static Point GetDirectionalOffset(Direction direction)
        {
            return direction switch
            {
                Direction.Bottom => new Point(0, -1),
                Direction.Left => new Point(-1, 0),
                Direction.Top => new Point(0, 1),
                Direction.Right => new Point(1, 0),

                _ => throw new ArgumentOutOfRangeException(nameof(direction)),
            };
        }
    }
}
