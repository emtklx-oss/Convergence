using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FadePlanet
{
    internal class OldMan : WorldObject
    {
        public const float InteractDistance = 100f;
        private bool firstInteraction = true;
        private Image ManImage;

        private DialogForm DialogWindow;
        
        public OldMan(PointF pos, SizeF size, ObjectType type = ObjectType.Friendly) : base(pos, size, type)
        {
            LoadImage();
        }

        private void LoadImage()
        {
            try
            {
                string imagePath = Path.Combine(Application.StartupPath, @"..\..\Graphics\Other Chars\WiseOldman.png");
                ManImage = Image.FromFile(imagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error laoding old man: " + ex.Message);
            }
        }
        public override void OnInteract(Player player)
        {

            if (firstInteraction)
            {
                ShowTutorial();
                firstInteraction = false;
            }
            else
            {
                ShowMenu();
            }
        }
        public override void Draw(Graphics g)
        {
            if (ManImage == null) return;

            g.DrawImage(
                ManImage,
                new RectangleF(
                    Position.X,
                    Position.Y,
                    ObjSize.Width,
                    ObjSize.Height
                )
            );
        }
        private void ShowTutorial()
        {
            string tutorialText = "Welcome, young one!\n\n" +
                "In this land, you must survive by:\n\n" +
                "1. FIGHTING ENEMIES\n" +
                "Use your sword to defeat enemies. Press right-click for basic attack.\n" +
                "Switch between scroll types to use different abilities with left-click\n" +
                "Abilities:\n\n" +
                "   • Fire: Shoot fireballs at enemies\n" +
                "   • Water: Create protective ripples\n" +
                "   • Earth: Raise rock barriers for defense\n" +
                "   • Air: Swift aerial attacks\n\n" +
                "2. COLLECTING TOKENS\n" +
                "Defeat all enemies of each realm to collect the realm's token." +
                "3. MANAGING STAMINA\n" +
                "Each ability costs stamina. Rest to regenerate it.\n\n" +
                "4. CURRENCY & POTIONS\n" +
                "Defeat enemies to earn currency. Use it at my shop to buy healing potions.\n\n" +
                "Good luck on your journey!";

            DialogResult result = MessageBox.Show(tutorialText, "Tutorial - Welcome", MessageBoxButtons.OK);
        }

        private void ShowMenu()
        {

            string menuText = "Greetings again, traveler!\n\nWhat can I help you with?";

            using (DialogWindow = new DialogForm("Old Man", menuText, "Tutorial", "Shop", "Nevermind"))
            {
                DialogWindow.ShowDialog();

                string selectedButton = (string)DialogWindow.Tag;

                switch (selectedButton)
                {
                    case "Tutorial":
                        ShowTutorialAgain();
                        break;
                    case "Shop":
                        ShowShop();
                        break;
                    case "Nevermind":
                        //Close form
                        DialogWindow.Close();
                        break;
                }
            }
        }

        private void ShowTutorialAgain()
        {
            string tutorialText = "Here's a reminder of the basics:\n\n" +
                "• LEFT-CLICK: Attack with your current ability\n" +
                "• RIGHT-CLICK: Basic sword attack\n" +
                "• 1-4 KEYS: Switch between elemental scrolls\n" +
                "• LEFT-CLICK: With the potion icon selected in the hotbar, consume a healing potion\n" +
                "• Collect token from realm after having defeated all enemies\n" +
                "• Use your currency to buy potions from my shop\n\n" +
                "Good luck out there!";

            MessageBox.Show(tutorialText, "Tutorial Reminder", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowShop()
        {
            string shopText = "Welcome to my shop!\n\n" +
                "I sell healing potions for 10 currency each.\n" +
                "(Shop functionality coming soon...)";

            using (DialogWindow = new DialogForm("Old Man's Shop", shopText, "Buy Potion - $10", "Nevermind"))
            {
                DialogWindow.ShowDialog();

                string selectedButton = (string)DialogWindow.Tag;

                switch(selectedButton)
                {
                    case "Buy Potion - $10":
                        if (GameManager.CurPlayer.Currency >= 10)
                        {
                            GameManager.CurPlayer.AddCurrency(-10); //Takes 10
                            GameManager.CurPlayer.AddPotions(1);
                            MessageBox.Show("Thank you for your purchase!", "Purchase confirmed", MessageBoxButtons.OK);
                        }
                        else
                        {
                            MessageBox.Show("Don't try that again...", "Purchase failed - Your too poor!", MessageBoxButtons.OK);
                        }
                        
                        break;
                    case "Nevermind":
                        // Return to menu
                        ShowMenu();
                        break;
                }
            }
        }
    }
}
