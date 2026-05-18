using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static FadePlanet.Abilities;

namespace FadePlanet
{
    public class Player : WorldObject
    {
        // =====================================================================
        // PLAYER KNOCKBACK SETTINGS
        // =====================================================================
        private const float PlayerKnockbackDistance = 60f;
        private const float PlayerKnockbackSpeed = 5f;

        // =====================================================================
        // SWORD ATTACK HITBOX SETTINGS
        // =====================================================================
        private const float SwordHitboxWidth = 140f;
        private const float SwordHitboxHeight = 100f;
        private const float SwordHitboxOffsetX = 160f;
        private const float SwordHitboxOffsetY = 80f;
        private const int SwordDamage = 20;
        // =====================================================================

        #region Player Stats
        public int Health { get; private set; } = 100;
        public int MaxHealth { get; private set; } = 100;

        public int Stamina { get; private set; } = 100;
        public int MaxStamina { get; private set; } = 100;
        #endregion

        #region Player Inventory
        public HashSet<ElementType> Tokens { get; set; } = new HashSet<ElementType>();
        public int PotionCount { get; private set; } = 0;
        public void PickUpItem(Item item)
        {
            switch (item.ItemType)
            {
                case ItemType.Token:
                    AddToken(item.TokenType);
                    break;
                case ItemType.Potion:
                    PotionCount++;
                    break;
            }
        }
        public void AddToken(ElementType element)
        {
            if (element == ElementType.None) return;

            bool wasAdded = Tokens.Add(element);

            if (wasAdded)
            {
                Console.WriteLine($"New token acquired: {element}!");
            }
        }

        public bool HasToken(ElementType element)
        {
            return Tokens.Contains(element);
        }
        private void TryConsumePotion()
        {
            if (PotionCount > 0 && Health < MaxHealth)
            {
                PotionCount--;
                Health += 10; //Replace with how much a health potion should recover
            }
        }
        #endregion

        #region Player Abilities
        public IElement CurrentElement { get; private set; }
        public IElement PendingElement { get; private set; }

        private Dictionary<Keys, IElement> _abilities = new Dictionary<Keys, IElement>
        {
            { Keys.D1, new WaterScroll() },
            { Keys.D2, new FireScroll() },
            { Keys.D3, new EarthScroll() },
            { Keys.D4, new AirScroll() }
        };

        public bool ScrollSwitchLocked { get; set; } = false;
        #endregion

        #region Hitbox
        // =====================================================================
        // HITBOX SETTINGS
        // =====================================================================
        private const float HitboxWidth = 80f;
        private const float HitboxHeight = 100f;
        private const float HitboxOffsetX = 72f;
        private const float HitboxOffsetY = 110f;

        // Override the base WorldObject Hitbox with a more precise player hitbox
        public override RectangleF Hitbox => new RectangleF(
            Position.X + HitboxOffsetX,
            Position.Y + HitboxOffsetY,
            HitboxWidth,
            HitboxHeight
        );

        // Sword attack hitbox — wider rectangle in front of the player
        public RectangleF SwordHitbox
        {
            get
            {
                float offsetX = isFacingLeft
                    ? -(SwordHitboxOffsetX + SwordHitboxWidth)
                    : SwordHitboxOffsetX;

                return new RectangleF(
                    Position.X + offsetX,
                    Position.Y + SwordHitboxOffsetY,
                    SwordHitboxWidth,
                    SwordHitboxHeight
                );
            }
        }

        // Override the base ShowHitbox toggle
        public override bool ShowHitbox { get; set; } = false;
        #endregion

        #region Knockback
        private bool isKnockedBack = false;
        private PointF knockbackDirection;
        private float knockbackRemaining = 0f;
        #endregion

        #region Secondary Attack Cooldown
        private const int SecondaryAttackCooldownMs = 1000; // 1 second cooldown
        private int secondaryAttackCooldownRemaining = 0;

        public bool CanUseSecondaryAttack => secondaryAttackCooldownRemaining <= 0;
        #endregion

        public Player(Point pos, Size size) : base(pos, size, ObjectType.Player)
        {
            CurrentElement = _abilities[Keys.D4];
        }

        #region Movement & Animation
        public int Speed { get; set; } = 4;

        private Image facingB;
        private Image facingF;
        private Image facingR;
        private Image facingL;
        private Image idleR1;
        private Image idleR2;
        private Image idleL1;
        private Image idleL2;
        private Image currentImage;

        private Image pickupFrame1;
        private Image pickupFrame2;
        private Image pickupFrame3;

        private Bitmap slashSheetR;
        private Bitmap slashSheetL;

