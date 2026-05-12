using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        public Convergence()
        {
            InitializeComponent();

            // Set window size
            this.ClientSize = new Size(1080, 720);

            // CRITICAL FOR WINFORMS GAMES
            this.DoubleBuffered = true;

            // Allows the form to capture key presses before other controls get them
            this.KeyPreview = true;
            this.KeyDown += Convergence_KeyDown;
            this.KeyUp += Convergence_KeyUp;

            // --- INITIALIZE CLASSES ---
            gameUI = new UI();

            // Start player centered on screen (1080/2 - 112, 720/2 - 112)
            player = new PlayerMovement(428f, 248f);

            // --- LOAD IMAGES ---
            string basePath = Application.StartupPath;

            try
            {
                healthGraphic = Image.FromFile(System.IO.Path.Combine(basePath, @"Graphics\UI\HealthGraphic.png"));
                healthBar = Image.FromFile(System.IO.Path.Combine(basePath, @"Graphics\UI\HealthBar.png"));
                staminaGraphic = Image.FromFile(System.IO.Path.Combine(basePath, @"Graphics\UI\StaminaGraphic.png"));
                staminaBar = Image.FromFile(System.IO.Path.Combine(basePath, @"Graphics\UI\StaminaBar.png"));

                player.LoadImages();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Image load failed. Make sure folders exist and 'Copy if newer' is set! Error: " + ex.Message);
            }

            // --- START GAME LOOP ---
            gameLoop = new System.Windows.Forms.Timer();
            gameLoop.Interval = 16; // ~60 FPS
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
            // 1. Send inputs to the player and update their logic/animation
            player.SetInput(moveUp, moveLeft, moveDown, moveRight);
            player.Update();

            // 2. UI Logic (Dummy drain effect)
            testHealth -= 0.2f;
            testStamina -= 0.5f;
            if (testHealth <= 0) testHealth = 100f;
            if (testStamina <= 0) testStamina = 100f;

            gameUI.UpdateHealth(testHealth, 100f);
            gameUI.UpdateStamina(testStamina, 100f);

            // 3. Trigger Redraw
            this.Invalidate();
        }

        // --- DRAW TO THE SCREEN ---
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw the Player FIRST so they are underneath the UI
            player.Draw(e.Graphics);

            // Draw the UI LAST so it sits on top of everything
            if (healthGraphic != null && healthBar != null && staminaGraphic != null && staminaBar != null)
            {
                gameUI.DrawWinFormsUI(e.Graphics, healthGraphic, healthBar, staminaGraphic, staminaBar);
            }
        }
    }
}