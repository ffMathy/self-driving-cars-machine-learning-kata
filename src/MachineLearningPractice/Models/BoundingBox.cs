using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MachineLearningPractice.Models
{
    public class BoundingBox
    {
        public Size Size { get; set; }

        public Point Center => new Point(
            Location.X + Size.Width / 2, 
            Location.Y + Size.Height / 2);

        public Point Location { get; set; }

        public BoundingBox()
        {
            Location = new Point();
            Size = new Size();
        }
    }
}
