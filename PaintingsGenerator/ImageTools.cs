using System;

using PaintingsGenerator.Images;
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
    }
}
