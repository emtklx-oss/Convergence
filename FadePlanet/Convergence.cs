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

namespace FadePlanet
{
    public partial class Convergence : Form
    {
        // --- DECLARE VARIABLES ---
        private UI gameUI;
        private Player player;

        private List<Enemy> enemies = new List<Enemy>();

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
            gameUI = new UI();
            player = new Player(new Point(528, 248), new Size(224, 224));

            // Create items
            Item airToken = new Item(new Point(700, 300), new Size(Item.DrawSize, Item.DrawSize), ItemType.Token, ElementType.Air);
            Item potion = new Item(new Point(600, 250), new Size(Item.DrawSize, Item.DrawSize), ItemType.Potion);
            

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

                player.LoadImages();
                gameUI.LoadScrollSheets(basePath);
                gameUI.LoadInventoryIcons(basePath);

                foreach (Enemy e in enemies)
                    e.LoadImages();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Image load failed: " + ex.Message);
            }

            // --- START GAME LOOP ---
            gameLoop = new System.Windows.Forms.Timer();
            gameLoop.Interval = 16;
            gameLoop.Tick += GameLoop_Tick;
            gameLoop.Start();
        }

        private void SpawnEnemy(EnemyType type, Point pos)
        {
            Enemy e = new Enemy(pos, new Size((int)(32 * 3.0f), (int)(32 * 3.0f)), type);
            e.LoadImages();
            enemies.Add(e);
        }

        // --- INPUT HANDLING ---
        private void Convergence_KeyDown(object sender, KeyEventArgs e)
        {
            if (!player.ScrollSwitchLocked)
            {
                var previousElement = player.CurrentElement;
                player.HandleScrollSwitch(e.KeyCode);

                if (player.PendingElement != null && player.CurrentElement == previousElement)
                    gameUI.StartScrollSwitch(player.PendingElement.Type);
            }

            if (e.KeyCode == Keys.D1) gameUI.SetSelectedSlot(1, player.ScrollSwitchLocked);
            if (e.KeyCode == Keys.D2) gameUI.SetSelectedSlot(2, player.ScrollSwitchLocked);
            if (e.KeyCode == Keys.D3) gameUI.SetSelectedSlot(3, player.ScrollSwitchLocked);
            if (e.KeyCode == Keys.D4) gameUI.SetSelectedSlot(4, player.ScrollSwitchLocked);
            if (e.KeyCode == Keys.D5) gameUI.SetSelectedSlot(5, false);
            if (e.KeyCode == Keys.D6) gameUI.SetSelectedSlot(6, false);

            player.HandleKeyDown(e);
        }
        private void Convergence_KeyUp(object sender, KeyEventArgs e)
        {
            player.HandleKeyUp(e);
        }
        private void Convergence_Load(object sender, EventArgs e) { }
        private void Convergence_MouseClick(object sender, MouseEventArgs e)
        {
            player.HandleMouseClick(e, gameUI.SelectedSlot);
        }


        // --- GAME LOOP UPDATE ---
        private void GameLoop_Tick(object sender, EventArgs e)
        {
            // 1. Build WorldObject list for sword hit detection
            List<WorldObject> enemyObjects = new List<WorldObject>(enemies);

            // 2. Update player
            player.Update(enemyObjects);

            // 3. Scroll animation milestones
            var (closingDone, openingDone) = gameUI.UpdateScrollAnimation();

            if (closingDone) player.ConfirmScrollSwitch();
            if (openingDone)
            {
                player.UnlockScrollSwitch();
                gameUI.FlushPendingSlot();
            }

            // 4. Remove dead enemies then update living ones
            enemies.RemoveAll(en =>
            {
                if (GameManager.AllObjects.TryGetValue(ObjectType.Enemy, out var dict))
                    return !dict.ContainsKey(en.Id);
                return true;
            });

            foreach (Enemy en in enemies) { en.Update(player); }

            // 5. Update all projectiles
            UpdateObjectType(ObjectType.Projectile, (obj) =>
            {
                if (obj is Projectile proj)
                {
                    proj.Update();
                    // Remove projectiles that go off screen
                    if (proj.Position.X < -50 || proj.Position.X > ClientSize.Width + 50)
                    {
                        GameManager.DespawnObject(proj);
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
                    if (!player.IsPlayingPickup && !player.IsPlayingSlash)
                    {
                        float dx = (item.Position.X + Item.DrawSize / 2f) - (player.Position.X + 112f);
                        float dy = (item.Position.Y + Item.DrawSize / 2f) - (player.Position.Y + 112f);
                        float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                        if (distance <= Item.PickupRange)
                        {
                            player.PickUpItem(item);
                            GameManager.DespawnObject(item);

                            if (item.ItemType == ItemType.Token)
                            player.TriggerPickupAnimation();
                        }
                    }
                }
            });

            // 8. Sync UI
            gameUI.UpdateHealth(player.Health, player.MaxHealth);
            gameUI.UpdateStamina(player.Stamina, player.MaxStamina);
            gameUI.UpdatePotionCount(player.PotionCount);

            // 9. Redraw
            this.Invalidate();
        }

        // Helper method to update all objects of a specific type from GameManager
        private void UpdateObjectType(ObjectType type, Action<WorldObject> updateAction)
        {
            if (GameManager.AllObjects.TryGetValue(type, out var objectDict))
            {
                foreach (WorldObject obj in objectDict.Values.ToList())
                {
                    updateAction(obj);
                }
            }
        }


        // --- DRAW TO THE SCREEN ---
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw all items
            DrawObjectType(e.Graphics, ObjectType.Item);

            // Draw all enemies
            foreach (Enemy en in enemies)
                en.Draw(e.Graphics);

            // Draw all projectiles
            DrawObjectType(e.Graphics, ObjectType.Projectile);

            // Draw all ripples
            DrawObjectType(e.Graphics, ObjectType.None, (obj) => obj is Ripple);

            // Draw player and hitbox
            player.Draw(e.Graphics);
            player.DrawHitbox(e.Graphics);

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

        // Helper method to draw all objects of a specific type from GameManager
        private void DrawObjectType(Graphics g, ObjectType type, Func<WorldObject, bool> filter = null)
        {
            if (GameManager.AllObjects.TryGetValue(type, out var objectDict))
            {
                foreach (WorldObject obj in objectDict.Values)
                {
                    if (filter == null || filter(obj))
                    {
                        obj.Draw(g);
                    }
                }
            }
        }
    }
}