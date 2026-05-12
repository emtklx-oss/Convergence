using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;

namespace FadePlanet
{
    //Use ObjectType to quickly search for an object
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
        public int Id { get; set; } //Used to look up object in dictionary 

        // the F means they are floats. float = decimals are allowed
        public PointF Position { get; set; }
        public SizeF ObjSize { get; set; }
        public ObjectType Type { get; set; }

        // Returns the bounding box of the object.
        public RectangleF Bounds => new RectangleF(Position, ObjSize);

        #region Animations
        // Store all spritesheets for this specific object
        // Key: each state | Value: The Bitmap sheet */
        public Dictionary<ObjectState, Bitmap> Animations = new Dictionary<ObjectState, Bitmap>();
        public ObjectState CurrentState { get; set; }
        public int CurrentFrame { get; set; } = 0;

        // Call this when drawing to get the right sheet
        public Bitmap GetCurrentSheet() => Animations[CurrentState];
        #endregion

        public WorldObject(Point pos, Size size, ObjectType type = ObjectType.None)
        {
            Position = pos;
            ObjSize = size;
            Type = type;
            GameManager.SpawnObject(this);
        }

        public virtual void OnInteract(Player player)
        {
            // Handle interactions (e.g., combat, dialogue) based on object types
        }
        public virtual void OnHit(float damage) { }

    }
}
