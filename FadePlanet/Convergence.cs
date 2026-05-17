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
        private Token airToken;

        // UI Images
        private Image healthGraphic;
        private Image healthBar;
        private Image staminaGraphic;
        private Image staminaBar;
        private Image inventorySlots;

        private System.Windows.Forms.Timer gameLoop;

        private string GetProjectRoot()
        {
            string path = Application.StartupPath;
            return Path.GetFullPath(Path.Combine(path, @"..\..\"));
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
            airToken = new Token(new Point(700, 300), new Size(Token.DrawSize, Token.DrawSize));

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

        // --- INPUT HANDLING ---
        private void Convergence_KeyDown(object sender, KeyEventArgs e)
        {
            // Scroll switching — only attempt if not locked
            if (!player.ScrollSwitchLocked)
            {
                var previousElement = player.CurrentElement;
                player.HandleScrollSwitch(e.KeyCode);

                if (player.PendingElement != null && player.CurrentElement == previousElement)
                {
                    gameUI.StartScrollSwitch(player.PendingElement.Type);
                }
            }

            // Highlight box slot selection
            // Pass ScrollSwitchLocked so slots 1-4 are blocked during animation
            if (e.KeyCode == Keys.D1) gameUI.SetSelectedSlot(1, player.ScrollSwitchLocked);
            if (e.KeyCode == Keys.D2) gameUI.SetSelectedSlot(2, player.ScrollSwitchLocked);
            if (e.KeyCode == Keys.D3) gameUI.SetSelectedSlot(3, player.ScrollSwitchLocked);
            if (e.KeyCode == Keys.D4) gameUI.SetSelectedSlot(4, player.ScrollSwitchLocked);

            // Slots 5 and 6 are never scroll slots so never blocked
            if (e.KeyCode == Keys.D5) gameUI.SetSelectedSlot(5, false);
            if (e.KeyCode == Keys.D6) gameUI.SetSelectedSlot(6, false);

            player.HandleKeyDown(e);
        }

        private void Convergence_Load(object sender, EventArgs e) { }

        private void Convergence_KeyUp(object sender, KeyEventArgs e)
        {
            player.HandleKeyUp(e);
        }

        private void Convergence_MouseClick(object sender, MouseEventArgs e)
        {
            player.HandleMouseClick(e);
        }

        // --- GAME LOOP UPDATE ---
        private void GameLoop_Tick(object sender, EventArgs e)
        {
            // 1. Update player
            player.Update();

            // 2. Scroll animation milestones
            var (closingDone, openingDone) = gameUI.UpdateScrollAnimation();

            if (closingDone) player.ConfirmScrollSwitch();
            if (openingDone) player.UnlockScrollSwitch();

            // 3. Token update and pickup check
            if (airToken != null)
            {
                airToken.Update();

                if (!player.IsPlayingPickup && !player.IsPlayingSlash)
                {
                    float dx = (airToken.Position.X + Token.DrawSize / 2f) - (player.Position.X + 112f);
                    float dy = (airToken.Position.Y + Token.DrawSize / 2f) - (player.Position.Y + 112f);
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= Token.PickupRange)
                    {
                        GameManager.DespawnObject(airToken);
                        airToken = null;
                        player.TriggerPickupAnimation();
                    }
                }
            }

            // 4. Sync UI health and stamina
            gameUI.UpdateHealth(player.Health, player.MaxHealth);
            gameUI.UpdateStamina(player.Stamina, player.MaxStamina);

            // 5. Redraw
            this.Invalidate();
        }

        // --- DRAW TO THE SCREEN ---
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            airToken?.Draw(e.Graphics);
            player.Draw(e.Graphics);
            player.DrawHitbox(e.Graphics);

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