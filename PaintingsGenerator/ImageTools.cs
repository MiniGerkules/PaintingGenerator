using System;

using PaintingsGenerator.Colors;
using PaintingsGenerator.Images;
using PaintingsGenerator.MathStuff;
using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator {
    internal class ImageTools {
        public static Position FindPosWithTheHighestDiff(RGBImage a, RGBImage b, uint height) {
            if (a.Width != b.Width || a.Height != b.Height)
                throw new Exception("Images must be the same size!");

            var posWithMaxDiff = new Position(0, 0);
            var maxDiff = RGBImage.GetDifference(a, b, posWithMaxDiff, height);

            for (int y = 0; y < a.Height; ++y) {
                for (int x = 0; x < a.Width; ++x) {
                    var curPos = new Position(x, y);
                    var curDiff = RGBImage.GetDifference(a, b, curPos, height);

                    if (curDiff > maxDiff) {
                        maxDiff = curDiff;
                        posWithMaxDiff = curPos;
                    }
                }
            }

            return posWithMaxDiff;
        }

        public static StrokePositions GetStroke(RGBImage image, Gradient gradient,
                                                Position start, uint height,
                                                int maxError = 4) {
            var positions = new StrokePositions { start };
            var prevColor = image.GetColor(positions[^1], height);
            var prevVec = gradient.GetPerpVector(positions[^1]);
            var step = height * 2 + 1;      // Step = width of brush

            while (true) {
                prevVec = gradient.GetPerpVector(positions[^1], prevVec);
                prevVec.Normalize();
                if (prevVec.IsPoint()) break;

                var newPos = GetNewPosition(positions[^1], prevVec, step);
                if (!newPos.InBounds(0, 0, image.Width - 1, image.Height - 1)) break;

                var newColor = image.GetColor(newPos, height);
                var curError = RGBColor.Difference(prevColor, newColor);
                if (curError > maxError) break;

                positions.Add(newPos);
                prevColor = newColor;
            }

            return positions;
        }

        private static Position GetNewPosition(Position pos, Vector2D direction,
                                               uint step) {
            var newX = pos.X + direction.X*step;
            var newY = pos.Y + direction.Y*step;

            return new((int)newX, (int)newY);
        }
    }
}
