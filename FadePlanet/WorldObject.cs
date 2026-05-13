using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadePlanet
{
    public enum ObjectType
    {
        None,
        Wall,
        Enemy,
        Friendly,
        Player,
        Item
    }

    public enum ObjectState
    {
        None,
        Idle,
        Move,
        Attack,
        Death
    }

    public enum ElementType
    {
        Water,
        Fire,
        Earth,
        Air
    }

    public class WorldObject
    {
        public int Id { get; set; }

        public PointF Position { get; set; }
        public SizeF ObjSize { get; set; }

        // Renamed from ObjectType to Type to avoid conflict with the enum name
        public ObjectType Type { get; set; }

        public RectangleF Bounds => new RectangleF(Position, ObjSize);

        #region Animations
        public Dictionary<ObjectState, Bitmap> Animations = new Dictionary<ObjectState, Bitmap>();
        public ObjectState CurrentState { get; set; }
        public int CurrentFrame { get; set; } = 0;

        public Bitmap GetCurrentSheet() => Animations[CurrentState];
        #endregion

        public WorldObject(Point pos, Size size, ObjectType type = ObjectType.None)
        {
            Position = pos;
            ObjSize = size;
            Type = type;
            GameManager.SpawnObject(this);
        }

        public virtual void OnInteract(Player player) { }
    }
}