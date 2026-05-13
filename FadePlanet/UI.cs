using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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



        // =========================
        //       DRAW METHOD
        // =========================

        public void DrawWinFormsUI(Graphics g, Image hGraphic, Image hBar, Image sGraphic, Image sBar)
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



            // Scaled dimensions
            int scaledHealthBarWidth = hBar.Width * scale;
            int scaledHealthBarHeight = hBar.Height * scale;

            int scaledHealthGraphicWidth = hGraphic.Width * scale;
            int scaledHealthGraphicHeight = hGraphic.Height * scale;



            // Fill amount
            int currentHealthWidth = (int)(scaledHealthBarWidth * healthPercent);



            // Draw fill bar
            if (currentHealthWidth > 0)
            {
                Rectangle destRect = new Rectangle(
                    healthX,
                    healthY,
                    currentHealthWidth,
                    scaledHealthBarHeight
                );

                Rectangle srcRect = new Rectangle(
                    0,
                    0,
                    (int)(hBar.Width * healthPercent),
                    hBar.Height
                );

                g.DrawImage(hBar, destRect, srcRect, GraphicsUnit.Pixel);
            }



            // Draw frame graphic
            g.DrawImage(
                hGraphic,
                new Rectangle(
                    healthX,
                    healthY,
                    scaledHealthGraphicWidth,
                    scaledHealthGraphicHeight
                )
            );



            // =========================
            //      STAMINA BAR
            // =========================

            int staminaX = 20;

            // Moved closer to health bar
            int staminaY = 80;



            // Scaled dimensions
            int scaledStaminaBarWidth = sBar.Width * scale;
            int scaledStaminaBarHeight = sBar.Height * scale;

            int scaledStaminaGraphicWidth = sGraphic.Width * scale;
            int scaledStaminaGraphicHeight = sGraphic.Height * scale;



            // Fill amount
            int currentStaminaWidth = (int)(scaledStaminaBarWidth * staminaPercent);



            // Draw fill bar
            if (currentStaminaWidth > 0)
            {
                Rectangle destRectStamina = new Rectangle(
                    staminaX,
                    staminaY,
                    currentStaminaWidth,
                    scaledStaminaBarHeight
                );

                Rectangle srcRectStamina = new Rectangle(
                    0,
                    0,
                    (int)(sBar.Width * staminaPercent),
                    sBar.Height
                );

                g.DrawImage(sBar, destRectStamina, srcRectStamina, GraphicsUnit.Pixel);
            }



            // Draw frame graphic
            g.DrawImage(
                sGraphic,
                new Rectangle(
                    staminaX,
                    staminaY,
                    scaledStaminaGraphicWidth,
                    scaledStaminaGraphicHeight
                )
            );
        }
    }
}


