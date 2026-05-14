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
        public IElement CurrentElement { get; private set; }
        public IElement PendingElement { get; private set; } // The scroll we are switching TO

        private Dictionary<Keys, IElement> _abilities = new Dictionary<Keys, IElement>
        {
            { Keys.D1, new WaterScroll() },
            { Keys.D2, new FireScroll() },
            { Keys.D3, new EarthScroll() },
            { Keys.D4, new AirScroll() }
        };

        // Cooldown flag — locked while scroll animation is playing
        public bool ScrollSwitchLocked { get; set; } = false;
        #endregion

        #region Hitbox
        // =====================================================================
        // HITBOX SETTINGS — Tweak these values to resize/reposition the hitbox
        // HitboxWidth   — how wide the hitbox is in pixels
        // HitboxHeight  — how tall the hitbox is in pixels
        // HitboxOffsetX — how far right from the player's Position the hitbox starts
        // HitboxOffsetY — how far down from the player's Position the hitbox starts
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

        public Player(Point pos, Size size) : base(pos, size, ObjectType.Player)
        {
            // Set default scroll to AirScroll on game start
            CurrentElement = _abilities[Keys.D4];
        }

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
        public void HandleKeyDown(KeyEventArgs e) { }

        public void HandleKeyUp(KeyEventArgs e) { }

        public void HandleMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                CurrentElement?.PrimaryAttack(this);
            }
            if (e.Button == MouseButtons.Right)
            {
                CurrentElement?.SecondaryAttack(this);
            }
        }

        public void HandleScrollSwitch(Keys key)
        {
            // Block switching if cooldown is active
            if (ScrollSwitchLocked) return;

            if (_abilities.ContainsKey(key))
            {
                if (CurrentElement == _abilities[key])
                {
                    Console.WriteLine($"Already active");
                }
                else
                {
                    // Store the scroll we want to switch to
                    PendingElement = _abilities[key];
                    ScrollSwitchLocked = true;
                    Console.WriteLine($"Switching to {PendingElement.Type.ToString()}...");
                }
            }
        }

        // Called by UI once the closing animation finishes
        public void ConfirmScrollSwitch()
        {
            CurrentElement = PendingElement;
            PendingElement = null;
            Console.WriteLine($"Switched to {CurrentElement.Type.ToString()}!");
        }

        // Called by UI once the opening animation finishes
        public void UnlockScrollSwitch()
        {
            ScrollSwitchLocked = false;
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
        public void TestTakeDamage(int amount)
        {
            TakeDamage(amount);
        }
        #endregion
    }
}