using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

using PaintingsGenerator.Colors;
using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator.Images {
    internal class RGBImage : Image<RGBColor> {
        private record class StrokeRestorer {
            public StrokePositions Positions { get; }
            public List<RGBColor> OldColors { get; }

            public StrokeRestorer(StrokePositions positions, List<RGBColor> colors) {
                Positions = positions;
                OldColors = colors;
            }
        }

        public static readonly PixelFormat FORMAT = PixelFormats.Rgb24;
        public static readonly int BYTES_PER_PIXEL = (FORMAT.BitsPerPixel + 7) / 8;

        private StrokeRestorer? toRestor = null;
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

            lastStrokePositions = new();
            colorsToRecover = new(lastStrokePositions.Count);

            var positions = new StrokePositions();
            for (int i = 0; i < stroke.Positions.Count - 1; ++i) {
                var curPos = stroke.Positions[i];
                var nextPos = stroke.Positions[i + 1];

                positions.Add(GetPositions(curPos, nextPos, stroke.Height));
            }

            positions.MakeUnique();

            foreach (var pos in positions) {
                lastStrokePositions.Add(pos);
                colorsToRecover.Add(this[pos.Y, pos.X]);

                this[pos.Y, pos.X] = stroke.Color;
            }
        }

        private StrokePositions GetPositions(Position start, Position end, uint radius) {
            var bounds = GetBounds(start, end, radius);
            var k_norm = (double)(end.Y-start.Y) / (end.X-start.X);
            StrokePositions positions;

            if (double.IsInfinity(k_norm)) { // Vertical
                positions = GetPositionsAlongVertical(bounds, start, end, k_norm, radius);
            } else if (Math.Abs(k_norm) <= 1e-5) { // Horizontal
                positions = GetPositionsAlongHorizontal(bounds, start, end, k_norm, radius);
            } else { // With another angle
                positions = GetPositionsAlongLine(bounds, start, end, k_norm, radius);
            }

            positions.Add(GetRoundPart(bounds, start, radius));
            positions.Add(GetRoundPart(bounds, end, radius));

            return positions;
        }

        private static StrokePositions GetPositionsAlongVertical(Bounds bounds, Position start,
                                                                 Position end, double k,
                                                                 uint radius) {
            if (start.Y > end.Y) (start, end) = (end, start);

            var positions = new StrokePositions();
            var getPositions = (Bounds bounds, Position pos, uint radius) => {
                return GetPositionsSymmetrically(bounds, pos, radius, (int a) => a, (int a) => 0);
            };

            for (int y = 0, endY = end.Y - start.Y + 1; y < endY; ++y) {
                int curY = y + start.Y;
                int curX = (int)(y/k) + start.X;

                positions.Add(getPositions(bounds, new(curX, curY), radius));
            }

            return positions;
        }

        private static StrokePositions GetPositionsAlongHorizontal(Bounds bounds, Position start,
                                                                   Position end, double k,
                                                                   uint radius) {
            if (start.X > end.X) (start, end) = (end, start);

            var positions = new StrokePositions();
            var getPositions = (Bounds bounds, Position pos, uint radius) => {
                return GetPositionsSymmetrically(bounds, pos, radius, (int a) => 0, (int a) => a);
            };

            for (int x = 0, endX = end.X - start.X + 1; x < endX; ++x) {
                int curX = x + start.X;
                int curY = (int)(k*x) + start.Y;

                positions.Add(getPositions(bounds, new(curX, curY), radius));
            }

            return positions;
        }

        private static StrokePositions GetPositionsSymmetrically(Bounds bounds, Position pos,
                                                                 uint radius, Func<int, int> additionX,
                                                                 Func<int, int> additionY) {
            var positions = new StrokePositions { pos };

            for (int i = 1; i <= radius; ++i) {
                var posPositive = new Position(pos.X + additionX(i), pos.Y + additionY(i));
                var posNegative = new Position(pos.X - additionX(i), pos.Y - additionY(i));

                if (bounds.InBounds(posPositive)) positions.Add(posPositive);
                if (bounds.InBounds(posNegative)) positions.Add(posNegative);
            }

            return positions;
        }

        private static StrokePositions GetPositionsAlongLine(Bounds bounds, Position start,
                                                             Position end, double k,
                                                             uint radius) {
            if (start.X > end.X) (start, end) = (end, start);

            int biasX = (int)(radius * Math.Abs(Math.Sin(Math.Atan(k))));
            var positions = new StrokePositions();

            AddPositionsBetween(start, end, bounds, positions, k, biasX, radius);

            int stepBack = (int)(radius * Math.Sin(Math.Atan(k))) * (k < 0 ? -1 : 1);
            int stepDown = (int)(radius * Math.Cos(Math.Atan(k))) * (k < 0 ? -1 : 1);

            int startX = start.X - stepBack;
            int startY = start.Y + stepDown;
            int maxY = Math.Max(end.Y + stepDown, start.Y + stepDown);
            var limitY1Y2 = (int y1, int y2) => (y1, Math.Min(y2, maxY));
            AddEdgePositions(positions, bounds, 0, biasX + stepBack, startX, startY, limitY1Y2, k);

            startX = end.X + stepBack;
            startY = end.Y - stepDown;
            int minY = Math.Min(end.Y - stepDown, start.Y - stepDown);
            limitY1Y2 = (int y1, int y2) => (Math.Max(y1, minY), y2);
            AddEdgePositions(positions, bounds, -(biasX + stepBack), 0, startX, startY, limitY1Y2, k);

            return positions;
        }

        private static void AddPositionsBetween(Position start, Position end,
                                                Bounds bounds, StrokePositions positions,
                                                double k, int biasX, uint radius) {
            int halfOfLen = (int)(radius / Math.Sin(Math.PI/2 - Math.Atan(k)));
            var getY1Y2 = (int x) => {
                int yBias = (int)(k*x) + start.Y;
                int y1 = yBias - halfOfLen;
                int y2 = yBias + halfOfLen;

                return (y1, y2);
            };

            AddNewPositions(positions, bounds, biasX + 1, end.X - start.X - biasX - 1, start.X, getY1Y2);
        }

        private static void AddEdgePositions(StrokePositions positions, Bounds bounds,
                                             int startX, int endX, int biasX, int biasY,
                                             Func<int, int, (int, int)> limitY1Y2, double k) {
            double kPerp = -1 / k;

            var getY1Y2 = (int x) => {
                int y1 = (int)(k*x) + biasY;
                int y2 = (int)(kPerp*x) + biasY;
                if (y1 > y2) (y1, y2) = (y2, y1);

                (y1, y2) = limitY1Y2(y1, y2);
                return (y1, y2);
            };

            AddNewPositions(positions, bounds, startX, endX, biasX, getY1Y2);
        }

        private static void AddNewPositions(StrokePositions positions,
                                            Bounds bounds, int startX, int endX,
                                            int biasX, Func<int, (int, int)> getY1Y2) {
            for (int x = startX; x <= endX; ++x) {
                var (y1, y2) = getY1Y2(x);

                for (int y = y1; y <= y2; ++y) {
                    var curPos = new Position(biasX + x, y);
                    if (bounds.InBounds(curPos)) positions.Add(curPos);
                }
            }
        }

        public void RemoveLastStroke() {
            if (lastStrokePositions == null) return;

            for (int i = 0; i < lastStrokePositions.Count; ++i) {
                var pos = lastStrokePositions[i];
                this[pos.Y, pos.X] = colorsToRecover![i];
            }

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
                    UnpuckWithAdd(part[y, x], ref red, ref green, ref blue);
            }

            return new((byte)(red/part.Size), (byte)(green/part.Size),
                       (byte)(blue/part.Size));
        }

        public double GetColorError(Position pos, uint height, RGBColor avgColor) {
            var part = GetPart(pos, height);
            double error = 0.0;

            for (int y = 0; y < part.Height; ++y) {
                for (int x = 0; x < part.Width; ++x) {
                    error += Math.Pow(part[y, x].Red - avgColor.Red, 2);
                    error += Math.Pow(part[y, x].Green - avgColor.Green, 2);
                    error += Math.Pow(part[y, x].Blue - avgColor.Blue, 2);
                }
            }

            return error;
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

        public static DifferenceOfImages GetDifference(RGBImage a, RGBImage b) {
            var partA = new Proxy(a, new(0, 0), new(a.Width - 1, a.Height - 1));
            var partB = new Proxy(b, new(0, 0), new(b.Width - 1, b.Height - 1));

            return GetDifference(partA, partB);
        }

        private static DifferenceOfImages GetDifference(Proxy a, Proxy b) {
            if (a.Width != b.Width || a.Height != b.Height)
                throw new Exception("Images must be the same size!");

            var diff = new DifferenceOfImages(a.Width, a.Height);
            for (int y = 0; y < a.Height; ++y) {
                for (int x = 0; x < a.Width; ++x) {
                    ushort pixelDiff = 0;
                    pixelDiff += (ushort)Math.Abs(a[y, x].Red - b[y, x].Red);
                    pixelDiff += (ushort)Math.Abs(a[y, x].Green - b[y, x].Green);
                    pixelDiff += (ushort)Math.Abs(a[y, x].Blue - b[y, x].Blue);

                    diff[y, x] = pixelDiff;
                }
            }

            return diff;
        }

        private static StrokePositions GetRoundPart(Bounds bounds, Position pos, uint radius) {
            var positions = new StrokePositions();
            for (int x = (int)-radius; x <= radius; ++x) {
                var curX = pos.X + x;
                if (!bounds.XInBounds(curX)) continue;

                for (int y = (int)-radius; y <= radius; ++y) {
                    var curY = pos.Y + y;
                    if (!bounds.YInBounds(curY)) continue;

                    if (x*x + y*y <= radius*radius)
                        positions.Add(new Position(curX, curY));
                }
            }

            return positions;
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
