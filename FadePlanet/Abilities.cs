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
            void PrimaryAttack(Player player);
            void SecondaryAttack(Player player);
             
        }

        public class FireScroll : IElement
        {
            public ElementType Type => ElementType.Fire;

            public void PrimaryAttack(Player player)
            {

                Console.WriteLine($"Primary attack of type: {Type}");
            }
            public void SecondaryAttack(Player player)
            {
                ShootFireball(player);
                Console.WriteLine($"Secondary attack of type: {Type}");
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
            public void PrimaryAttack(Player player)
            {
                Console.WriteLine($"Primary attack of type: {Type}");
            }
            public void SecondaryAttack(Player player)
            {
                SpawnRipple(player);
                Console.WriteLine($"Secondary attack of type: {Type}");
            }
            public void SpawnRipple(Player player)
            {
                new Ripple(player.Position);
            }
        }
        public class EarthScroll : IElement
        {
            public ElementType Type => ElementType.Earth;
            public void PrimaryAttack(Player player)
            {
                Console.WriteLine($"Primary attack of type: {Type}");
            }

            public void SecondaryAttack(Player player)
            {
                Console.WriteLine($"Secondary attack of type: {Type}");
            }
           
        }
        public class AirScroll : IElement
        {
            public ElementType Type => ElementType.Air;
            public void PrimaryAttack(Player player)
            { // Logic to spawn a Fireball projectile moving toward target
                Console.WriteLine($"Primary attack of type: {Type}");
            }
            public void SecondaryAttack(Player player)
            { // Logic for an explosion around the player 
                Console.WriteLine($"Secondary attack of type: {Type}");
            }
        }
    }
}