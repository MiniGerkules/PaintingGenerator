using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using PaintingsGenerator.StrokesLib.Colors;
using PaintingsGenerator.Images.ImageStuff.Colors;
using PaintingsGenerator.StrokesLib.ColorProducers;

namespace PaintingsGenerator.StrokesLib {
    internal class RealStroke<ColorProducer> where ColorProducer : IColorProducer, new() {
        private static readonly ColorProducer colorProducer = new();
        private readonly IStrokeColor[,] pixels;

        private RealStroke(IStrokeColor[,] pixels) {
            this.pixels = pixels;
        }

        public static RealStroke<ColorProducer> Create(Uri pathToStroke) {
            var image = new BitmapImage(pathToStroke);
            if (image.Format != PixelFormats.Bgra32)
                throw new FormatException("ERROR! Image must be in Bgra32 format!");

            int height = image.PixelHeight, width = image.PixelWidth;
            int bytesPerPixel = (image.Format.BitsPerPixel + 7) / 8;
            var pixels = new IStrokeColor[height, width];

            byte[] vals = new byte[bytesPerPixel * width * height];
            image.CopyPixels(vals, bytesPerPixel * width, 0);

            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    int blockStart = y * bytesPerPixel * width + x * bytesPerPixel;
                    byte blue = vals[blockStart + 0];
                    byte green = vals[blockStart + 1];
                    byte red = vals[blockStart + 2];
                    byte alpha = vals[blockStart + 3];

                    pixels[y, x] = colorProducer.FromBgra32(blue, green, red, alpha);
                }
            }

            return new(pixels);
        }

        public void ChangeColor(RGBColor color) {
            var hsvaColor = colorProducer.FromBgra32(color.Blue, color.Green, color.Red, 0);

            for (int y = 0, endY = pixels.GetLength(0); y < endY; ++y) {
                for (int x = 0, endX = pixels.GetLength(1); x < endX; ++x)
                    pixels[y, x] = colorProducer.Update(pixels[y, x], hsvaColor);
            }
        }

        public BitmapSource ToBitmap() {
            var format = PixelFormats.Bgra32;
            var bytesPerPixel = (format.BitsPerPixel + 7) / 8;
            int height = pixels.GetLength(0), width = pixels.GetLength(1);
            int stride = width * bytesPerPixel;

            var vals = new byte[height * stride];
            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    var rgbColor = pixels[y, x].ToColor();
                    int blockStart = y*stride + x*bytesPerPixel;

                    vals[blockStart + 0] = rgbColor.B;
                    vals[blockStart + 1] = rgbColor.G;
                    vals[blockStart + 2] = rgbColor.R;
                    vals[blockStart + 3] = rgbColor.A;
                }
            }

            var bitmap = BitmapSource.Create(width, height, 96, 96, format, null, vals, stride);
            bitmap.Freeze();

            return bitmap;
        }
    }
}
