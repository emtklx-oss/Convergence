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
        private int TokenCount { get; set; } = 0;
        private int PotionCount { get; set; } = 0;
        #endregion
        #region Player Abilities 
        private readonly List<Scroll> PlrScrolls = new List<Scroll>();
        public ElementType ActiveScrollType { get; private set; }
        #endregion

        public Player(Point pos, Size size) : base(pos, size, ObjectType.Player) { }

        public void SwitchActiveScroll()
        {
           
        }

        #region Damage/Death Functions
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
        #endregion 


    }
}
