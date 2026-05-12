using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadePlanet
{
    public class Player : WorldObject
    {
        #region Player Stats
        private float Health = 100f;
        private readonly float MaxHealth = 100f;

        private float Stamina = 100f;
        private readonly float MaxStamina = 100f;
        #endregion

        public Player(Point pos, Size size) : base(pos, size, ObjectType.Player) { }
        
        
        public override void OnHit(float damage)
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
            // Handle player death (e.g., show game over screen, reset level, etc.)
        }

    }
}
