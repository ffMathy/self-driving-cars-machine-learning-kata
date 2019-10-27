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
            Location.X + (Size.Width / 2), 
            Location.Y + (Size.Height / 2));

        public Point Location { get; set; }

        public bool IsWithin(BoundingBox other)
        {
            return 
                Location.X > other.Location.X &&
                Location.Y > other.Location.Y &&
                Location.X + Size.Width < other.Location.X + other.Size.Width &&
                Location.Y + Size.Height < other.Location.Y + other.Size.Height;
        }

        public BoundingBox()
        {
            Location = new Point();
            Size = new Size();
        }
    }
}
