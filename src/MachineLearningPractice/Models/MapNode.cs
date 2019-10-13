using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MachineLearningPractice.Models
{
    public class MapNode
    {
        public Direction EntranceDirection { get; set; }
        public Direction ExitDirection { get; set; }

        public Point Position { get; set; }

        public Line[] ProgressLines { get; set; }

        public Line[] WallLines { get; set; }

        public int Offset { get; set; }
    }
}
