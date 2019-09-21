using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningPractice.Models
{
    class Map
    {
        public MapNode[] Nodes { get; set; }

        public bool IsCarCollidingWithWall(Car car)
        {
            var carX = car.Location.X;
            var carY = car.Location.Y;
            var carWidth = car.Size.Width;
            var carHeight = car.Size.Height;

            foreach (var node in Nodes)
            {
                foreach(var line in node.Lines)
                {
                }
            }
        }
    }
}
