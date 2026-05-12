using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace FadePlanet
{
    internal class PlayerMovement
    {
        // --- 1. VARIABLES ---

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Speed { get; set; } = 4.0f;

        // Graphics
        private Image facingB;
        private Image facingF;
        private Image facingR;
        private Image facingL;
        private Image idle1;
        private Image idle2;
        private Image currentImage;

        // Animation tracking
        private int frameCounter = 0;
        private int idleAnimationSpeed = 30;
        private bool isIdle1 = true;

        // Input tracking
        private bool isMovingUp, isMovingDown, isMovingLeft, isMovingRight;

        // --- 2. CONSTRUCTOR & SETUP ---

        public PlayerMovement(float startX, float startY)
        {
            X = startX;
            Y = startY;
        }

        public void LoadImages()
        {
            try
            {
                string basePath = Application.StartupPath;

                facingB = Image.FromFile(System.IO.Path.Combine(basePath, @"Graphics\Player\Walking\MainCharacter.FacingB.png"));
                facingF = Image.FromFile(System.IO.Path.Combine(basePath, @"Graphics\Player\Walking\MainCharacter.FacingF.png"));
                facingR = Image.FromFile(System.IO.Path.Combine(basePath, @"Graphics\Player\Walking\MainCharacter.FacingR.png"));
                facingL = Image.FromFile(System.IO.Path.Combine(basePath, @"Graphics\Player\Walking\MainCharacter.FacingL.png"));
                idle1 = Image.FromFile(System.IO.Path.Combine(basePath, @"Graphics\Player\Walking\MainCharacter.Idle1.png"));
                idle2 = Image.FromFile(System.IO.Path.Combine(basePath, @"Graphics\Player\Walking\MainCharacter.Idle2.png"));

                currentImage = idle1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load player images! Error: " + ex.Message);
            }
        }

        // --- 3. MOVEMENT & LOGIC ---

        public void SetInput(bool w, bool a, bool s, bool d)
        {
            isMovingUp = w;
            isMovingLeft = a;
            isMovingDown = s;
            isMovingRight = d;
        }

        public void Update()
        {
            bool isMoving = false;

            if (isMovingUp) { Y -= Speed; isMoving = true; currentImage = facingB; }
            if (isMovingDown) { Y += Speed; isMoving = true; currentImage = facingF; }
            if (isMovingLeft) { X -= Speed; isMoving = true; currentImage = facingL; }
            if (isMovingRight) { X += Speed; isMoving = true; currentImage = facingR; }

            // Handle Idle Animation
            if (!isMoving)
            {
                frameCounter++;
                if (frameCounter >= idleAnimationSpeed)
                {
                    frameCounter = 0;
                    isIdle1 = !isIdle1;
                }
                currentImage = isIdle1 ? idle1 : idle2;
            }
            else
            {
                frameCounter = 0;
            }
        }

        // --- 4. DRAWING ---

        public void Draw(Graphics g)
        {
            if (currentImage != null)
            {
                g.DrawImage(currentImage, X, Y, 224, 224);
            }
        }
    }
}