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
            void 
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
                Console.WriteLine($"Secondary attack of type: {Type}");
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
                Console.WriteLine($"Secondary attack of type: {Type}");
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