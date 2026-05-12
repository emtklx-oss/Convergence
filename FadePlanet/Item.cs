using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadePlanet
{
    internal class Item : WorldObject
    {
        private string Name { get; set; }
        private string Description { get; set; }
        private const int MaxStackSize = 99;
        private int CurrentStackSize { get; set; } = 0;

        public Item(Point pos, Size size) : base(pos, size, ObjectType.Item) { }
    }
}
