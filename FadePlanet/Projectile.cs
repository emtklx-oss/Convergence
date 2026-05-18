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
        private readonly float Speed = 10.0f;
        private PointF Direction;
        private int ProjectileDamage = 15;

        public Projectile(PointF pos, SizeF size, ElementType element, PointF dir, ObjectType type = ObjectType.Projectile) : base(pos, size, type)
        {
            ProjType = element;
            Direction = dir;
            SetType();
        }

        private void SetType()
        {
            switch (ProjType)
            {
                case ElementType.Air:
                    Col = Color.White;
                    ProjectileDamage = 10;
                    break;
                case ElementType.Water:
                    Col = Color.Blue;
                    ProjectileDamage = 20;
                    break;
                case ElementType.Fire:
                    Col = Color.Red;
                    ProjectileDamage = 25;
                    break;
                case ElementType.Earth:
                    Col = Color.Gray;
                    ProjectileDamage = 50;
                    break;
            }
        }

        public override void Draw(Graphics g)
        {
            using (Pen pen = new Pen(Col))
            {
                RectangleF rect = new RectangleF(Position, ObjSize);
                g.FillEllipse(new SolidBrush(Col), rect);
            }
        }

        public void Update()
        {
            float x = Position.X;
            float y = Position.Y;

            PointF pos = new PointF(x + Direction.X * Speed, y + Direction.Y * Speed);

            Position = pos;

            // Check for collisions with enemies and walls
            CheckImpact();

            // Despawn if off-screen
            if (Position.X < -50 || Position.X > 1350) 
            {
                GameManager.DespawnObject(this);
            }
        }

        public void CheckImpact()
        {
            // Check collision with enemies
            if (GameManager.AllObjects.TryGetValue(ObjectType.Enemy, out var enemyDict))
            {
                foreach (Enemy enemy in enemyDict.Values.ToList())
                {
                    if (Bounds.IntersectsWith(enemy.Bounds))
                    {
                        enemy.TakeDamage(ProjectileDamage, Position);
                        GameManager.DespawnObject(this);
                        return;
                    }
                }
            }

            // Check collision with walls
            if (GameManager.AllObjects.TryGetValue(ObjectType.Wall, out var wallDict))
            {
                foreach (WorldObject wall in wallDict.Values)
                {
                    if (Bounds.IntersectsWith(wall.Hitbox))
                    {
                        GameManager.DespawnObject(this);
                        return;
                    }
                }
            }
        }
    }
}