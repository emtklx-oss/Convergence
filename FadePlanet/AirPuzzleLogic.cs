using System;
using System.Collections.Generic;
using System.Drawing;

namespace FadePlanet
{
    public enum ArrowGraphic
    {
        Straight,
        DownRight,
        LeftUp,
        UpRight,
        UpLeft
    }

    public class PuzzleArrowPiece
    {
        public int GridX { get; }
        public int GridY { get; }
        public ArrowGraphic Graphic { get; }
        public int SolvedRotation { get; }
        public int Rotation { get; set; }

        public PuzzleArrowPiece(int gridX, int gridY, ArrowGraphic graphic, int solvedRotation, int startRotation)
        {
            GridX = gridX;
            GridY = gridY;
            Graphic = graphic;
            SolvedRotation = solvedRotation;
            Rotation = startRotation;
        }

        public bool IsCorrect => Rotation % 4 == SolvedRotation % 4;

        public void Rotate()
        {
            Rotation = (Rotation + 1) % 4;
        }
    }

    public static class AirPuzzleLogic
    {
        public const int DesignWidth = 320;
        public const int DesignHeight = 180;
        public const int CellSize = 32;
        public const int ArrowDrawSize = 128;
        public const int GridOriginX = 108;
        public const int GridOriginY = 32;

        // Solved path: Right -> RightDown -> Down -> DownLeft -> UpRight -> Up -> Up -> UpLeft -> Left
        public static IReadOnlyList<PuzzleArrowPiece> CreatePieces()
        {
            return new List<PuzzleArrowPiece>
            {
                new PuzzleArrowPiece(0, 1, ArrowGraphic.Straight, 0, 2),
                new PuzzleArrowPiece(1, 1, ArrowGraphic.DownRight, 1, 0),
                new PuzzleArrowPiece(1, 2, ArrowGraphic.Straight, 1, 3),
                new PuzzleArrowPiece(2, 2, ArrowGraphic.LeftUp, 3, 1),
                new PuzzleArrowPiece(3, 2, ArrowGraphic.LeftUp, 1, 2),
                new PuzzleArrowPiece(3, 3, ArrowGraphic.Straight, 3, 0),
                new PuzzleArrowPiece(3, 4, ArrowGraphic.Straight, 3, 1),
                new PuzzleArrowPiece(4, 4, ArrowGraphic.UpRight, 2, 0),
                new PuzzleArrowPiece(5, 4, ArrowGraphic.Straight, 2, 3)
            };
        }

        public static bool IsSolved(IReadOnlyList<PuzzleArrowPiece> pieces)
        {
            foreach (var piece in pieces)
            {
                if (!piece.IsCorrect) return false;
            }
            return true;
        }

        public static PointF GetPieceCenter(PuzzleArrowPiece piece)
        {
            float x = GridOriginX + piece.GridX * CellSize + CellSize / 2f;
            float y = GridOriginY + piece.GridY * CellSize + CellSize / 2f;
            return new PointF(x, y);
        }

        public static PuzzleArrowPiece HitTest(IReadOnlyList<PuzzleArrowPiece> pieces, PointF designPoint)
        {
            float half = ArrowDrawSize / 2f;
            foreach (var piece in pieces)
            {
                PointF center = GetPieceCenter(piece);
                if (designPoint.X >= center.X - half && designPoint.X <= center.X + half &&
                    designPoint.Y >= center.Y - half && designPoint.Y <= center.Y + half)
                {
                    return piece;
                }
            }
            return null;
        }
    }
}
