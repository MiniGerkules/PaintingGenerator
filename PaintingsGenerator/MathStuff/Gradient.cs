using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using PaintingsGenerator.Images;

namespace PaintingsGenerator.MathStuff {
    internal class Gradient {
        private static readonly double[,] kernel_x;
        private static readonly double[,] kernel_y;

        static Gradient() {
            double p1 = 0.183;
            kernel_x = new double[,] {
                {       p1,     0,          -p1 },
                { 1 - 2*p1,     0,     2*p1 - 1 },
                {       p1,     0,          -p1 },
            };

            int kernel_x_height = kernel_x.GetLength(0);
            int kernel_x_width = kernel_x.GetLength(1);
            kernel_y = new double[kernel_x_width, kernel_x_height];

            for (int y = 0; y < kernel_x_height; ++y) {
                for (int x = 0; x < kernel_x_width; ++x) {
                    kernel_x[y, x] *= 0.5;
                    kernel_y[x, y] = kernel_x[y, x];
                }
            }
        }

        private readonly double[,] dx;
        private readonly double[,] dy;

        public Gradient(double[,] dx, double[,] dy) {
            this.dx = dx;
            this.dy = dy;
        }

        public Vector2D GetPerpVector(Position position) {
            double grad_x = dx[position.Y, position.X];
            double grad_y = dy[position.Y, position.X];

            double perp_x = 1;
            double perp_y = -grad_x * perp_x / grad_y;

            return new(perp_x, perp_y);
        }

        public Vector2D GetPerpVector(Position position, Vector2D prevDir) {
            var newDir = GetPerpVector(position);

            if (newDir.X*prevDir.X + newDir.Y*prevDir.Y < 0)
                newDir.Reverse();
            return newDir;
        }

        public BitmapSource ToBitmap() {
            int height = dx.GetLength(0), width = dx.GetLength(1);
            int stride = width;
            var pixels = new byte[height * stride];

            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    var val = (int)(Math.Atan2(dy[y, x], dx[y, x])*byte.MaxValue);
                    val = Math.Max(Math.Min(val, byte.MaxValue), byte.MinValue);

                    pixels[y*width + x] = (byte)val;
                }
            }

            return BitmapSource.Create(
                width, height, 96, 96,
                PixelFormats.Gray8, null,
                pixels, stride
            );
        }

        #region StaticMethods
        public static Gradient GetGradient(GrayImage grayImage) {
            var dx = MakeConvolution(grayImage, kernel_x);
            var dy = MakeConvolution(grayImage, kernel_y);

            return new(dx, dy);
        }

        private static double[,] MakeConvolution(GrayImage image, double[,] kernel) {
            var kernel_height = kernel.GetLength(0);
            var kernel_width = kernel.GetLength(1);

            int kernel_center_y = kernel_height / 2;
            int kernel_center_x = kernel_width / 2;

            var res = new double[image.Height, image.Width];
            for (int img_y = 0; img_y < image.Height; ++img_y) {
                for (int img_x = 0; img_x < image.Width; ++img_x) {
                    for (int kern_y = 0; kern_y < kernel_height; ++kern_y) {
                        for (int kern_x = 0; kern_x < kernel_width; ++kern_x) {
                            int y = img_y + (kern_y - kernel_center_y);
                            int x = img_x + (kern_x - kernel_center_x);

                            if (y >= 0 && y < image.Height && x >= 0 && x < image.Width)
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
