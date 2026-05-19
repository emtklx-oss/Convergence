using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FadePlanet.GameManager;

namespace FadePlanet
{
    public partial class Convergence : Form
    {
        // --- DECLARE UI ---
        private UI gameUI;

        // UI Images
        private Image healthGraphic;
        private Image healthBar;
        private Image staminaGraphic;
        private Image staminaBar;
        private Image inventorySlots;

        private System.Windows.Forms.Timer gameLoop;

        private string GetProjectRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\"));
        }

        public Convergence()
        {
            InitializeComponent();

            this.ClientSize = new Size(1280, 720);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.KeyDown += Convergence_KeyDown;
            this.KeyUp += Convergence_KeyUp;
            this.MouseClick += Convergence_MouseClick;

            // --- INITIALIZE CLASSES ---
            // REMEMBER: Objects are automatatically added to the dictionary of current objects in room in GameManager

            SetPlayer(new Player(new Point(528, 248), new Size(224, 224)));

            new OldMan(new Point(100, 100), new Size(224, 224));

            // Create items
            new Item(new Point(700, 300), new Size(Item.DrawSize, Item.DrawSize), ItemType.Token, ElementType.Air);
            new Item(new Point(600, 250), new Size(Item.DrawSize, Item.DrawSize), ItemType.Potion);


            // --- SPAWN TEST ENEMIES ---
            SpawnEnemy(EnemyType.Air, new Point(200, 200));
            SpawnEnemy(EnemyType.Water, new Point(900, 400));
            SpawnEnemy(EnemyType.Earth, new Point(300, 500));
            SpawnEnemy(EnemyType.Fire, new Point(1000, 200));

            // --- LOAD IMAGES ---
            string basePath = GetProjectRoot();
            

            try
            {
                healthGraphic = Image.FromFile(Path.Combine(basePath, @"Graphics\UI\HealthGraphic.png"));
                healthBar = Image.FromFile(Path.Combine(basePath, @"Graphics\UI\HealthBar.png"));
                staminaGraphic = Image.FromFile(Path.Combine(basePath, @"Graphics\UI\StaminaGraphic.png"));
                staminaBar = Image.FromFile(Path.Combine(basePath, @"Graphics\UI\StaminaBar.png"));
                inventorySlots = Image.FromFile(Path.Combine(basePath, @"Graphics\UI\Inventory Slots.png"));

                foreach (Enemy e in GetObjectsByType(ObjectType.Enemy))
                    e.LoadImages();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Image load failed: " + ex.Message);
            }
            
            gameUI = new UI(basePath);

            // --- START GAME LOOP ---
            gameLoop = new System.Windows.Forms.Timer();
            gameLoop.Interval = 16;
            gameLoop.Tick += GameLoop_Tick;
            gameLoop.Start();
        }

        private void SpawnEnemy(EnemyType type, Point pos)
        {
            new Enemy(pos, new Size((int)(32 * 3.0f), (int)(32 * 3.0f)), type);
        }

        // --- INPUT HANDLING ---
        private void Convergence_KeyDown(object sender, KeyEventArgs e)
        {
            if (!CurPlayer.ScrollSwitchLocked)
            {
                var previousElement = CurPlayer.CurrentElement;
                CurPlayer.HandleScrollSwitch(e.KeyCode);

                if (CurPlayer.PendingElement != null && CurPlayer.CurrentElement == previousElement)
                    gameUI.StartScrollSwitch(CurPlayer.PendingElement.Type);
            }

            if (e.KeyCode == Keys.D1) gameUI.SetSelectedSlot(1, CurPlayer.ScrollSwitchLocked);
            if (e.KeyCode == Keys.D2) gameUI.SetSelectedSlot(2, CurPlayer.ScrollSwitchLocked);
            if (e.KeyCode == Keys.D3) gameUI.SetSelectedSlot(3, CurPlayer.ScrollSwitchLocked);
            if (e.KeyCode == Keys.D4) gameUI.SetSelectedSlot(4, CurPlayer.ScrollSwitchLocked);
            if (e.KeyCode == Keys.D5) gameUI.SetSelectedSlot(5, false);
            if (e.KeyCode == Keys.D6) gameUI.SetSelectedSlot(6, false);

            CurPlayer.HandleKeyDown(e);
        }
        private void Convergence_KeyUp(object sender, KeyEventArgs e)
        {
            CurPlayer.HandleKeyUp(e);
        }
        private void Convergence_Load(object sender, EventArgs e) { }
        private void Convergence_MouseClick(object sender, MouseEventArgs e)
        {
            CurPlayer.HandleMouseClick(e, gameUI.SelectedSlot);
        }


