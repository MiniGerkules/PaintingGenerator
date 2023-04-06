using System;
using System.Linq;
using System.Collections.Generic;

using PaintingsGenerator.Colors;
using PaintingsGenerator.Images;
using PaintingsGenerator.MathStuff;
using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator {
    internal class ImageTools {
        public static Position FindPosWithTheHighestDiff(DifferenceOfImages diff, uint height) {
            uint startY = 2*height, startX = startY;
            var posWithMaxDiff = new Position((int)startX, (int)startY);
            var maxDiff = diff.GetDifference(posWithMaxDiff, height);

            for (uint y = startY, endY = (uint)(diff.Height - 2*height); y < endY; y += height) {
                for (uint x = startX, endX = (uint)(diff.Width - 2*height); x < endX; x += height) {
                    var curPos = new Position((int)x, (int)y);
                    var curDiff = diff.GetDifference(curPos, height);

                    if (curDiff > maxDiff) {
                        maxDiff = curDiff;
                        posWithMaxDiff = curPos;
                    }
                }
            }

            return posWithMaxDiff;
        }

        public static Position GetStrokeStart(DifferenceOfImages diff, uint height) {
            var rand = new Random();

            var possibleStarts = new List<KeyValuePair<Position, double>>();
            for (int i = 0; i < 20; ++i) {
                var start = new Position(rand.Next(0, diff.Width), rand.Next(0, diff.Height));
                var difference = diff.GetDifference(start, height);

                possibleStarts.Add(new(start, difference));
            }

            return possibleStarts.MaxBy((elem) => elem.Value).Key;
        }

        public static StrokePositions GetStroke(RGBImage image, Gradient gradient,
                                                Position start, uint height,
                                                uint maxLength, uint maxError = 4) {
            var positions = new StrokePositions { start };
            var prevColor = image.GetColor(positions[^1], height);
            var prevVec = gradient.GetPerpVector(positions[^1]);
            var step = height * 2 + 1;      // Step = width of brush

            for (uint i = 0, max = maxLength/step; i < max; ++i) {
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
