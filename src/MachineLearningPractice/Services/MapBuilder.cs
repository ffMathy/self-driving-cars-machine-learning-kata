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
                nodes.Count,
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
            return new Map()
            {
                Nodes = nodes.ToArray()
            };
        }

        private static MapNode GenerateMapSegmentNode(
            int offset,
            Point origin,
            Direction entranceDirection,
            Direction exitDirection)
        {
            if (entranceDirection == exitDirection)
            {
                throw new InvalidOperationException("Entrance and exit directions must be different.");
            }

            return new MapNode()
            {
                Offset = offset,
                EntranceDirection = entranceDirection,
                ExitDirection = exitDirection,
                ProgressLines = GetProgressLines(
                    origin,
                    entranceDirection,
                    exitDirection),
                WallLines = GetWallLines(
                    origin,
                    entranceDirection,
                    exitDirection),
                Position = new Point(
                    (decimal)Map.TileSize,
                    origin.X,
                    origin.Y)
            };
        }

        private static Line[] GetProgressLines(
            Point origin,
            Direction entranceDirection,
            Direction exitDirection)
        {
            var lines = new List<Line>();

            var combinedDirection = DirectionHelper.GetCombinedDirection(
                entranceDirection,
                exitDirection);

            var directions = new[]
            {
                combinedDirection,
                DirectionHelper.GetOppositeDirection(combinedDirection)
            };

            if (directions.Contains(Direction.BottomLeft))
            {
                lines.Add(GetBottomLeftProgressLine(origin));
            }
            else if (directions.Contains(Direction.BottomRight))
            {
                lines.Add(GetBottomRightProgressLine(origin));
            }
            else if (directions.Contains(Direction.Bottom))
            {
                lines.Add(GetBottomProgressLine(origin));
            }
            else if (directions.Contains(Direction.Right))
            {
                lines.Add(GetRightProgressLine(origin));
            }

            return lines.ToArray();
        }

        private static Line[] GetWallLines(
            Point origin,
            Direction entranceDirection,
            Direction exitDirection)
        {
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

            return lines.ToArray();
        }

        private static Line GetRightProgressLine(Point origin)
        {
            return new Line()
            {
                Start = new Point((decimal)Map.TileSize, 0m + origin.X, -0.5m + origin.Y),
                End = new Point((decimal)Map.TileSize, 0m + origin.X, 0.5m + origin.Y)
            };
        }

        private static Line GetBottomProgressLine(Point origin)
        {
            return new Line()
            {
                Start = new Point((decimal)Map.TileSize, -0.5m + origin.X, 0m + origin.Y),
                End = new Point((decimal)Map.TileSize, 0.5m + origin.X, 0m + origin.Y)
            };
        }

        private static Line GetBottomRightProgressLine(Point origin)
        {
            return new Line()
            {
                Start = new Point((decimal)Map.TileSize, 0.5m + origin.X, 0.5m + origin.Y),
                End = new Point((decimal)Map.TileSize, -0.5m + origin.X, -0.5m + origin.Y)
            };
        }

        private static Line GetBottomLeftProgressLine(Point origin)
        {
            return new Line()
            {
                Start = new Point((decimal)Map.TileSize, -0.5m + origin.X, 0.5m + origin.Y),
                End = new Point((decimal)Map.TileSize, 0.5m + origin.X, -0.5m + origin.Y)
            };
        }

        private static Line GetRightWallLine(Point origin)
        {
            return new Line()
            {
                Start = new Point((decimal)Map.TileSize, 0.5m + origin.X, -0.5m + origin.Y),
                End = new Point((decimal)Map.TileSize, 0.5m + origin.X, 0.5m + origin.Y)
            };
        }

        private static Line GetLeftWallLine(Point origin)
        {
            return new Line()
            {
                Start = new Point((decimal)Map.TileSize, -0.5m + origin.X, -0.5m + origin.Y),
                End = new Point((decimal)Map.TileSize, -0.5m + origin.X, 0.5m + origin.Y)
            };
        }

        private static Line GetTopWallLine(Point origin)
        {
            return new Line()
            {
                Start = new Point((decimal)Map.TileSize, -0.5m + origin.X, 0.5m + origin.Y),
                End = new Point((decimal)Map.TileSize, 0.5m + origin.X, 0.5m + origin.Y)
            };
        }

        private static Line GetBottomWallLine(Point origin)
        {
            return new Line()
            {
                Start = new Point((decimal)Map.TileSize, -0.5m + origin.X, -0.5m + origin.Y),
                End = new Point((decimal)Map.TileSize, 0.5m + origin.X, -0.5m + origin.Y)
            };
        }
    }
}
