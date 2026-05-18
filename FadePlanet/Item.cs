using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FadePlanet
{
    public enum ItemType
    {
        None,
        Potion,
        Token
    }
    

    public class Item : WorldObject
    {
        public ItemType ItemType { get; private set; }
        public ElementType TokenType { get; private set; } // only relevant if Type == Token
        private const int MaxStackSize = 99;
        private int CurrentStackSize { get; set; } = 0;
        public Image ItemImage { get; set; }

        // Floating animation
        private float floatOffset = 0f;
        private float floatTimer = 0f;
        private const float FloatSpeed = 0.05f;
        private const float FloatAmplitude = 6f;

        // Pickup proximity range (in pixels)
        public const float PickupRange = 60f;

        // Drawn size (scaled down from 224x224)
        public const int DrawSize = 67;

        public Item(Point pos, Size size, ItemType itemT, ElementType tokenT = ElementType.None) : base(pos, size, ObjectType.Item) 
        {
            ItemType = itemT;
            TokenType = tokenT;
            LoadImage();
        }

        // Walks up from bin\Debug to the project root where Graphics folder lives
        private string GetProjectRoot()
        {
            string path = Application.StartupPath;
            return Path.GetFullPath(Path.Combine(path, @"..\..\"));
        }

        private void LoadImage()
        {

            try
            {
                switch (ItemType)
                {
                    case ItemType.Potion:
                        ItemImage = Image.FromFile(
                            Path.Combine(GetProjectRoot(), @"Graphics\Items\HealthPotion.png")
                        );
                        break;
                    case ItemType.Token:
                        
                        string basePath = GetProjectRoot();
                        switch (TokenType)
                        {
                            case ElementType.Fire:
                                ItemImage = Image.FromFile(Path.Combine(basePath, @"Graphics\Items\Tokens\FireToken.png"));
                                break;
                            case ElementType.Water:
                                ItemImage = Image.FromFile(Path.Combine(basePath, @"Graphics\Items\Tokens\WaterToken.png"));
                                break;
                            case ElementType.Earth:
                                ItemImage = Image.FromFile(Path.Combine(GetProjectRoot(), @"Graphics\Items\Tokens\EarthToken.png"));
                                break;
                            case ElementType.Air:
                                ItemImage = Image.FromFile(Path.Combine(basePath, @"Graphics\Items\Tokens\AirToken.png"));
                                break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load item image: " + ex.Message);
            }
        }

        public void Update()
        {
            floatTimer += FloatSpeed;
            floatOffset = (float)Math.Sin(floatTimer) * FloatAmplitude;
        }

        public override void Draw(Graphics g)
        {
            if (ItemImage == null) return;

            g.DrawImage(
                ItemImage,
                new RectangleF(
                    Position.X,
                    Position.Y + floatOffset,
                    DrawSize,
                    DrawSize
                )
            );
        }
    }
}
