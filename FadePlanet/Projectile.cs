using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadePlanet
{
    internal class Projectile : WorldObject
    {
        ElementType ProjType { get; set; }
        private Color Col;
        private readonly float Speed = 4.0f;

        public Projectile(Point pos, Size size, ElementType element, ObjectType type = ObjectType.None) : base(pos, size, type)
        {
            ProjType = element;
            SetColor();
        }

        private void SetColor()
        {
            switch (ProjType)
            {
                case ElementType.Air:
                    Col = Color.White;
                    break;
                case ElementType.Water:
                    Col = Color.Blue;
                    break;
                case ElementType.Fire:
                    Col = Color.Red;
                    break;
                case ElementType.Earth:
                    Col = Color.Gray;
                    break;
            }
        }

        public override void Draw(Graphics g)
        {
            using (Pen pen = new Pen(Col))
            {
                RectangleF rect = new RectangleF(Position, ObjSize);
                g.DrawEllipse(pen, rect);
            }
        }

        public void Update()
        {
            // Movement logic goes here
        }
    }
}