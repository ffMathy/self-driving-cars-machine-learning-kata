using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MachineLearningPractice.Models
{
    public class ProgressLine
    {
        public Line Line { get; set; }
        public int Offset { get; set; }
    }

    public class WallLine
    {
        public Line Line { get; set; }
        public Direction Direction { get;set;}
    }

    public class MapNode
    {
        public Direction EntranceDirection { get; set; }
        public Direction ExitDirection { get; set; }

        public Point Position { get; set; }

        public ProgressLine[] ProgressLines { get; set; }

        public WallLine[] WallLines { get; set; }
        public WallLine[] OpeningLines { get; set; }

        public int Offset { get; set; }
    }
}
