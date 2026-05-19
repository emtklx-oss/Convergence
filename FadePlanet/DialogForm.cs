using System;
using System.Drawing;
using System.Windows.Forms;

namespace FadePlanet
{
    public partial class DialogForm : Form
    {
        public new DialogResult DialogResult { get; set; }

        public DialogForm(string title, string message, params string[] buttonLabels)
        {
            InitializeComponent();

            this.Text = title;
            labelMessage.Text = message;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            this.Controls.Clear(); // Clear existing controls to add new ones based on parameters

            // Create buttons based on labels provided
            int buttonCount = buttonLabels.Length;
            int buttonWidth = 100;
            int buttonHeight = 30;
            int spacing = 10;
            int startX = (this.ClientSize.Width - (buttonCount * buttonWidth + (buttonCount - 1) * spacing)) / 2;
            int buttonY = labelMessage.Bottom + 30;

            for (int i = 0; i < buttonCount; i++)
            {
                Button btn = new Button();
                btn.Text = buttonLabels[i];
                btn.Width = buttonWidth;
                btn.Height = buttonHeight;
                btn.Left = startX + (i * (buttonWidth + spacing));
                btn.Top = buttonY;
                btn.Click += (sender, e) =>
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Tag = btn.Text; // Store which button was clicked
                    this.Close();
                };
                this.Controls.Add(btn);
            }

            // Adjust form height based on content
            this.ClientSize = new Size(Math.Max(400, startX + (buttonCount * buttonWidth + (buttonCount - 1) * spacing) + 20), buttonY + buttonHeight + 20);
        }

        private void InitializeComponent()
        {
            labelMessage = new Label();
            this.SuspendLayout();

            // labelMessage
            labelMessage.AutoSize = false;
            labelMessage.Dock = DockStyle.Top;
            labelMessage.Font = new Font("Segoe UI", 10F);
            labelMessage.Padding = new Padding(20, 20, 20, 0);
            labelMessage.Text = "";
            labelMessage.Height = 100;
            labelMessage.TextAlign = ContentAlignment.TopLeft;

            // DialogForm
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(400, 150);
            this.Controls.Add(labelMessage);
            this.Font = new Font("Segoe UI", 9F);
            this.Name = "DialogForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
        }

        private Label labelMessage;
        
    }
}
