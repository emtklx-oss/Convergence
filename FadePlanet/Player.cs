using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadePlanet
{
    public class Player : WorldObject
    {
        #region Player Stats
        private int Health { get; set; } = 100;
        private int MaxHealth { get; set; } = 100;

        private int Stamina { get; set; } = 100;
        private int MaxStamina { get; set; } = 100;
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

        public void SwitchActiveScroll() { }

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

        public override void OnDeath() { }
        #endregion
    }
}