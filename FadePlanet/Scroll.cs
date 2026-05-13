using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadePlanet
{
    public class Scroll : Item
    {
        private ElementType ElementalType { get; set; }

        public Scroll(Point pos, Size size, ElementType type) : base(pos, size)
        {
            ElementalType = type;
        }


    }
}