        // --- GAME LOOP UPDATE ---
        private void GameLoop_Tick(object sender, EventArgs e)
        {


            // 2. Update player
            UpdateObjectType(ObjectType.Player, (obj) =>
            {
                if (obj is Player p)
                {
                    p.Update(GetObjectsByType(ObjectType.Enemy));
                }
            });

            // 3. Scroll animation milestones
            var (closingDone, openingDone) = gameUI.UpdateScrollAnimation();

            if (closingDone) CurPlayer.ConfirmScrollSwitch();
            if (openingDone)
            {
                CurPlayer.UnlockScrollSwitch();
                gameUI.FlushPendingSlot();
            }

            // 4. Remove dead enemies then update living ones
            GetObjectsByType(ObjectType.Enemy).RemoveAll(en =>
            {
                if (GameManager.AllObjects.TryGetValue(ObjectType.Enemy, out var dict))
                    return !dict.ContainsKey(en.Id);
                return true;
            });

            foreach (Enemy en in GetObjectsByType(ObjectType.Enemy)) { en.Update(CurPlayer); }

            // 5. Update all projectiles
            UpdateObjectType(ObjectType.Projectile, (obj) =>
            {
                if (obj is Projectile proj)
                {
                    proj.Update();
                    // Remove projectiles that go off screen
                    if (proj.Position.X < -50 || proj.Position.X > ClientSize.Width + 50)
                    {
                        DespawnObject(proj);
                    }
                }
            });

            // 6. Update all ripples
            UpdateObjectType(ObjectType.None, (obj) =>
            {
                if (obj is Ripple r)
                {
                    r.Update();
                }
            });

            // 7. Update all items (tokens, potions, etc.)
            UpdateObjectType(ObjectType.Item, (obj) =>
            {
                if (obj is Item item)
                {
                    item.Update();

                    // Check for pickup
                    if (!CurPlayer.IsPlayingPickup && !CurPlayer.IsPlayingSlash)
                    {
                        float dx = (item.Position.X + Item.DrawSize / 2f) - (CurPlayer.Position.X + 112f);
                        float dy = (item.Position.Y + Item.DrawSize / 2f) - (CurPlayer.Position.Y + 112f);
                        float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                        if (distance <= Item.PickupRange)
                        {
                            CurPlayer.PickUpItem(item);
                            GameManager.DespawnObject(item);

                            if (item.ItemType == ItemType.Token)
                                CurPlayer.TriggerPickupAnimation();
                        }
                    }
                }
            });
            UpdateObjectType(ObjectType.Friendly, (obj) =>
            {
                if (obj is OldMan man)
                {
                    //Check interaction if player isn't attacking or picking something up
                    if (!CurPlayer.IsPlayingPickup && !CurPlayer.IsPlayingSlash)
                    {
                        float dx = (man.Position.X + man.ObjSize.Width / 2f) - (CurPlayer.Position.X + 112f);
                        float dy = (man.Position.Y + man.ObjSize.Height / 2f) - (CurPlayer.Position.Y + 112f);
                        float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                        if (distance <= OldMan.InteractDistance) { man.OnInteract(CurPlayer); }
                    }
                }
            });

            // 8. Sync UI
            gameUI.UpdateHealth(CurPlayer.Health, CurPlayer.MaxHealth);
            gameUI.UpdateStamina(CurPlayer.Stamina, CurPlayer.MaxStamina);
            gameUI.UpdatePotionCount(CurPlayer.PotionCount);

            // 9. Redraw
            this.Invalidate();
        }


        // --- DRAW TO THE SCREEN ---
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw all items
            DrawObjectType(e.Graphics, ObjectType.Item);

            // Draw all enemies
            foreach (Enemy en in GetObjectsByType(ObjectType.Enemy))
                en.Draw(e.Graphics);

            // Draw all projectiles
            DrawObjectType(e.Graphics, ObjectType.Projectile);

            // Draw all ripples
            DrawObjectType(e.Graphics, ObjectType.None, (obj) => obj is Ripple);


            // Draw player and hitbox
            CurPlayer?.Draw(e.Graphics);
            CurPlayer?.DrawHitbox(e.Graphics);




            // Draw UI
            if (healthGraphic != null && healthBar != null && staminaGraphic != null && staminaBar != null && inventorySlots != null)
            {
                gameUI.DrawWinFormsUI(
                    e.Graphics,
                    healthGraphic,
                    healthBar,
                    staminaGraphic,
                    staminaBar,
                    inventorySlots,
                    this.ClientSize.Width,
                    this.ClientSize.Height
                );
            }

        }
    }
}