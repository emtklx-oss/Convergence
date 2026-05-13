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
        private PlayerMovement player;
        private Token airToken;

        // UI Images
        private Image healthGraphic;
        private Image healthBar;
        private Image staminaGraphic;
        private Image staminaBar;

        private System.Windows.Forms.Timer gameLoop;

        // Input booleans
        private bool moveUp, moveDown, moveLeft, moveRight;

        // Dummy variables for UI testing
        private float testHealth = 100f;
        private float testStamina = 100f;

        // Walks up from bin\Debug to the project root where Graphics folder lives
        private string GetProjectRoot()
        {
            string path = Application.StartupPath;
            return Path.GetFullPath(Path.Combine(path, @"..\..\"));
        }

        public Convergence()
        {
            InitializeComponent();

            this.ClientSize = new Size(1080, 720);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.KeyDown += Convergence_KeyDown;
            this.KeyUp += Convergence_KeyUp;

            // --- INITIALIZE CLASSES ---
            gameUI = new UI();
            player = new PlayerMovement(428f, 248f);
            airToken = new Token(new Point(600, 300), new Size(Token.DrawSize, Token.DrawSize));

            // --- LOAD IMAGES ---
            string basePath = GetProjectRoot();

            try
            {
                healthGraphic = Image.FromFile(Path.Combine(basePath, @"Graphics\UI\HealthGraphic.png"));
                healthBar = Image.FromFile(Path.Combine(basePath, @"Graphics\UI\HealthBar.png"));
                staminaGraphic = Image.FromFile(Path.Combine(basePath, @"Graphics\UI\StaminaGraphic.png"));
                staminaBar = Image.FromFile(Path.Combine(basePath, @"Graphics\UI\StaminaBar.png"));

                player.LoadImages();
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
            if (e.KeyCode == Keys.W) moveUp = true;
            if (e.KeyCode == Keys.S) moveDown = true;
            if (e.KeyCode == Keys.A) moveLeft = true;
            if (e.KeyCode == Keys.D) moveRight = true;
        }

        private void Convergence_Load(object sender, EventArgs e) { }

        private void Convergence_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W) moveUp = false;
            if (e.KeyCode == Keys.S) moveDown = false;
            if (e.KeyCode == Keys.A) moveLeft = false;
            if (e.KeyCode == Keys.D) moveRight = false;
        }

        // --- GAME LOOP UPDATE ---
        private void GameLoop_Tick(object sender, EventArgs e)
        {
            // 1. Input and player update
            player.SetInput(moveUp, moveLeft, moveDown, moveRight);
            player.Update();

            // 2. Token update and pickup check
            if (airToken != null)
            {
                airToken.Update();

                if (!player.IsPlayingPickup)
                {
                    float dx = (airToken.Position.X + Token.DrawSize / 2f) - (player.X + 112f);
                    float dy = (airToken.Position.Y + Token.DrawSize / 2f) - (player.Y + 112f);
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= Token.PickupRange)
                    {
                        GameManager.DespawnObject(airToken);
                        airToken = null;
                        player.TriggerPickupAnimation();
                    }
                }
            }

            // 3. UI dummy drain
            testHealth -= 0.2f;
            testStamina -= 0.5f;
            if (testHealth <= 0) testHealth = 100f;
            if (testStamina <= 0) testStamina = 100f;

            gameUI.UpdateHealth(testHealth, 100f);
            gameUI.UpdateStamina(testStamina, 100f);

            // 4. Trigger redraw
            this.Invalidate();
        }

        // --- DRAW TO THE SCREEN ---
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            player.Draw(e.Graphics);
            airToken?.Draw(e.Graphics);

            if (healthGraphic != null && healthBar != null && staminaGraphic != null && staminaBar != null)
            {
                gameUI.DrawWinFormsUI(e.Graphics, healthGraphic, healthBar, staminaGraphic, staminaBar);
            }
        }
    }
}