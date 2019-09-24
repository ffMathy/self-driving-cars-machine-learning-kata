﻿using MachineLearningPractice.Helpers;
using MachineLearningPractice.Models;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MachineLearningPractice.Services
{
    class MapGeneratorService
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
            var maps = new List<MapNode[]>();

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
            var nodes = maps[index];

            return new Map()
            {
                Nodes = nodes
            };
        }

        private MapBuilder CreateMapBuilder()
        {
            return new MapBuilder(directionHelper);
        }

        public Map GenerateRandomMap()
        {
            while (true)
            {
                var mapBuilder = new MapBuilder(directionHelper);
                var directions = new List<Direction>();

                var currentPoint = new Point();
                var seenPoints = new HashSet<Point>();

                var repeatedFailureCount = 0;

                var currentDirection = Direction.Top;
                while (repeatedFailureCount < 100)
                {
                    var previousDirection = currentDirection;
                    var newDirection = directionHelper.GetRandomDirectionOtherThan(
                        previousDirection,
                        directionHelper.GetOppositeDirection(
                            previousDirection));

                    mapBuilder.MoveInDirection(newDirection);

                    repeatedFailureCount = 0;

                    currentPoint = mapBuilder.CurrentPoint;
                    currentDirection = newDirection;

                    directions.Add(newDirection);
                    seenPoints.Add(currentPoint);

                    if (currentPoint.X == 0 && currentPoint.Y == 0)
                    {
                        if (directions.Count == 4)
                            break;

                        return new Map() {
                            Nodes = mapBuilder.Build()
                        };
                    }
                }
            }

            throw new NotImplementedException();
        }
    }
}
