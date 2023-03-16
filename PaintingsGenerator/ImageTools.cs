using System;

using PaintingsGenerator.Images;
using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator {
    internal class ImageTools {
        #region RGB
        public static Position FindPosWithTheHighestDiff(RGBImage a, RGBImage b, uint height) {
            if (a.Width != b.Width || a.Height != b.Height)
                throw new Exception("Images must be the same size!");

            var posWithMaxDiff = new Position(0, 0);
            var maxDiff = GetDifference(a, b, posWithMaxDiff, height);

            for (int y = 0; y < a.Height; ++y) {
                for (int x = 0; x < a.Width; ++x) {
                    var curPos = new Position(x, y);
                    var curDiff = GetDifference(a, b, curPos, height);

                    if (curDiff > maxDiff) {
                        maxDiff = curDiff;
                        posWithMaxDiff = curPos;
                    }
                }
            }

            return posWithMaxDiff;
        }
        #endregion

        #region Gray
        public static Gradient GetGradient(GrayImage grayImage) {
            double p1 = 0.183;
            var kernel_x = new double[3, 3] {
                { p1,           0,      -p1 },
                { 1 - 2*p1,     0,      2*p1 - 1 },
                { p1,           0,      -p1 },
            };

            var kernel_y = new double[kernel_x.GetLength(0), kernel_x.GetLength(1)];
            for (int i = 0; i < kernel_x.GetLength(0); ++i) {
                for (int j = 0; j < kernel_x.GetLength(1); ++j)
                    kernel_y[j, i] = kernel_x[i, j];
            }

            var dx = MakeConvolution(grayImage, kernel_x);
            var dy = MakeConvolution(grayImage, kernel_y);

            return new(dx, dy);
        }

        private static double[,] MakeConvolution(GrayImage image, double[,] kernel) {
            int kCenterY = kernel.GetLength(0) / 2;
            int kCenterX = kernel.GetLength(1) / 2;

            int imageHeight = image.Height;
            int imageWidth = image.Width;

            var res = new double[image.Height, image.Width];
            for (int img_y = 0; img_y < imageHeight; ++img_y) {
                for (int img_x = 0; img_x < imageWidth; ++img_x) {
                    for (int kern_y = 0; kern_y < kernel.GetLength(0); ++kern_y) {
                        for (int kern_x = 0; kern_x < kernel.GetLength(1); ++kern_x) {
                            int y = img_y + (kern_y - kCenterY);
                            int x = img_x + (kern_x - kCenterX);

                            if (y >= 0 && y < imageHeight && x >= 0 && x < imageWidth)
                                res[img_y, img_x] += image[y, x].Gray * kernel[kern_y, kern_x];
                        }
                    }
                }
            }

            return res;
        }
        #endregion
    }
}
