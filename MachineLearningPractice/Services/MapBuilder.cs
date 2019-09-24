using MachineLearningPractice.Helpers;
using MachineLearningPractice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MachineLearningPractice.Services
{
    class MapBuilder
    {
        private readonly List<MapNode> nodes;
        private readonly DirectionHelper directionHelper;

        private Point origin;
        private Direction previousDirection;

        public Point CurrentPoint => origin;

        public MapBuilder(DirectionHelper directionHelper)
        {
            this.nodes = new List<MapNode>();

            this.origin = new Point(0, 0);
            this.directionHelper = directionHelper;
        }

        public MapBuilder MoveInDirection(Direction direction)
        {
            var node = GenerateMapSegmentNode(
                origin,
                directionHelper.GetOppositeDirection(previousDirection),
                direction);

            var offset = directionHelper.GetDirectionalOffset(direction);
            var newPoint = new Point(
                origin.X + offset.X,
                origin.Y + offset.Y);
            origin = newPoint;
            previousDirection = direction;

            nodes.Add(node);

            return this;
        }

        public MapNode[] Build()
        {
            return nodes.ToArray();
        }

        private MapNode GenerateMapSegmentNode(
            Point origin,
            Direction entranceDirection,
            Direction exitDirection)
        {
            if (entranceDirection == exitDirection)
            {
                throw new InvalidOperationException("Entrance and exit directions must be different.");
            }

            var openingDirections = new[] {
                entranceDirection,
                exitDirection
            };

            var lines = new List<Line>();

            if (!openingDirections.Contains(Direction.Bottom))
                lines.Add(GetBottomWallLine(origin));

            if (!openingDirections.Contains(Direction.Top))
                lines.Add(GetTopWallLine(origin));

            if (!openingDirections.Contains(Direction.Left))
                lines.Add(GetLeftWallLine(origin));

            if (!openingDirections.Contains(Direction.Right))
                lines.Add(GetRightWallLine(origin));

            return new MapNode()
            {
                EntranceDirection = entranceDirection,
                ExitDirection = exitDirection,
                Lines = lines.ToArray(),
                Position = origin
            };
        }

        private static Line GetRightWallLine(Point origin)
        {
            return new Line()
            {
                Start = new Point(0.5 + origin.X, -0.5 + origin.Y),
                End = new Point(0.5 + origin.X, 0.5 + origin.Y)
            };
        }

        private static Line GetLeftWallLine(Point origin)
        {
            return new Line()
            {
                Start = new Point(-0.5 + origin.X, -0.5 + origin.Y),
                End = new Point(-0.5 + origin.X, 0.5 + origin.Y)
            };
        }

        private static Line GetTopWallLine(Point origin)
        {
            return new Line()
            {
                Start = new Point(-0.5 + origin.X, 0.5 + origin.Y),
                End = new Point(0.5 + origin.X, 0.5 + origin.Y)
            };
        }

        private static Line GetBottomWallLine(Point origin)
        {
            return new Line()
            {
                Start = new Point(-0.5 + origin.X, -0.5 + origin.Y),
                End = new Point(0.5 + origin.X, -0.5 + origin.Y)
            };
        }
    }
}
