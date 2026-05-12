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
        private float Health { get; set; } = 50f; // Default health, can be adjusted based on enemy type
        #endregion

        public Enemy(Point pos, Size size) : base(pos, size, ObjectType.Enemy)
        {
        }

        public override void OnHit(float damage)
        {
            
        }
    }
}
