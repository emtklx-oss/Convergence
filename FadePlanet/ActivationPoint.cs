using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace FadePlanet
{
    public class ActivationPoint : WorldObject
    {
        public const float InteractRadius = 120f;
        public const int FrameCount = 8;
        public const float DrawScale = 2.5f;

        public ElementType Element { get; private set; }
        public bool PlayerInRange { get; private set; }
        public bool Activated { get; private set; }

        private static Bitmap spritesheet;
        private static int frameWidth;
        private static int frameHeight;

        private int IdleFrameIndex => GetFramePairIndex() * 2;
        private int ActiveFrameIndex => IdleFrameIndex + 1;

        public ActivationPoint(PointF pos, ElementType element) : base(pos, new SizeF(1, 1), ObjectType.ActivationPoint)
        {
            Element = element;
            EnsureSpritesheetLoaded();
            ObjSize = new SizeF(frameWidth * DrawScale, frameHeight * DrawScale);
        }

        private static void EnsureSpritesheetLoaded()
        {
            if (spritesheet != null) return;

            string path = Path.Combine(
                Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\")),
                @"Graphics\Realms\Puzzles\ActivationPoints.png");

            spritesheet = new Bitmap(Image.FromFile(path));
            frameWidth = spritesheet.Width / FrameCount;
            frameHeight = spritesheet.Height;
        }

        private int GetFramePairIndex()
        {
            switch (Element)
            {
                case ElementType.Earth: return 0;
                case ElementType.Water: return 1;
                case ElementType.Air: return 2;
                case ElementType.Fire: return 3;
                default: return 0;
            }
        }

        public void UpdateProximity(Player player)
        {
            if (Activated) return;

            float centerX = Position.X + ObjSize.Width / 2f;
            float centerY = Position.Y + ObjSize.Height / 2f;
            float playerCenterX = player.Position.X + 112f;
            float playerCenterY = player.Position.Y + 112f;

            float dx = centerX - playerCenterX;
            float dy = centerY - playerCenterY;
            PlayerInRange = (dx * dx + dy * dy) <= InteractRadius * InteractRadius;
        }

        public void MarkActivated()
        {
            Activated = true;
            PlayerInRange = false;
        }

        public override void Draw(Graphics g)
        {
            if (spritesheet == null) return;

            int frameIndex = Activated ? ActiveFrameIndex : (PlayerInRange ? ActiveFrameIndex : IdleFrameIndex);
            Rectangle src = new Rectangle(frameIndex * frameWidth, 0, frameWidth, frameHeight);
            float drawW = frameWidth * DrawScale;
            float drawH = frameHeight * DrawScale;
            RectangleF dest = new RectangleF(Position.X, Position.Y, drawW, drawH);

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(spritesheet, dest, src, GraphicsUnit.Pixel);
        }

        public override void OnInteract(Player player)
        {
            if (!PlayerInRange || Activated) return;
            MarkActivated();
        }
    }
}
