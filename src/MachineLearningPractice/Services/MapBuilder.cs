using MachineLearningPractice.Helpers;
using MachineLearningPractice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MachineLearningPractice.Services
{
    public class MapBuilder
    {
        private readonly List<MapNode> nodes;

        private Point origin;
        private Direction previousDirection;

        public Point CurrentPoint => origin;

        public MapBuilder()
        {
            this.nodes = new List<MapNode>();
            this.origin = new Point(0, 0);
        }

        public MapBuilder MoveInDirection(Direction direction)
        {
            var node = GenerateMapSegmentNode(
                origin,
                DirectionHelper.GetOppositeDirection(previousDirection),
                direction);

            var offset = DirectionHelper.GetDirectionalOffset(direction);
            var newPoint = new Point(
                origin.X + offset.X,
                origin.Y + offset.Y);
            origin = newPoint;
            previousDirection = direction;

            nodes.Add(node);

            return this;
        }

        public Map Build()
        {
            return new Map() {
                Nodes = nodes.ToArray()
            };
        }

        private static MapNode GenerateMapSegmentNode(
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
                Position = new Point(
                    Map.TileSize,
                    origin.X, 
                    origin.Y)
            };
        }

        private static Line GetRightWallLine(Point origin)
        {
            return new Line()
            {
                Start = new Point(Map.TileSize, 0.5 + origin.X, -0.5 + origin.Y),
                End = new Point(Map.TileSize, 0.5 + origin.X, 0.5 + origin.Y)
            };
        }

        private static Line GetLeftWallLine(Point origin)
        {
            return new Line()
            {
                Start = new Point(Map.TileSize, -0.5 + origin.X, -0.5 + origin.Y),
                End = new Point(Map.TileSize, -0.5 + origin.X, 0.5 + origin.Y)
            };
        }

        private static Line GetTopWallLine(Point origin)
        {
            return new Line()
            {
                Start = new Point(Map.TileSize, -0.5 + origin.X, 0.5 + origin.Y),
                End = new Point(Map.TileSize, 0.5 + origin.X, 0.5 + origin.Y)
            };
        }

        private static Line GetBottomWallLine(Point origin)
        {
            return new Line()
            {
                Start = new Point(Map.TileSize, -0.5 + origin.X, -0.5 + origin.Y),
                End = new Point(Map.TileSize, 0.5 + origin.X, -0.5 + origin.Y)
            };
        }
    }
}
