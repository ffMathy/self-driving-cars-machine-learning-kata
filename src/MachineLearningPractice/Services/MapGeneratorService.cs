using MachineLearningPractice.Helpers;
using MachineLearningPractice.Models;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MachineLearningPractice.Services
{
    public class MapGeneratorService
    {
        private readonly Random random;
        private readonly DirectionHelper directionHelper;

        public MapGeneratorService(
            Random random,
            DirectionHelper directionHelper)
        {
            this.random = random;
            this.directionHelper = directionHelper;
        }

        public Map PickRandomPredefinedMap()
        {
            //return CreateMapBuilder()
            //    .MoveInDirection(Direction.Top)
            //    .MoveInDirection(Direction.Top)
            //    .MoveInDirection(Direction.Top)
            //    .MoveInDirection(Direction.Top)
            //    .Build();

            //return CreateMapBuilder()
            //    .MoveInDirection(Direction.Top)
            //    .MoveInDirection(Direction.Left)
            //    .MoveInDirection(Direction.Bottom)
            //    .MoveInDirection(Direction.Bottom)
            //    .MoveInDirection(Direction.Right)
            //    .MoveInDirection(Direction.Top)
            //    .Build();

            var maps = new List<Map>();

            maps.Add(CreateMapBuilder()
                .MoveInDirection(Direction.Top)
                .MoveInDirection(Direction.Right)
                .MoveInDirection(Direction.Right)
                .MoveInDirection(Direction.Top)
                .MoveInDirection(Direction.Top)
                .MoveInDirection(Direction.Left)
                .MoveInDirection(Direction.Top)
                .MoveInDirection(Direction.Right)
                .MoveInDirection(Direction.Top)
                .MoveInDirection(Direction.Left)
                .MoveInDirection(Direction.Left)
                .MoveInDirection(Direction.Left)
                .MoveInDirection(Direction.Bottom)
                .MoveInDirection(Direction.Left)
                .MoveInDirection(Direction.Left)
                .MoveInDirection(Direction.Bottom)
                .MoveInDirection(Direction.Right)
                .MoveInDirection(Direction.Bottom)
                .MoveInDirection(Direction.Right)
                .MoveInDirection(Direction.Bottom)
                .MoveInDirection(Direction.Left)
                .MoveInDirection(Direction.Left)
                .MoveInDirection(Direction.Top)
                .MoveInDirection(Direction.Left)
                .MoveInDirection(Direction.Bottom)
                .MoveInDirection(Direction.Bottom)
                .MoveInDirection(Direction.Right)
                .MoveInDirection(Direction.Right)
                .MoveInDirection(Direction.Right)
                .MoveInDirection(Direction.Bottom)
                .MoveInDirection(Direction.Right)
                .MoveInDirection(Direction.Top)
                .Build());

            var index = random.Next(0, maps.Count);
            return maps[index];
        }

        private static MapBuilder CreateMapBuilder()
        {
            return new MapBuilder();
        }

        public Map GenerateRandomMap()
        {
            while (true)
            {
                var mapBuilder = CreateMapBuilder();
                var directions = new List<Direction>();

                var seenPoints = new HashSet<Point>();

                var repeatedFailureCount = 0;

                var currentDirection = Direction.Top;
                while (repeatedFailureCount < 100)
                {
                    var previousDirection = currentDirection;
                    var newDirection = directionHelper.GetRandomDirectionOtherThan(
                        previousDirection,
                        DirectionHelper.GetOppositeDirection(
                            previousDirection));

                    mapBuilder.MoveInDirection(newDirection);

                    repeatedFailureCount = 0;

                    var currentPoint = mapBuilder.CurrentPoint;
                    currentDirection = newDirection;

                    directions.Add(newDirection);
                    seenPoints.Add(currentPoint);

                    if (currentPoint.X == 0 && currentPoint.Y == 0)
                    {
                        if (directions.Count == 4)
                            break;

                        return mapBuilder.Build();
                    }
                }
            }

            throw new NotImplementedException();
        }
    }
}
