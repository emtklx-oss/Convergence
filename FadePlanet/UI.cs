using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FadePlanet
{
    internal class UI
    {
        // =========================
        //        VARIABLES
        // =========================

        public float MaxHealth { get; private set; } = 100f;
        public float CurrentHealth { get; private set; } = 100f;

        public float MaxStamina { get; private set; } = 100f;
        public float CurrentStamina { get; private set; } = 100f;

        // =========================
        //    INVENTORY SLOT SETTINGS
        // =========================
        private const int InventoryScale = 2;
        private const int InventoryOriginalW = 208;
        private const int InventoryOriginalH = 44;
        private const int InventoryDrawW = InventoryOriginalW * InventoryScale;
        private const int InventoryDrawH = InventoryOriginalH * InventoryScale;
        private const int InventoryMargin = 16;

        // =========================
        //    SCROLL DISPLAY SETTINGS
        // =========================
        // Each frame is 32x32, 7 frames per sheet, displayed at 4x scale = 128x128
        private const int ScrollFrameSize = 32;
        private const int ScrollTotalFrames = 7;
        private const int ScrollScale = 5;
        private const int ScrollDrawSize = ScrollFrameSize * ScrollScale; // 128px
        private const int ScrollMargin = 16;
        private const int ScrollFrameDuration = 4; // Ticks per frame

        // Scroll spritesheets
        private Bitmap airScrollSheet;
        private Bitmap earthScrollSheet;
        private Bitmap fireScrollSheet;
        private Bitmap waterScrollSheet;

        // Scroll animation state
        private enum ScrollAnimState { Idle, Closing, Opening }
        private ScrollAnimState scrollState = ScrollAnimState.Idle;
        private int scrollFrameIndex = 0;
        private int scrollFrameTimer = 0;

        // Which sheet to draw from
        private Bitmap currentScrollSheet;
        private Bitmap pendingScrollSheet;



        // =========================
        //      LOAD SCROLL SHEETS
        // =========================

        public void LoadScrollSheets(string projectRoot)
        {
            try
            {
                airScrollSheet = new Bitmap(Path.Combine(projectRoot, @"Graphics\Items\Scrolls\AirScroll.png"));
                earthScrollSheet = new Bitmap(Path.Combine(projectRoot, @"Graphics\Items\Scrolls\EarthScroll.png"));
                fireScrollSheet = new Bitmap(Path.Combine(projectRoot, @"Graphics\Items\Scrolls\FireScroll.png"));
                waterScrollSheet = new Bitmap(Path.Combine(projectRoot, @"Graphics\Items\Scrolls\WaterScroll.png"));

                // Default to AirScroll on game start, sitting on frame 1
                currentScrollSheet = airScrollSheet;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load scroll sheets: " + ex.Message);
            }
        }

        // Called when the player requests a scroll switch
        public void StartScrollSwitch(ElementType newElement)
        {
            // Set the pending sheet based on what we are switching to
            switch (newElement)
            {
                case ElementType.Air: pendingScrollSheet = airScrollSheet; break;
                case ElementType.Earth: pendingScrollSheet = earthScrollSheet; break;
                case ElementType.Fire: pendingScrollSheet = fireScrollSheet; break;
                case ElementType.Water: pendingScrollSheet = waterScrollSheet; break;
            }

            // Start closing animation from frame 1
            scrollState = ScrollAnimState.Closing;
            scrollFrameIndex = 0;
            scrollFrameTimer = 0;
        }

        // =========================
        //      UPDATE METHODS
        // =========================

        public void UpdateHealth(float currentHealth, float maxHealth)
        {
            CurrentHealth = Math.Max(0, Math.Min(currentHealth, maxHealth));
            MaxHealth = maxHealth;
        }

        public void UpdateStamina(float currentStamina, float maxStamina)
        {
            CurrentStamina = Math.Max(0, Math.Min(currentStamina, maxStamina));
            MaxStamina = maxStamina;
        }

        // Returns true when closing animation finishes (time to confirm switch in Player)
        // Returns false when opening animation finishes (time to unlock switch in Player)
        public (bool closingDone, bool openingDone) UpdateScrollAnimation()
        {
            bool closingDone = false;
            bool openingDone = false;

            if (scrollState == ScrollAnimState.Idle) return (false, false);

            scrollFrameTimer++;

            if (scrollFrameTimer >= ScrollFrameDuration)
            {
                scrollFrameTimer = 0;

                if (scrollState == ScrollAnimState.Closing)
                {
                    scrollFrameIndex++;

                    if (scrollFrameIndex >= ScrollTotalFrames)
                    {
                        // Closing done — switch to pending sheet, start opening from last frame
                        currentScrollSheet = pendingScrollSheet;
                        scrollFrameIndex = ScrollTotalFrames - 1;
                        scrollState = ScrollAnimState.Opening;
                        closingDone = true;
                    }
                }
                else if (scrollState == ScrollAnimState.Opening)
                {
                    scrollFrameIndex--;

                    if (scrollFrameIndex < 0)
                    {
                        // Opening done — land on frame 1, go idle
                        scrollFrameIndex = 0;
                        scrollState = ScrollAnimState.Idle;
                        openingDone = true;
                    }
                }
            }

            return (closingDone, openingDone);
        }



        // =========================
        //       DRAW METHOD
        // =========================

        public void DrawWinFormsUI(Graphics g, Image hGraphic, Image hBar, Image sGraphic, Image sBar, Image inventorySlots, int screenWidth, int screenHeight)
        {
            // =========================
            //      PIXEL ART SETTINGS
            // =========================

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.None;



            // =========================
            //        UI SCALE
            // =========================

            int scale = 2;



            // =========================
            //     HEALTH/STAMINA %
            // =========================

            float healthPercent = CurrentHealth / MaxHealth;
            float staminaPercent = CurrentStamina / MaxStamina;



            // =========================
            //      HEALTH BAR
            // =========================

            int healthX = 20;
            int healthY = 20;

            int scaledHealthBarWidth = hBar.Width * scale;
            int scaledHealthBarHeight = hBar.Height * scale;
            int scaledHealthGraphicWidth = hGraphic.Width * scale;
            int scaledHealthGraphicHeight = hGraphic.Height * scale;

            int currentHealthWidth = (int)(scaledHealthBarWidth * healthPercent);

            if (currentHealthWidth > 0)
            {
                Rectangle destRect = new Rectangle(healthX, healthY, currentHealthWidth, scaledHealthBarHeight);
                Rectangle srcRect = new Rectangle(0, 0, (int)(hBar.Width * healthPercent), hBar.Height);
                g.DrawImage(hBar, destRect, srcRect, GraphicsUnit.Pixel);
            }

            g.DrawImage(hGraphic, new Rectangle(healthX, healthY, scaledHealthGraphicWidth, scaledHealthGraphicHeight));



            // =========================
            //      STAMINA BAR
            // =========================

            int staminaX = 20;
            int staminaY = 80;

            int scaledStaminaBarWidth = sBar.Width * scale;
            int scaledStaminaBarHeight = sBar.Height * scale;
            int scaledStaminaGraphicWidth = sGraphic.Width * scale;
            int scaledStaminaGraphicHeight = sGraphic.Height * scale;

            int currentStaminaWidth = (int)(scaledStaminaBarWidth * staminaPercent);

            if (currentStaminaWidth > 0)
            {
                Rectangle destRectStamina = new Rectangle(staminaX, staminaY, currentStaminaWidth, scaledStaminaBarHeight);
                Rectangle srcRectStamina = new Rectangle(0, 0, (int)(sBar.Width * staminaPercent), sBar.Height);
                g.DrawImage(sBar, destRectStamina, srcRectStamina, GraphicsUnit.Pixel);
            }

            g.DrawImage(sGraphic, new Rectangle(staminaX, staminaY, scaledStaminaGraphicWidth, scaledStaminaGraphicHeight));



            // =========================
            //      INVENTORY SLOTS
            // =========================

            int inventoryX = InventoryMargin;
            int inventoryY = screenHeight - InventoryDrawH - InventoryMargin;

            g.DrawImage(inventorySlots, new Rectangle(inventoryX, inventoryY, InventoryDrawW, InventoryDrawH));



            // =========================
            //      ACTIVE SCROLL
            // =========================

            if (currentScrollSheet != null)
            {
                // Position bottom right
                int scrollX = screenWidth - ScrollDrawSize - ScrollMargin;
                int scrollY = screenHeight - ScrollDrawSize - ScrollMargin;

                // Grab the correct frame from the spritesheet
                Rectangle srcRect = new Rectangle(scrollFrameIndex * ScrollFrameSize, 0, ScrollFrameSize, ScrollFrameSize);
                Rectangle destRect = new Rectangle(scrollX, scrollY, ScrollDrawSize, ScrollDrawSize);

                g.DrawImage(currentScrollSheet, destRect, srcRect, GraphicsUnit.Pixel);
            }
        }
    }
}