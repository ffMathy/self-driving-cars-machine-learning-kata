using MachineLearningPractice.Models;
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

        public Direction GetOppositeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Bottom:
                    return Direction.Top;

                case Direction.Left:
                    return Direction.Right;

                case Direction.Top:
                    return Direction.Bottom;

                case Direction.Right:
                    return Direction.Left;

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }

        public Point GetDirectionalOffset(Direction direction)
        {
            switch (direction)
            {
                case Direction.Bottom:
                    return new Point(0, -1);

                case Direction.Left:
                    return new Point(-1, 0);

                case Direction.Top:
                    return new Point(0, 1);

                case Direction.Right:
                    return new Point(1, 0);

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }
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

        public Direction GetRandomDirection()
        {
            return (Direction)random.Next(0, 4);
        }
    }
}
