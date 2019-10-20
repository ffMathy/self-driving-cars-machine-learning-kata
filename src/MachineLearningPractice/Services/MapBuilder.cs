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
            var currentNode = nodes.FirstOrDefault();
            var currentProgressLine = currentNode
                ?.ProgressLines
                ?.OrderBy(x => x.Line.Center.GetDistanceTo(currentNode.Position))
                ?.First();

            var progressLineOffset = 1;
            while (currentNode != null)
            {
                var nodeEntranceWallLine = currentNode.OpeningLines.Single(x => x.Direction == currentNode.EntranceDirection);
                var nodeProgressLines = currentNode
                    .ProgressLines
                    .OrderBy(x => x.Line.Center.GetDistanceTo(nodeEntranceWallLine.Line.Center))
                    .ToArray();

                foreach (var nodeProgressLine in nodeProgressLines)
                {
                    nodeProgressLine.Offset = progressLineOffset++;
                }

                currentNode = nodes.SingleOrDefault(x => x.Offset == currentNode.Offset + 1);
            }

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
                    offset,
                    origin,
                    entranceDirection,
                    exitDirection),
                WallLines = GetWallLines(
                    origin,
                    entranceDirection,
                    exitDirection),
                OpeningLines = new[] {
                    GetWallLineFromDirection(origin, entranceDirection),
                    GetWallLineFromDirection(origin, exitDirection)
                },
                Position = new Point(
                    (decimal)Map.TileSize,
                    origin.X,
                    origin.Y)
            };
        }

        private static WallLine GetWallLineFromDirection(Point origin, Direction direction)
        {
            switch (direction)
            {
                case Direction.Bottom:
                    return GetBottomWallLine(origin);

                case Direction.Top:
                    return GetTopWallLine(origin);

                case Direction.Right:
                    return GetRightWallLine(origin);

                case Direction.Left:
                    return GetLeftWallLine(origin);

                default:
                    throw new InvalidOperationException("Invalid direction to get wall line from: " + direction);
            }
        }

        private static ProgressLine[] GetProgressLines(
            int offset,
            Point origin,
            Direction entranceDirection,
            Direction exitDirection)
        {
            var lines = new List<ProgressLine>();

            var combinedDirection = DirectionHelper.GetCombinedDirection(
                entranceDirection,
                exitDirection);

            switch (combinedDirection)
            {
                case Direction.Right:
                    lines.AddRange(GetRightProgressLines(offset, origin));
                    break;

                case Direction.Left:
                    lines.AddRange(GetLeftProgressLines(offset, origin));
                    break;

                case Direction.Top:
                    lines.AddRange(GetTopProgressLines(offset, origin));
                    break;

                case Direction.Bottom:
                    lines.AddRange(GetBottomProgressLines(offset, origin));
                    break;

                case Direction.BottomLeft:
                    lines.AddRange(GetBottomLeftProgressLines(offset, origin));
                    break;

                case Direction.BottomRight:
                    lines.AddRange(GetBottomRightProgressLines(offset, origin));
                    break;

                case Direction.TopLeft:
                    lines.AddRange(GetTopLeftProgressLines(offset, origin));
                    break;

                case Direction.TopRight:
                    lines.AddRange(GetTopRightProgressLines(offset, origin));
                    break;
            }

            return lines.ToArray();
        }

        private static ProgressLine[] GetLeftProgressLines(int offset, Point origin)
        {
            var progressLines = GetAngledStraightProgressLines(origin, Direction.Left)
                .Select(x => new ProgressLine()
                {
                    Line = x
                })
                .ToArray();

            return progressLines;
        }

        private static ProgressLine[] GetRightProgressLines(int offset, Point origin)
        {
            var progressLines = GetAngledStraightProgressLines(origin, Direction.Right)
                .Select(x => new ProgressLine()
                {
                    Line = x
                })
                .ToArray();

            return progressLines;
        }

        private static ProgressLine[] GetBottomProgressLines(int offset, Point origin)
        {
            var progressLines = GetAngledStraightProgressLines(origin, Direction.Bottom)
                .Select(x => new ProgressLine()
                {
                    Line = x
                })
                .ToArray();

            return progressLines;
        }

        private static ProgressLine[] GetTopProgressLines(int offset, Point origin)
        {
            var progressLines = GetAngledStraightProgressLines(origin, Direction.Top)
                .Select(x => new ProgressLine()
                {
                    Line = x
                })
                .ToArray();

            return progressLines;
        }

        private static Line[] GetAngledStraightProgressLines(Point origin, Direction direction)
        {
            var originDirection = Direction.Bottom;
            var scalingFactor = (decimal)Map.TileSize;

            var lines = new[]
            {
                new Line()
                {
                    Start = new Point(scalingFactor, -0.5m + origin.X, -0.35m + origin.Y),
                    End = new Point(scalingFactor, 0.5m + origin.X, -0.35m + origin.Y)
                },
                new Line()
                {
                    Start = new Point(scalingFactor, -0.5m + origin.X, 0m + origin.Y),
                    End = new Point(scalingFactor, 0.5m + origin.X, 0m + origin.Y)
                },
                new Line()
                {
                    Start = new Point(scalingFactor, -0.5m + origin.X, 0.35m + origin.Y),
                    End = new Point(scalingFactor, 0.5m + origin.X, 0.35m + origin.Y)
                }
            };

            var angle = DirectionHelper.GetClockwiseAngleBetweenDirections(originDirection, direction);

            return lines
                .Select(x => x.RotateAround(
                    new Point(scalingFactor, origin.X, origin.Y),
                    angle))
                .ToArray();
        }

        private static ProgressLine[] GetBottomRightProgressLines(int offset, Point origin)
        {
            var progressLines = GetAngledCornerProgressLines(origin, Direction.BottomRight)
                .Select(x => new ProgressLine()
                {
                    Line = x
                })
                .ToArray();

            return progressLines;
        }

        private static ProgressLine[] GetTopLeftProgressLines(int offset, Point origin)
        {
            var progressLines = GetAngledCornerProgressLines(origin, Direction.TopLeft)
                .Select(x => new ProgressLine()
                {
                    Line = x
                })
                .ToArray();

            return progressLines;
        }

        private static Line[] GetAngledCornerProgressLines(Point origin, Direction direction)
        {
            var originDirection = Direction.TopRight;
            var scalingFactor = (decimal)Map.TileSize;

            var lines = new[] {
                new Line()
                {
                    Start = new Point(scalingFactor, -0.5m + origin.X, -0m + origin.Y),
                    End = new Point(scalingFactor, 0.5m + origin.X, -0.5m + origin.Y)
                },
                new Line()
                {
                    Start = new Point(scalingFactor, 0.5m + origin.X, -0.5m + origin.Y),
                    End = new Point(scalingFactor, -0.5m + origin.X, 0.5m + origin.Y)
                },
                new Line()
                {
                    Start = new Point(scalingFactor, 0m + origin.X, 0.5m + origin.Y),
                    End = new Point(scalingFactor, 0.5m + origin.X, -0.5m + origin.Y)
                }
            };

            var angle = DirectionHelper.GetClockwiseAngleBetweenDirections(originDirection, direction);

            return lines
                .Select(x => x.RotateAround(
                    new Point(scalingFactor, origin.X, origin.Y),
                    angle))
                .ToArray();
        }

        private static ProgressLine[] GetBottomLeftProgressLines(int offset, Point origin)
        {
            var progressLines = GetAngledCornerProgressLines(origin, Direction.BottomLeft)
                .Select(x => new ProgressLine()
                {
                    Line = x
                })
                .ToArray();

            return progressLines;
        }

        private static ProgressLine[] GetTopRightProgressLines(int offset, Point origin)
        {
            var progressLines = GetAngledCornerProgressLines(origin, Direction.TopRight)
                .Select(x => new ProgressLine()
                {
                    Line = x
                })
                .ToArray();

            return progressLines;
        }

        private static WallLine[] GetWallLines(
            Point origin,
            Direction entranceDirection,
            Direction exitDirection)
        {
            var openingDirections = new[] {
                entranceDirection,
                exitDirection
            };

            var lines = new List<WallLine>();

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

        private static WallLine GetRightWallLine(Point origin)
        {
            return new WallLine()
            {
                Direction = Direction.Right,
                Line = new Line()
                {
                    Start = new Point((decimal)Map.TileSize, 0.5m + origin.X, -0.5m + origin.Y),
                    End = new Point((decimal)Map.TileSize, 0.5m + origin.X, 0.5m + origin.Y)
                }
            };
        }

        private static WallLine GetLeftWallLine(Point origin)
        {
            return new WallLine()
            {
                Direction = Direction.Left,
                Line = new Line()
                {
                    Start = new Point((decimal)Map.TileSize, -0.5m + origin.X, -0.5m + origin.Y),
                    End = new Point((decimal)Map.TileSize, -0.5m + origin.X, 0.5m + origin.Y)
                }
            };
        }

        private static WallLine GetTopWallLine(Point origin)
        {
            return new WallLine()
            {
                Direction = Direction.Top,
                Line = new Line()
                {
                    Start = new Point((decimal)Map.TileSize, -0.5m + origin.X, -0.5m + origin.Y),
                    End = new Point((decimal)Map.TileSize, 0.5m + origin.X, -0.5m + origin.Y)
                }
            };
        }

        private static WallLine GetBottomWallLine(Point origin)
        {
            return new WallLine()
            {
                Direction = Direction.Bottom,
                Line = new Line()
                {
                    Start = new Point((decimal)Map.TileSize, -0.5m + origin.X, 0.5m + origin.Y),
                    End = new Point((decimal)Map.TileSize, 0.5m + origin.X, 0.5m + origin.Y)
                }
            };
        }
    }
}
