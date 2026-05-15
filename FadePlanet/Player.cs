using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FadePlanet.Abilities;

namespace FadePlanet
{
    public class Player : WorldObject
    {
        #region Player Stats
        public int Health { get; private set; } = 100;
        public int MaxHealth { get; private set; } = 100;

        public int Stamina { get; private set; } = 100;
        public int MaxStamina { get; private set; } = 100;
        #endregion

        #region Player Inventory
        private int TokenCount { get; set; } = 0;
        private int PotionCount { get; set; } = 0;
        #endregion

        #region Player Abilities
        public IElement CurrentElement { get; private set; }
        public IElement PendingElement { get; private set; } // The scroll we are switching TO

        private Dictionary<Keys, IElement> _abilities = new Dictionary<Keys, IElement>
        {
            { Keys.D1, new WaterScroll() },
            { Keys.D2, new FireScroll() },
            { Keys.D3, new EarthScroll() },
            { Keys.D4, new AirScroll() }
        };

        // Cooldown flag — locked while scroll animation is playing
        public bool ScrollSwitchLocked { get; set; } = false;
        #endregion

        #region Hitbox
        // =====================================================================
        // HITBOX SETTINGS — Tweak these values to resize/reposition the hitbox
        // HitboxWidth   — how wide the hitbox is in pixels
        // HitboxHeight  — how tall the hitbox is in pixels
        // HitboxOffsetX — how far right from the player's Position the hitbox starts
        // HitboxOffsetY — how far down from the player's Position the hitbox starts
        // =====================================================================
        private const float HitboxWidth = 80f;
        private const float HitboxHeight = 100f;
        private const float HitboxOffsetX = 72f;
        private const float HitboxOffsetY = 110f;

        public RectangleF Hitbox => new RectangleF(
            Position.X + HitboxOffsetX,
            Position.Y + HitboxOffsetY,
            HitboxWidth,
            HitboxHeight
        );

        public bool ShowHitbox { get; set; } = false;
        #endregion

        public Player(Point pos, Size size) : base(pos, size, ObjectType.Player)
        {
            // Set default scroll to AirScroll on game start
            CurrentElement = _abilities[Keys.D4];
        }
        #region Movement & Animation
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
            float x = Position.X;
            float y = Position.Y;

            if (isMovingUp) { y -= Speed; isMoving = true; currentImage = facingB; }
            if (isMovingDown) { y += Speed; isMoving = true; currentImage = facingF; }
            if (isMovingLeft) { x -= Speed; isMoving = true; currentImage = facingL; }
            if (isMovingRight) { x += Speed; isMoving = true; currentImage = facingR; }

            Position = new PointF(x, y);

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

        
        public override void Draw(Graphics g)
        {
            if (IsPlayingSlash && slashSheet != null)
            {
                // Grab the correct frame from the spritesheet
                Point framePos = SlashFramePositions[slashFrameIndex];

                Rectangle srcRect = new Rectangle(framePos.X, framePos.Y, 224, 224);
                RectangleF destRect = new RectangleF(Position.X, Position.Y, 224, 224);

                g.DrawImage(slashSheet, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else if (currentImage != null)
            {
                g.DrawImage(currentImage, Position.X, Position.Y, 224, 224);
            }
        }
        #endregion
        #region Hitbox Drawing
        public void DrawHitbox(Graphics g)
        {
            if (!ShowHitbox) return;

            using (Pen hitboxPen = new Pen(Color.Red, 2f))
            {
                g.DrawRectangle(hitboxPen, Hitbox.X, Hitbox.Y, Hitbox.Width, Hitbox.Height);
            }
        }
        #endregion

        #region Input
        public void HandleKeyDown(KeyEventArgs e) 
        {
            if (e.KeyCode == Keys.W) isMovingUp = true;
            if (e.KeyCode == Keys.S) isMovingDown = true;
            if (e.KeyCode == Keys.A) isMovingLeft = true;
            if (e.KeyCode == Keys.D) isMovingRight = true;

            // Toggle hitbox visibility
            if (e.KeyCode == Keys.H) ShowHitbox = !ShowHitbox;

            // Test damage
            if (e.KeyCode == Keys.J) TakeDamage(10);
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W) isMovingUp = false;
            if (e.KeyCode == Keys.S) isMovingDown = false;
            if (e.KeyCode == Keys.A) isMovingLeft = false;
            if (e.KeyCode == Keys.D) isMovingRight = false;
        }

        public void HandleMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TriggerSlashAnimation();
                CurrentElement?.PrimaryAttack(this);
            }
            if (e.Button == MouseButtons.Right)
            {
                CurrentElement?.SecondaryAttack(this);
            }
        }

        public void HandleScrollSwitch(Keys key)
        {
            // Block switching if cooldown is active
            if (ScrollSwitchLocked) return;

            if (_abilities.ContainsKey(key))
            {
                if (CurrentElement == _abilities[key])
                {
                    Console.WriteLine($"Already active");
                }
                else
                {
                    // Store the scroll we want to switch to
                    PendingElement = _abilities[key];
                    ScrollSwitchLocked = true;
                    Console.WriteLine($"Switching to {PendingElement.Type.ToString()}...");
                }
            }
        }

        // Called by UI once the closing animation finishes
        public void ConfirmScrollSwitch()
        {
            CurrentElement = PendingElement;
            PendingElement = null;
            Console.WriteLine($"Switched to {CurrentElement.Type.ToString()}!");
        }

        // Called by UI once the opening animation finishes
        public void UnlockScrollSwitch()
        {
            ScrollSwitchLocked = false;
        }
        #endregion

        #region Damage/Death Functions
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
            // Handle player death here later
        }
        #endregion

    }
}