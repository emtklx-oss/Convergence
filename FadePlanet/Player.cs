using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace FadePlanet
{
    public class Player : WorldObject
    {
        #region Player Stats
        private int Health = 100;
        private readonly int MaxHealth = 100;

        private int Stamina = 100;
        private readonly int MaxStamina = 100;
        #endregion

        #region Player Inventory
        private const int hotbarLength = 4;
        private WorldObject[] Inventory = new WorldObject[hotbarLength]; // Array to hold hotbar items
        #endregion

        public Player(Point pos, Size size) : base(pos, size, ObjectType.Player) { }
        
        
        public override void OnHit(int damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Health = 0;
                OnDeath();
            }
        }

        // Handle player death (e.g., show game over screen, reset level, etc.)
        public override void OnDeath() { }

    }
}
