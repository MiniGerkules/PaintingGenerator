using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

using PaintingsGenerator.Colors;
using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator.Images {
    internal class RGBImage : Image<RGBColor> {
        private record class StrokeRestorer {
            public List<Position> Positions { get; }
            public List<RGBColor> OldColors { get; }

            public StrokeRestorer(List<Position> positions, List<RGBColor> colors) {
                Positions = positions;
                OldColors = colors;
            }
        }

        private StrokeRestorer? toRestore = null;

        #region Constructors
        private RGBImage(int width, int height) : base(PixelFormats.Rgb24, new RGBColor[height, width]) {
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

        public override BitmapSource ToBitmap() {
            int stride = Width * BYTES_PER_PIXEL;
            var pixels = new byte[Height * stride];

            if (FORMAT == PixelFormats.Rgb24) {
                for (int y = 0; y < Height; ++y) {
                    for (int x = 0; x < Width; ++x) {
                        var curPtr = 3*(y*Width + x);

                        pixels[curPtr + 0] = this[y, x].Red;
                        pixels[curPtr + 1] = this[y, x].Green;
                        pixels[curPtr + 2] = this[y, x].Blue;
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
            if (stroke.Positions.Count == 0) return;

            var positionsToRecover = new List<Position>();
            var colorsToRecover = new List<RGBColor>();

            var positions = new PositionManager();
            for (int i = 0; i < stroke.Positions.Count - 1; ++i) {
                var curPos = stroke.Positions[i];
                var nextPos = stroke.Positions[i + 1];
                var bounds = GetBounds(curPos.Position, curPos.Radius, nextPos.Position, nextPos.Radius);

                positions.StoreStrokePositions(bounds, curPos, nextPos);
            }

            foreach (var pos in positions.StoredPositions) {
                positionsToRecover.Add(pos);
                colorsToRecover.Add(this[pos.Y, pos.X]);

                this[pos.Y, pos.X] = stroke.Color;
            }

            toRestore = new(positionsToRecover, colorsToRecover);
        }

        public void RemoveLastStroke() {
            if (toRestore == null) return;

            for (int i = 0; i < toRestore.Positions.Count; ++i) {
                var pos = toRestore.Positions[i];
                this[pos.Y, pos.X] = toRestore.OldColors[i];
            }

            toRestore = null;
        }

        public RGBColor GetColor(StrokePivot point) {
            ulong red = 0, green = 0, blue = 0;
            var part = GetCirclePart(point.Position, point.Radius);

            ulong partSize = 0;
            foreach (var elem in part) {
                ++partSize;
                UnpuckWithAdd(elem, ref red, ref green, ref blue);
            }

            return new((byte)(red/partSize), (byte)(green/partSize),
                       (byte)(blue/partSize));
        }

        public RGBColor GetColor(StrokePositions positions) {
            ulong red = 0, green = 0, blue = 0;
            ulong numPositions = (ulong)positions.Count;

            foreach (var pos in positions) {
                var colorInPos = GetColor(pos);
                UnpuckWithAdd(colorInPos, ref red, ref green, ref blue);
            }

            return new((byte)(red/numPositions), (byte)(green/numPositions),
                       (byte)(blue/numPositions));
        }

        public double GetColorError(StrokePivot point, RGBColor avgColor) {
            var part = GetCirclePart(point.Position, point.Radius);
            double error = 0.0;

            foreach (var elem in part) {
                error += Math.Pow(elem.Red - avgColor.Red, 2);
                error += Math.Pow(elem.Green - avgColor.Green, 2);
                error += Math.Pow(elem.Blue - avgColor.Blue, 2);
            }

            return error;
        }

        private BitmapSource ConverFormat(BitmapSource image) {
            var converter = new FormatConvertedBitmap();
            converter.BeginInit();
            converter.Source = image;
            converter.DestinationFormat = FORMAT;
            converter.EndInit();

            return converter;
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
            return new(pixelBytes[0], pixelBytes[1], pixelBytes[2]);
        }

        public static DifferenceOfImages GetDifference(RGBImage a, RGBImage b) {
            if (a.Width != b.Width || a.Height != b.Height)
                throw new Exception("Images must be the same size!");

            var diff = new DifferenceOfImages(a.Width, a.Height);
            for (int y = 0; y < a.Height; ++y) {
                for (int x = 0; x < a.Width; ++x) {
                    int pixelDiff = 0;
                    pixelDiff += Math.Abs(a[y, x].Red - b[y, x].Red);
                    pixelDiff += Math.Abs(a[y, x].Green - b[y, x].Green);
                    pixelDiff += Math.Abs(a[y, x].Blue - b[y, x].Blue);

                    diff[y, x] = (ushort)pixelDiff;
                }
            }

            return diff;
        }
        #endregion
        #endregion
    }
}
