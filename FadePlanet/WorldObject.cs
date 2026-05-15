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
        Item,
        Projectile
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
        public Size ObjSize { get; set; }
        private Image ObjImage { get; set; }

        // Renamed from ObjectType to Type to avoid conflict with the enum name
        public ObjectType Type { get; set; }

        #region Hitbox
        // =====================================================================
        // HITBOX SETTINGS — Tweak these values to resize/reposition the hitbox
        // HitboxWidth   — how wide the hitbox is in pixels
        // HitboxHeight  — how tall the hitbox is in pixels
        // HitboxOffsetX — how far right from the object's Position the hitbox starts
        // HitboxOffsetY — how far down from the object's Position the hitbox starts
        // =====================================================================
        private const float HitboxWidth = 80f;
        private const float HitboxHeight = 100f;
        private const float HitboxOffsetX = 72f;
        private const float HitboxOffsetY = 110f;

        public RectangleF Hitbox => new RectangleF(
            Position.X + HitboxOffsetX,
            Position.Y + HitboxOffsetY,
            HitboxWidth,
            HitboxHeight
        );

        public bool ShowHitbox { get; set; } = false;
        #endregion
        #region Animations
        public Dictionary<ObjectState, Bitmap> Animations = new Dictionary<ObjectState, Bitmap>();
        public ObjectState CurrentState { get; set; }
        public int CurrentFrame { get; set; } = 0;

        public Bitmap GetCurrentSheet() => Animations[CurrentState];
        #endregion

        public WorldObject(Point pos, Size size, ObjectType type = ObjectType.None, Image objImage = null)
        {
            Position = pos;
            ObjSize = size;
            Type = type;
            GameManager.SpawnObject(this);
            ObjImage = objImage;
        }
        public virtual void Draw(Graphics g)
        {
            if (ObjImage != null)
            {
                g.DrawImage(ObjImage, Position.X, Position.Y, ObjSize.Width, ObjSize.Height);
            }
        }
        
        public virtual void OnInteract(Player player) { }
    }
}