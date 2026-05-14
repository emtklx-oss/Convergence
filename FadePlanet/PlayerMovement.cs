using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        // Pickup animation frames
        private Image pickupFrame1;
        private Image pickupFrame2;
        private Image pickupFrame3;

        // Slash spritesheet
        private Bitmap slashSheet;

        // Pickup animation state
        public bool IsPlayingPickup { get; private set; } = false;
        private int pickupFrameIndex = 0;
        private int pickupFrameTimer = 0;
        private const int PickupFrameDuration = 12;

        // Slash animation state
        public bool IsPlayingSlash { get; private set; } = false;
        private int slashFrameIndex = 0;
        private int slashFrameTimer = 0;
        private const int SlashFrameDuration = 2; // Ticks per frame — lower = faster
        private const int SlashTotalFrames = 6;

        // Slash frame positions on the spritesheet (2 columns, 3 rows, each frame 224x224)
        private static readonly Point[] SlashFramePositions = new Point[]
        {
            new Point(0,   0),   // Frame 1
            new Point(224, 0),   // Frame 2
            new Point(0,   224), // Frame 3
            new Point(224, 224), // Frame 4
            new Point(0,   448), // Frame 5
            new Point(224, 448)  // Frame 6
        };

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

        // Walks up from bin\Debug to the project root where Graphics folder lives
        private string GetProjectRoot()
        {
            string path = Application.StartupPath;
            return Path.GetFullPath(Path.Combine(path, @"..\..\"));
        }

        public void LoadImages()
        {
            try
            {
                string basePath = GetProjectRoot();

                facingB = Image.FromFile(Path.Combine(basePath, @"Graphics\Player\Walking\MainCharacter.FacingB.png"));
                facingF = Image.FromFile(Path.Combine(basePath, @"Graphics\Player\Walking\MainCharacter.FacingF.png"));
                facingR = Image.FromFile(Path.Combine(basePath, @"Graphics\Player\Walking\MainCharacter.FacingR.png"));
                facingL = Image.FromFile(Path.Combine(basePath, @"Graphics\Player\Walking\MainCharacter.FacingL.png"));
                idle1 = Image.FromFile(Path.Combine(basePath, @"Graphics\Player\Walking\MainCharacter.Idle1.png"));
                idle2 = Image.FromFile(Path.Combine(basePath, @"Graphics\Player\Walking\MainCharacter.Idle2.png"));

                pickupFrame1 = Image.FromFile(Path.Combine(basePath, @"Graphics\Player\Tokens\ClaimingTokens1.png"));
                pickupFrame2 = Image.FromFile(Path.Combine(basePath, @"Graphics\Player\Tokens\ClaimingTokens2.png"));
                pickupFrame3 = Image.FromFile(Path.Combine(basePath, @"Graphics\Player\Tokens\ClaimingTokens3.png"));

                slashSheet = new Bitmap(Path.Combine(basePath, @"Graphics\Player\Sword Animation\Slashing.png"));

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

        public void TriggerPickupAnimation()
        {
            IsPlayingPickup = true;
            pickupFrameIndex = 0;
            pickupFrameTimer = 0;
        }

        public void TriggerSlashAnimation()
        {
            // Don't interrupt a slash already playing
            if (IsPlayingSlash) return;

            IsPlayingSlash = true;
            slashFrameIndex = 0;
            slashFrameTimer = 0;
        }

        public void Update()
        {
            // Pickup animation takes highest priority — freezes everything
            if (IsPlayingPickup)
            {
                pickupFrameTimer++;

                if (pickupFrameTimer >= PickupFrameDuration)
                {
                    pickupFrameTimer = 0;
                    pickupFrameIndex++;

                    if (pickupFrameIndex >= 3)
                    {
                        IsPlayingPickup = false;
                        pickupFrameIndex = 0;
                        currentImage = idle1;
                    }
                }

                if (pickupFrameIndex == 0) currentImage = pickupFrame1;
                else if (pickupFrameIndex == 1) currentImage = pickupFrame2;
                else if (pickupFrameIndex == 2) currentImage = pickupFrame3;

                return;
            }

            // Slash animation — freezes movement but is handled in Draw()
            if (IsPlayingSlash)
            {
                slashFrameTimer++;

                if (slashFrameTimer >= SlashFrameDuration)
                {
                    slashFrameTimer = 0;
                    slashFrameIndex++;

                    if (slashFrameIndex >= SlashTotalFrames)
                    {
                        IsPlayingSlash = false;
                        slashFrameIndex = 0;
                        currentImage = idle1;
                    }
                }

                return; // Skip movement
            }

            // Normal movement
            bool isMoving = false;

            if (isMovingUp) { Y -= Speed; isMoving = true; currentImage = facingB; }
            if (isMovingDown) { Y += Speed; isMoving = true; currentImage = facingF; }
            if (isMovingLeft) { X -= Speed; isMoving = true; currentImage = facingL; }
            if (isMovingRight) { X += Speed; isMoving = true; currentImage = facingR; }

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
            if (IsPlayingSlash && slashSheet != null)
            {
                // Grab the correct frame from the spritesheet
                Point framePos = SlashFramePositions[slashFrameIndex];

                Rectangle srcRect = new Rectangle(framePos.X, framePos.Y, 224, 224);
                RectangleF destRect = new RectangleF(X, Y, 224, 224);

                g.DrawImage(slashSheet, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else if (currentImage != null)
            {
                g.DrawImage(currentImage, X, Y, 224, 224);
            }
        }
    }
}