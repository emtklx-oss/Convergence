using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FadePlanet.Abilities;

namespace FadePlanet
{
    public class Player : WorldObject
    {
        #region Player Stats
        public int Health { get; private set; } = 100;
        public int MaxHealth { get; private set; } = 100;

        public int Stamina { get; private set; } = 100;
        public int MaxStamina { get; private set; } = 100;
        #endregion

        #region Player Inventory
        private int TokenCount { get; set; } = 0;
        private int PotionCount { get; set; } = 0;
        #endregion

        #region Player Abilities
        public IElement CurrentElement { get; private set; } //Current scroll

        // Dictionary to store our element instances
        private Dictionary<Keys, IElement> _abilities = new Dictionary<Keys, IElement>
        {
            { Keys.D1, new WaterScroll() },
            { Keys.D2, new FireScroll() },
            { Keys.D3, new EarthScroll() },
            { Keys.D4, new AirScroll() }
        };
        #endregion

        #region Hitbox
        // =====================================================================
        // HITBOX SETTINGS — Tweak these values to resize/reposition the hitbox
        // HitboxWidth  — how wide the hitbox is in pixels
        // HitboxHeight — how tall the hitbox is in pixels
        // HitboxOffsetX — how far right from the player's Position the hitbox starts
        // HitboxOffsetY — how far down from the player's Position the hitbox starts
        // =====================================================================
        private const float HitboxWidth = 80f;
        private const float HitboxHeight = 100f;
        private const float HitboxOffsetX = 72f; // Centers the hitbox on the 224px wide sprite
        private const float HitboxOffsetY = 110f; // Pushes it down to the body/feet area

        // This is the hitbox other objects should check against for combat/interaction
        public RectangleF Hitbox => new RectangleF(
            Position.X + HitboxOffsetX,
            Position.Y + HitboxOffsetY,
            HitboxWidth,
            HitboxHeight
        );

        // Toggle this to show/hide the hitbox visually
        public bool ShowHitbox { get; set; } = false;
        #endregion

        public Player(Point pos, Size size) : base(pos, size, ObjectType.Player) { }


        #region Hitbox Drawing
        public void DrawHitbox(Graphics g)
        {
            if (!ShowHitbox) return;

            using (Pen hitboxPen = new Pen(Color.Red, 2f))
            {
                g.DrawRectangle(hitboxPen, Hitbox.X, Hitbox.Y, Hitbox.Width, Hitbox.Height);
            }
        }
        #endregion


        #region Input
        // Input booleans
        private bool moveUp, moveDown, moveLeft, moveRight;
        
        public void HandleKeyDown(KeyEventArgs e)
        {
            
        }
        public void HandleKeyUp(KeyEventArgs e)
        {
            
        }
        public void HandleMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                CurrentElement?.PrimaryAttack(this.Position);
            }
            if (e.Button == MouseButtons.Right)
            {
                CurrentElement?.SecondaryAttack(this.Position);
            }
        }
        public void HandleScrollSwitch(Keys key)
        {
            if (_abilities.ContainsKey(key))
            {
                if (CurrentElement == _abilities[key])
                {
                    //In case we want special feedback
                    Console.WriteLine($"Already active");
                }
                else
                {
                    CurrentElement = _abilities[key];
                    Console.WriteLine($"Switched to {CurrentElement.Type.ToString()}!");
                }
                    
            }
        }

        #endregion


        #region Damage/Death Functions
        public void TakeDamage(int damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Health = 0;
                OnDeath();
            }
        }

        public void OnDeath()
        {
            // Handle player death here later
        }
        #endregion

        #region Testing
      
        #endregion
    }
}