        public bool IsPlayingPickup { get; private set; } = false;
        private int pickupFrameIndex = 0;
        private int pickupFrameTimer = 0;
        private const int PickupFrameDuration = 12;

        public bool IsPlayingSlash { get; private set; } = false;
        private int slashFrameIndex = 0;
        private int slashFrameTimer = 0;
        private const int SlashFrameDuration = 2;
        private const int SlashTotalFrames = 6;

        public bool swordHitDealtThisSwing = false; //set in Abilities when player left-clicks, not when they use secondary
        public bool isFacingLeft { get; private set; } = false;

        private static readonly Point[] SlashFramePositions = new Point[]
        {
            new Point(0,   0),
            new Point(224, 0),
            new Point(0,   224),
            new Point(224, 224),
            new Point(0,   448),
            new Point(224, 448)
        };

        private int frameCounter = 0;
        private int idleAnimationSpeed = 30;
        private bool isIdle1 = true;

        private bool isMovingUp, isMovingDown, isMovingLeft, isMovingRight;

        private string GetProjectRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\"));
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

                idleR1 = Image.FromFile(Path.Combine(basePath, @"Graphics\Player\Walking\MainCharacter.Idle1.png"));
                idleR2 = Image.FromFile(Path.Combine(basePath, @"Graphics\Player\Walking\MainCharacter.Idle2.png"));

                idleL1 = (Image)idleR1.Clone();
                idleL1.RotateFlip(RotateFlipType.RotateNoneFlipX);
                idleL2 = (Image)idleR2.Clone();
                idleL2.RotateFlip(RotateFlipType.RotateNoneFlipX);

                pickupFrame1 = Image.FromFile(Path.Combine(basePath, @"Graphics\Player\Tokens\ClaimingTokens1.png"));
                pickupFrame2 = Image.FromFile(Path.Combine(basePath, @"Graphics\Player\Tokens\ClaimingTokens2.png"));
                pickupFrame3 = Image.FromFile(Path.Combine(basePath, @"Graphics\Player\Tokens\ClaimingTokens3.png"));

                slashSheetR = new Bitmap(Path.Combine(basePath, @"Graphics\Player\Sword Animation\Slashing.png"));
                slashSheetL = (Bitmap)slashSheetR.Clone();
                slashSheetL.RotateFlip(RotateFlipType.RotateNoneFlipX);

