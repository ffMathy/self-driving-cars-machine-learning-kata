using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MachineLearningPractice.Models
{
    class MapNode
    {
        public Direction EntranceDirection { get; set; }
        public Direction ExitDirection { get; set; }

        public Point Position { get; set; }

        public Line[] Lines { get;set;}
    }
}
