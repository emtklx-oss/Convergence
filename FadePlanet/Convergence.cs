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
        // --- DECLARE VARIABLES ---
        private UI gameUI;
        
        private Token airToken;

        // UI Images
        private Image healthGraphic;
        private Image healthBar;
        private Image staminaGraphic;
        private Image staminaBar;
        private Image inventorySlots;

        private System.Windows.Forms.Timer gameLoop;


        // Walks up from bin\Debug to the project root where Graphics folder lives
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
            LoadRoom();
            gameUI = new UI();
            
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

                GameManager.Player.LoadImages();

                // Load scroll spritesheets into UI
                gameUI.LoadScrollSheets(basePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Image load failed. Make sure your Graphics folder is in the project root! Error: " + ex.Message);
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
            GameManager.Player.HandleKeyDown(e);

            // Scroll switching (1-4)
            // Store old element before switch attempt
            var previousElement = GameManager.Player.CurrentElement;
            GameManager.Player.HandleScrollSwitch(e.KeyCode);

            // If a new pending element was set, tell the UI to start the animation
            if (GameManager.Player.PendingElement != null && GameManager.Player.CurrentElement == previousElement)
            {
                gameUI.StartScrollSwitch(GameManager.Player.PendingElement.Type);
            }

            GameManager.Player.HandleKeyDown(e);
        }

        private void Convergence_Load(object sender, EventArgs e) { }

        private void Convergence_KeyUp(object sender, KeyEventArgs e)
        {
            GameManager.Player.HandleKeyUp(e);
        }

        private void Convergence_MouseClick(object sender, MouseEventArgs e)
        {
            GameManager.Player.HandleMouseClick(e);
        }

        // --- GAME LOOP UPDATE ---
        private async void GameLoop_Tick(object sender, EventArgs e)
        {
            GameManager.Player.Update();

            // 3. Update scroll animation and check for animation milestones
            var (closingDone, openingDone) = gameUI.UpdateScrollAnimation();

            if (closingDone)
            {
                // Closing animation finished — confirm the switch in Player
                GameManager.Player.ConfirmScrollSwitch();
            }

            if (openingDone)
            {
                // Opening animation finished — unlock switching
                GameManager.Player.UnlockScrollSwitch();
            }

            // 4. Token update and pickup check
            if (airToken != null)
            {
                airToken.Update();

                if (!GameManager.Player.IsPlayingPickup && !GameManager.Player.IsPlayingSlash)
                {
                    float dx = (airToken.Position.X + Token.DrawSize / 2f) - (GameManager.Player.Position.X + 112f);
                    float dy = (airToken.Position.Y + Token.DrawSize / 2f) - (GameManager.Player.Position.Y + 112f);
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= Token.PickupRange)
                    {
                        GameManager.DespawnObject(airToken);
                        airToken = null;
                        GameManager.Player.TriggerPickupAnimation();
                    }
                }
            }

            // 5. Sync UI with actual player health and stamina
            gameUI.UpdateHealth(GameManager.Player.Health, GameManager.Player.MaxHealth);
            gameUI.UpdateStamina(GameManager.Player.Stamina, GameManager.Player.MaxStamina);

            // 6. Trigger redraw
            this.Invalidate();
        }

        // --- DRAW TO THE SCREEN ---
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            GameManager.Player.Draw(e.Graphics);
            airToken?.Draw(e.Graphics);

            GameManager.Player.DrawHitbox(e.Graphics);

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