using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace FadePlanet
{
    public partial class Puzzles : Form
    {
        public event Action PuzzleCompleted;

        private readonly ElementType element;
        private Image background;
        private readonly Dictionary<ArrowGraphic, Image> arrowImages = new Dictionary<ArrowGraphic, Image>();
        private List<PuzzleArrowPiece> airPieces;
        private bool solved;

        public Puzzles(ElementType element)
        {
            this.element = element;
            InitializeComponent();

            ClientSize = new Size(1280, 720);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Text = "Puzzle";
            KeyPreview = true;
            KeyDown += Puzzles_KeyDown;
            MouseClick += Puzzles_MouseClick;

            LoadAssets();
        }

        private string GetProjectRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\"));
        }

        private void LoadAssets()
        {
            string basePath = GetProjectRoot();
            string puzzleFolder = Path.Combine(basePath, @"Graphics\Realms\Puzzles");
            string arrowFolder = Path.Combine(basePath, @"Graphics\Realms\AirPuzzle Arrows");

            string backgroundFile;
            switch (element)
            {
                case ElementType.Earth: backgroundFile = "EarthPuzzle.png"; break;
                case ElementType.Water: backgroundFile = "WaterPuzzle.png"; break;
                case ElementType.Fire: backgroundFile = "FirePuzzle.png"; break;
                default: backgroundFile = "AirPuzzle.png"; break;
            }

            background = Image.FromFile(Path.Combine(puzzleFolder, backgroundFile));

            arrowImages[ArrowGraphic.Straight] = Image.FromFile(Path.Combine(arrowFolder, "Right.png"));
            arrowImages[ArrowGraphic.DownRight] = Image.FromFile(Path.Combine(arrowFolder, "DownRight.png"));
            arrowImages[ArrowGraphic.LeftUp] = Image.FromFile(Path.Combine(arrowFolder, "LeftUp.png"));
            arrowImages[ArrowGraphic.UpRight] = Image.FromFile(Path.Combine(arrowFolder, "UpRight.png"));
            arrowImages[ArrowGraphic.UpLeft] = Image.FromFile(Path.Combine(arrowFolder, "UpLeft.png"));

            if (element == ElementType.Air)
                airPieces = new List<PuzzleArrowPiece>(AirPuzzleLogic.CreatePieces());
        }

        private void Puzzles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        private PointF ScreenToDesign(Point screenPoint)
        {
            float scaleX = (float)ClientSize.Width / AirPuzzleLogic.DesignWidth;
            float scaleY = (float)ClientSize.Height / AirPuzzleLogic.DesignHeight;
            return new PointF(screenPoint.X / scaleX, screenPoint.Y / scaleY);
        }

        private void Puzzles_MouseClick(object sender, MouseEventArgs e)
        {
            if (element != ElementType.Air || airPieces == null || solved) return;

            PointF designPoint = ScreenToDesign(e.Location);
            PuzzleArrowPiece piece = AirPuzzleLogic.HitTest(airPieces, designPoint);
            if (piece == null) return;

            piece.Rotate();
            if (AirPuzzleLogic.IsSolved(airPieces))
            {
                solved = true;
                PuzzleCompleted?.Invoke();
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.None;

            if (background != null)
            {
                g.DrawImage(background, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
            }

            if (element == ElementType.Air && airPieces != null)
            {
                float scaleX = (float)ClientSize.Width / AirPuzzleLogic.DesignWidth;
                float scaleY = (float)ClientSize.Height / AirPuzzleLogic.DesignHeight;

                foreach (var piece in airPieces)
                {
                    DrawArrowPiece(g, piece, scaleX, scaleY);
                }

                if (solved)
                {
                    using (var font = new Font("Arial", 14, FontStyle.Bold))
                    using (var brush = new SolidBrush(Color.FromArgb(220, 40, 90, 40)))
                    {
                        string msg = "Puzzle Solved!";
                        var size = g.MeasureString(msg, font);
                        g.DrawString(msg, font, brush,
                            (ClientSize.Width - size.Width) / 2f,
                            ClientSize.Height * 0.08f);
                    }
                }
            }
        }

        private void DrawArrowPiece(Graphics g, PuzzleArrowPiece piece, float scaleX, float scaleY)
        {
            if (!arrowImages.TryGetValue(piece.Graphic, out Image img)) return;

            PointF designCenter = AirPuzzleLogic.GetPieceCenter(piece);
            float screenX = designCenter.X * scaleX;
            float screenY = designCenter.Y * scaleY;
            float drawSize = AirPuzzleLogic.ArrowDrawSize * scaleX;

            var state = g.Save();
            g.TranslateTransform(screenX, screenY);
            g.RotateTransform(piece.Rotation * 90f);
            g.DrawImage(img, -drawSize / 2f, -drawSize / 2f, drawSize, drawSize);
            g.Restore(state);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            background?.Dispose();
            foreach (var img in arrowImages.Values)
                img?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
