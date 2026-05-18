using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadePlanet
{
    public class Abilities
    {
        public interface IElement
        {
            ElementType Type { get; }
            float StaminaCost  { get; }
            void PrimaryAttack(Player player);
            
        }

        public class FireScroll : IElement
        {
            public ElementType Type => ElementType.Fire;
            public float StaminaCost { get; } = 5f;
            public void PrimaryAttack(Player player)
            {
                if (!player.CanUseAbility(StaminaCost)) return;
                player.UseStamina(StaminaCost);
                ShootFireball(player);
                Console.WriteLine($"Primary attack of type: {Type}");
            }
            
            public void ShootFireball(Player player)
            {
                PointF dir = player.GetAttackDirection();

                // Calculate spawn position based on direction
                float posX = player.Position.X + player.ObjSize.Width / 2;
                float posY = player.Position.Y + player.ObjSize.Height / 2;

                // Offset spawn position in the direction being fired
                if (dir.X < 0) posX = player.Position.X;
                if (dir.X > 0) posX = player.Position.X + player.ObjSize.Width;
                if (dir.Y < 0) posY = player.Position.Y;
                if (dir.Y > 0) posY = player.Position.Y + player.ObjSize.Height;

                new Projectile(new PointF(posX, posY), new SizeF(32, 32), ElementType.Fire, dir);
            }
        }
        public class WaterScroll : IElement
        {
            public ElementType Type => ElementType.Water;
            public float StaminaCost { get; } = 20f;

            public void PrimaryAttack(Player player)
            {
                if (!player.CanUseAbility(StaminaCost)) return;
                player.UseStamina(StaminaCost);
                SpawnRipple(player);
                Console.WriteLine($"Primary attack of type: {Type}");
            }
            public void SpawnRipple(Player player)
            {
                new Ripple(player.Position);
            }

        }
        public class EarthScroll : IElement
        {
            public ElementType Type => ElementType.Earth;
            public float StaminaCost { get; } = 25f;
            public void PrimaryAttack(Player player)
            {
                if (!player.CanUseAbility(StaminaCost)) return;
                player.UseStamina(StaminaCost);
                Console.WriteLine($"Primary attack of type: {Type}");
            }

        }
        public class AirScroll : IElement
        {
            public ElementType Type => ElementType.Air;
            public float StaminaCost { get; } = 5f;
            public void PrimaryAttack(Player player)
            {
                if (!player.CanUseAbility(StaminaCost)) return;
                player.UseStamina(StaminaCost);
                Console.WriteLine($"Primary attack of type: {Type}");
            }
            
        }
    }
}