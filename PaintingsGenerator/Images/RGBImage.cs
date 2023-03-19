using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

using PaintingsGenerator.Colors;
using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator.Images {
    internal class RGBImage : Image<RGBColor> {
        public static readonly PixelFormat FORMAT = PixelFormats.Rgb24;
        public static readonly int BYTES_PER_PIXEL = (FORMAT.BitsPerPixel + 7) / 8;

        private StrokePositions? lastStrokePositions = null;
        private List<RGBColor>? colorsToRecover = null;

        #region Constructors
        private RGBImage(int width, int height) : base(new RGBColor[height, width]) {
        }

        public RGBImage(BitmapSource imageToHandle)
                : this(imageToHandle.PixelWidth, imageToHandle.PixelHeight) {
            if (imageToHandle.Format != FORMAT)
                imageToHandle = ConverFormat(imageToHandle);

            byte[] vals = new byte[BYTES_PER_PIXEL * Width * Height];
            imageToHandle.CopyPixels(vals, BYTES_PER_PIXEL * Width, 0);

            for (int i = 0; i < Height; ++i) {
                for (int j = 0; j < Width; ++j) {
                    int blockStart = i*BYTES_PER_PIXEL*Width + j*BYTES_PER_PIXEL;
                    this[i, j] = GetPixel(vals[blockStart..(blockStart + BYTES_PER_PIXEL)]);
                }
            }
        }
        #endregion

        public BitmapSource ToBitmap() {
            int stride = Width * BYTES_PER_PIXEL;
            var pixels = new byte[Height * stride];

            if (FORMAT == PixelFormats.Rgb24) {
                for (int y = 0; y < Height; ++y) {
                    for (int x = 0; x < Width; x += 3) {
                        pixels[y*Width + 3*x + 0] = this[y, x].Red;
                        pixels[y*Width + 3*x + 1] = this[y, x].Green;
                        pixels[y*Width + 3*x + 2] = this[y, x].Blue;
                    }
                }
            } else {
                throw new Exception("Unsupported pixel encoding format!");
            }

            return BitmapSource.Create(
                Width, Height, 96, 96,
                FORMAT, null,
                pixels, stride
            );
        }

        public override void AddStroke(Stroke<RGBColor> stroke) {
            lastStrokePositions = stroke.Positions;
            colorsToRecover = new(lastStrokePositions.Count);

            foreach (var pos in stroke.Positions) {
                colorsToRecover.Add(this[pos.Y, pos.X]);
                this[pos.Y, pos.X] = new(stroke.Color.Red, stroke.Color.Green, stroke.Color.Blue);
            }
        }

        public void RemoveLastStroke() {
            if (lastStrokePositions == null) return;

            int curColor = 0;
            foreach (var pos in lastStrokePositions) {
                this[pos.Y, pos.X] = colorsToRecover![curColor++];
            }

            lastStrokePositions = null;
            colorsToRecover!.Clear();
        }

        public RGBColor GetColor(StrokePositions positions, uint height) {
            ulong red = 0, green = 0, blue = 0;
            ulong numPositions = (ulong)positions.Count;

            foreach (var pos in positions) {
                var colorInPos = GetColor(pos, height);
                UnpuckWithAdd(colorInPos, ref red, ref green, ref blue);
            }

            return new((byte)(red/numPositions), (byte)(green/numPositions),
                       (byte)(blue/numPositions));
        }

        public RGBColor GetColor(Position pos, uint height) {
            ulong red = 0, green = 0, blue = 0;
            var part = GetPart(pos, height);

            for (int y = 0; y < part.Height; ++y) {
                for (int x = 0; x < part.Width; ++x)
                    UnpuckWithAdd(part[pos.Y, pos.X], ref red, ref green, ref blue);
            }

            return new((byte)(red/part.Size), (byte)(green/part.Size),
                       (byte)(blue/part.Size));
        }

        private static void UnpuckWithAdd(RGBColor color, ref ulong red,
                                          ref ulong green, ref ulong blue) {
            red += color.Red;
            green += color.Green;
            blue += color.Blue;
        }

        #region StaticMethods
        public static RGBImage CreateEmpty(int width, int height) {
            var newImage = new RGBImage(width, height);

            for (int i = 0; i < height; ++i) {
                for (int j = 0; j < width; ++j)
                    newImage[i, j] = new RGBColor(byte.MaxValue, byte.MaxValue, byte.MaxValue);
            }

            return newImage;
        }

        #region Getters
        private static RGBColor GetPixel(byte[] pixelBytes) {
            if (FORMAT == PixelFormats.Rgb24)
                return new(pixelBytes[0], pixelBytes[1], pixelBytes[2]);
            else
                throw new Exception("Unsupported pixel encoding format!");
        }

        public static double GetDifference(RGBImage a, RGBImage b) {
            var partA = new Proxy(a, new(0, 0), new(a.Width - 1, a.Height - 1));
            var partB = new Proxy(b, new(0, 0), new(b.Width - 1, b.Height - 1)); ;

            return GetDifference(partA, partB);
        }

        private static double GetDifference(Proxy a, Proxy b) {
            if (a.Width != b.Width || a.Height != b.Height)
                throw new Exception("Images must be the same size!");

            ulong diff = 0;
            for (int y = 0; y < a.Height; ++y) {
                for (int x = 0; x < a.Width; ++x) {
                    diff += (ulong)Math.Abs(a[y, x].Red - b[y, x].Red);
                    diff += (ulong)Math.Abs(a[y, x].Green - b[y, x].Green);
                    diff += (ulong)Math.Abs(a[y, x].Blue - b[y, x].Blue);
                }
            }

            return (double)diff / (a.Height * a.Width);
        }
        #endregion

        private static BitmapSource ConverFormat(BitmapSource image) {
            var converter = new FormatConvertedBitmap();
            converter.BeginInit();
            converter.Source = image;
            converter.DestinationFormat = FORMAT;
            converter.EndInit();

            return converter;
        }
        #endregion
    }
}
