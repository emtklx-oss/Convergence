using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FadePlanet
{
    internal class Token : Item
    {
        // Floating animation
        private float floatOffset = 0f;
        private float floatTimer = 0f;
        private const float FloatSpeed = 0.05f;
        private const float FloatAmplitude = 6f;

        // Pickup proximity range (in pixels)
        public const float PickupRange = 60f;

        // Drawn size (scaled down from 224x224)
        public const int DrawSize = 48;

        private Image tokenImage;

        public Token(Point pos, Size size) : base(pos, size)
        {
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
                string basePath = GetProjectRoot();
                tokenImage = Image.FromFile(
                    Path.Combine(basePath, @"Graphics\Items\Tokens\AirToken.png")
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load token image: " + ex.Message);
            }
        }

        public void Update()
        {
            floatTimer += FloatSpeed;
            floatOffset = (float)Math.Sin(floatTimer) * FloatAmplitude;
        }

        public void Draw(Graphics g)
        {
            if (tokenImage == null) return;

            g.DrawImage(
                tokenImage,
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