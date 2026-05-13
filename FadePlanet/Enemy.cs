using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadePlanet
{
    

    internal class Enemy : WorldObject
    {
        #region Enemy Stats
        public ElementType ElementType { get; private set; }
        private int Health { get; set; } = 50; // Default health, can be adjusted based on enemy type
        #endregion

        public Enemy(Point pos, Size size) : base(pos, size, ObjectType.Enemy)
        {
        }

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
            // Handle enemy death (e.g., remove from game, drop loot, etc.)
            // For now, we just remove it from the game
            GameManager.DespawnObject(this);
        }
    }
}
