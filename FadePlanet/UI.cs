using System;
using System.Collections.Generic;
using System.Drawing; // Required for handling Graphics, Images, and Rectangles
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadePlanet
{
    internal class UI
    {
        // --- 1. VARIABLES ---

        // Track the stats. We use float so the division for percentages works correctly.
        public float MaxHealth { get; private set; } = 100f;
        public float CurrentHealth { get; private set; } = 100f;

        public float MaxStamina { get; private set; } = 100f;
        public float CurrentStamina { get; private set; } = 100f;


        // --- 2. UPDATE METHODS ---

        public void UpdateHealth(float currentHealth, float maxHealth)
        {
            // Math.Min ensures it doesn't go above maxHealth
            // Math.Max ensures it doesn't drop below 0
            CurrentHealth = Math.Max(0, Math.Min(currentHealth, maxHealth));
            MaxHealth = maxHealth;
        }

        public void UpdateStamina(float currentStamina, float maxStamina)
        {
            // Same manual clamping logic here
            CurrentStamina = Math.Max(0, Math.Min(currentStamina, maxStamina));
            MaxStamina = maxStamina;
        }



        // --- 3. DRAWING METHOD ---

        // This is called inside Convergence.cs during the OnPaint event.
        public void DrawWinFormsUI(Graphics g, Image hGraphic, Image hBar, Image sGraphic, Image sBar)
        {
            // Calculate Percentages
            float healthPercent = CurrentHealth / MaxHealth;
            float staminaPercent = CurrentStamina / MaxStamina;

            // =========================
            //      DRAW HEALTH BAR
            // =========================

            // Set position of the Health UI on screen
            int healthX = 20;
            int healthY = 20;

            // A. Draw the background frame first
            g.DrawImage(hGraphic, healthX, healthY);

            // B. Calculate how many pixels of the inner bar to show based on health percentage
            int currentHealthWidth = (int)(hBar.Width * healthPercent);

            // Only draw the inner bar if health is greater than 0
            if (currentHealthWidth > 0)
            {
                // Destination: Where it goes on the screen
                Rectangle destRect = new Rectangle(healthX, healthY, currentHealthWidth, hBar.Height);
                // Source: The exact pixels we are copying from the image file (This creates the clipping effect)
                Rectangle srcRect = new Rectangle(0, 0, currentHealthWidth, hBar.Height);

                g.DrawImage(hBar, destRect, srcRect, GraphicsUnit.Pixel);
            }

            // =========================
            //     DRAW STAMINA BAR
            // =========================

            // Set position of the Stamina UI on screen just below the health bar
            int staminaX = 20;
            int staminaY = 80;

            // A. Draw the background frame first
            g.DrawImage(sGraphic, staminaX, staminaY);

            // B. Calculate how many pixels of the inner bar to show based on stamina percentage
            int currentStaminaWidth = (int)(sBar.Width * staminaPercent);

            // Only draw the inner bar if stamina is greater than 0
            if (currentStaminaWidth > 0)
            {
                // Destination: Where it goes on the screen
                Rectangle destRectStamina = new Rectangle(staminaX, staminaY, currentStaminaWidth, sBar.Height);
                // Source: The exact pixels we are copying from the image file
                Rectangle srcRectStamina = new Rectangle(0, 0, currentStaminaWidth, sBar.Height);

                g.DrawImage(sBar, destRectStamina, srcRectStamina, GraphicsUnit.Pixel);
            }
        }
    }
}