                currentImage = idleR1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load player images! Error: " + ex.Message);
            }
        }

        public void TriggerPickupAnimation()
        {
            IsPlayingPickup = true;
            pickupFrameIndex = 0;
            pickupFrameTimer = 0;
        }

        public void TriggerSlashAnimation()
        {
            if (IsPlayingSlash) return;

            IsPlayingSlash = true;
            slashFrameIndex = 0;
            slashFrameTimer = 0;
        }

        // Takes a plain list of WorldObjects so accessibility matches public
        public void Update(List<WorldObject> enemyObjects)
        {
            // Decrement secondary attack cooldown
            if (secondaryAttackCooldownRemaining > 0)
            {
                secondaryAttackCooldownRemaining -= 16; // ~16ms per frame at 60fps
            }

            // Handle knockback first — overrides everything
            if (isKnockedBack)
            {
                if (knockbackRemaining > 0)
                {
                    float step = Math.Min(PlayerKnockbackSpeed, knockbackRemaining);
                    Position = new PointF(
                        Position.X + knockbackDirection.X * step,
                        Position.Y + knockbackDirection.Y * step
                    );
                    knockbackRemaining -= step;
                }
                else
                {
                    isKnockedBack = false;
                }

                currentImage = isFacingLeft ? idleL1 : idleR1;
                return;
            }

            // Pickup animation
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
                        currentImage = isFacingLeft ? idleL1 : idleR1;
                    }
                }

                if (pickupFrameIndex == 0) currentImage = pickupFrame1;
                else if (pickupFrameIndex == 1) currentImage = pickupFrame2;
                else if (pickupFrameIndex == 2) currentImage = pickupFrame3;

                return;
            }

            // Slash animation
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
                        currentImage = isFacingLeft ? idleL1 : idleR1;
                    }
                }

                // Check sword hitbox against enemies once per swing
                if (!swordHitDealtThisSwing && enemyObjects != null)
                {
                    foreach (WorldObject obj in enemyObjects)
                    {
                        Enemy enemy = obj as Enemy;
                        if (enemy == null) continue;

                        if (SwordHitbox.IntersectsWith(enemy.Bounds))
                        {
                            enemy.TakeDamage(SwordDamage, new PointF(
                                Position.X + 112f,
                                Position.Y + 112f
                            ));
                        }
                    }
                    swordHitDealtThisSwing = true;
                }

                return;
            }

            // Normal movement
            bool isMoving = false;
            float x = Position.X;
            float y = Position.Y;

            if (isMovingUp) { y -= Speed; isMoving = true; currentImage = facingB; }
            if (isMovingDown) { y += Speed; isMoving = true; currentImage = facingF; }
            if (isMovingLeft) { x -= Speed; isMoving = true; isFacingLeft = true; currentImage = facingL; }
            if (isMovingRight) { x += Speed; isMoving = true; isFacingLeft = false; currentImage = facingR; }

            Position = new PointF(x, y);

            if (!isMoving)
            {
                frameCounter++;
                if (frameCounter >= idleAnimationSpeed)
                {
                    frameCounter = 0;
                    isIdle1 = !isIdle1;
                }
                currentImage = isFacingLeft
                    ? (isIdle1 ? idleL1 : idleL2)
                    : (isIdle1 ? idleR1 : idleR2);
            }
            else
            {
                frameCounter = 0;
            }
        }

        public void ApplyKnockback(PointF sourcePosition)
        {
            float kDx = Position.X - sourcePosition.X;
            float kDy = Position.Y - sourcePosition.Y;
            float kLen = (float)Math.Sqrt(kDx * kDx + kDy * kDy);

            knockbackDirection = kLen > 0
                ? new PointF(kDx / kLen, kDy / kLen)
                : new PointF(1f, 0f);

            knockbackRemaining = PlayerKnockbackDistance;
            isKnockedBack = true;
        }

        public PointF GetAttackDirection()
        {
            // Check movement direction first (WASD keys)
            if (isMovingUp) return new PointF(0, -1);
            if (isMovingDown) return new PointF(0, 1);
            if (isMovingLeft) return new PointF(-1, 0);
            if (isMovingRight) return new PointF(1, 0);

            // Fall back to facing direction
            return isFacingLeft ? new PointF(-1, 0) : new PointF(1, 0);
        }

        public override void Draw(Graphics g)
        {
            if (IsPlayingSlash)
            {
                Bitmap activeSheet = isFacingLeft ? slashSheetL : slashSheetR;

                if (activeSheet != null)
                {
                    Point framePos = SlashFramePositions[slashFrameIndex];

                    Rectangle srcRect;
                    if (isFacingLeft)
                    {
                        int mirroredX = framePos.X == 0 ? 224 : 0;
                        srcRect = new Rectangle(mirroredX, framePos.Y, 224, 224);
                    }
                    else
                    {
                        srcRect = new Rectangle(framePos.X, framePos.Y, 224, 224);
                    }

                    RectangleF destRect = new RectangleF(Position.X, Position.Y, 224, 224);
                    g.DrawImage(activeSheet, destRect, srcRect, GraphicsUnit.Pixel);
                }
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
                g.DrawRectangle(hitboxPen, Hitbox.X, Hitbox.Y, Hitbox.Width, Hitbox.Height);

            if (IsPlayingSlash)
            {
                using (Pen swordPen = new Pen(Color.Blue, 2f))
                    g.DrawRectangle(swordPen, SwordHitbox.X, SwordHitbox.Y, SwordHitbox.Width, SwordHitbox.Height);
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

            if (e.KeyCode == Keys.H) ShowHitbox = !ShowHitbox;
            if (e.KeyCode == Keys.E) TryConsumePotion();
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
                swordHitDealtThisSwing = false;
                TriggerSlashAnimation();
                
                CurrentElement?.PrimaryAttack(this);
            }
            if (e.Button == MouseButtons.Right)
            { 
                if (CanUseSecondaryAttack)
                {
                    TriggerSlashAnimation();
                    CurrentElement?.SecondaryAttack(this);
                    secondaryAttackCooldownRemaining = SecondaryAttackCooldownMs;
                }
            }
        }

        public void HandleScrollSwitch(Keys key)
        {
            if (ScrollSwitchLocked) return;

            if (_abilities.ContainsKey(key))
            {
                if (CurrentElement == _abilities[key])
                    Console.WriteLine("Already active");
                else
                {
                    PendingElement = _abilities[key];
                    ScrollSwitchLocked = true;
                    Console.WriteLine($"Switching to {PendingElement.Type}...");
                }
            }
        }

        public void ConfirmScrollSwitch()
        {
            CurrentElement = PendingElement;
            PendingElement = null;
            Console.WriteLine($"Switched to {CurrentElement.Type}!");
        }

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

        public new void OnDeath() { }
        #endregion

        #region Testing
        public void TestTakeDamage(int amount)
        {
            TakeDamage(amount);
        }
        #endregion
    }
}