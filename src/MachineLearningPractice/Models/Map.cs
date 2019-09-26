using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningPractice.Models
{
    public class Map
    {
        public const double TileSize = 100;
        public const double HalfTileSize = TileSize / 2;

        public MapNode[] Nodes { get; set; }
    }
}
