using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Windows.Media.Imaging;

using PaintingsGenerator.MathStuff;
using PaintingsGenerator.StrokesLib.Colors;
using PaintingsGenerator.StrokesLib.ColorProducers;

namespace PaintingsGenerator.StrokesLib {
    internal class LibStroke<ColorProducer> where ColorProducer : IColorProducer, new() {
        private static readonly Color skeletonLineColor = Color.FromArgb(255, 255, 0, 0);
        private static readonly ColorProducer colorProducer = new();

        private readonly IStrokeColor[,] pixels;

        public double Length { get; private set; } = 0;
        public double Width { get; private set; } = 0;
        public double ImageWidth => Math.Min(pixels.GetLength(0), pixels.GetLength(1));

        private LibStroke(IStrokeColor[,] pixels) {
            this.pixels = pixels;
            DetermineWidthHeight();
        }

        private LibStroke(IStrokeColor[,] pixels, double length, double width) {
            this.pixels = pixels;
            Length = length;
            Width = width;
        }

        public static LibStroke<ColorProducer> Create(Uri pathToStroke) {
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
                    byte blue = vals[blockStart + 0], green = vals[blockStart + 1];
                    byte red = vals[blockStart + 2], alpha = vals[blockStart + 3];

                    pixels[y, x] = colorProducer.FromBgra32(blue, green, red, alpha);
                }
            }

            return new(pixels);
        }

        public static LibStroke<ColorProducer> Copy<SourceColProducer>(LibStroke<SourceColProducer> stroke)
                where SourceColProducer : IColorProducer, new() {
            int height = stroke.pixels.GetLength(0), width = stroke.pixels.GetLength(1);
            var pixels = new IStrokeColor[height, width];

            if (typeof(ColorProducer) == typeof(SourceColProducer)) {
                Array.Copy(stroke.pixels, pixels, stroke.pixels.Length);
            } else {
                var producer = new SourceColProducer();
                for (int y = 0; y < height; ++y) {
                    for (int x = 0; x < width; ++x)
                        pixels[y, x] = producer.FromColor(stroke.pixels[y, x].ToColor());
                }
            }

            return new(pixels, stroke.Length, stroke.Width);
        }

        public void ChangeColor(IStrokeColor color) {
            var updatingColor = colorProducer.FromColor(color.ToColor());

            for (int y = 0, endY = pixels.GetLength(0); y < endY; ++y) {
                for (int x = 0, endX = pixels.GetLength(1); x < endX; ++x)
                    pixels[y, x] = colorProducer.Update(pixels[y, x], updatingColor);
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

        public double CountCurvature() {
            var positions = GetSkeletonPositions();
            return Approximator.GetQuadraticApproximation(positions.ToImmutableList()).GetCurvative();
        }

        private void DetermineWidthHeight() {
            var positions = GetSkeletonPositions();
            long xSum = 0, ySum = 0;
            for (int i = 1; i < positions.Count; ++i) {
                xSum += positions[i].X - positions[i - 1].X;
                ySum += positions[i].Y - positions[i - 1].Y;
            }
            Length = Math.Sqrt(xSum*xSum + ySum*ySum);

            var quadratic = Approximator.GetQuadraticApproximation(positions.ToImmutableList());
            var bounds = new Bounds(0, pixels.GetLength(1) - 1, 0, pixels.GetLength(0) - 1);

            List<double> widths = new();
            foreach (var paramVal in quadratic.Parameter) {
                var derativeX0 = quadratic.DerivativeAt(paramVal);
                double x0 = quadratic.CountX(paramVal), y0 = quadratic.CountY(paramVal);

                // y = (-1/f'(x0))*x + (1/f'(x0))*x0 + f(x0)
                var perpFunc = new LineFunc(-1/derativeX0, 1/derativeX0*x0 + y0);
                widths.Add(GetWidthInPoint(bounds, perpFunc, (int)x0, (int)y0));
            }

            Width = 0;
            widths.ForEach(width => Width += width / widths.Count);
        }

        private double GetWidthInPoint(Bounds bounds, LineFunc perp, int x0, int y0) {
            Position left, right;
            if (perp.IsVertical()) {
                left = GetTheEdgePosition(bounds, x0, y0, pos => pos.X, pos => pos.Y - 1);
                right = GetTheEdgePosition(bounds, x0, y0, pos => pos.X, pos => pos.Y + 1);
            } else {
                left = GetTheEdgePosition(bounds, x0, y0, pos => pos.X - 1, pos => (int)perp.CountY(pos.X - 1));
                right = GetTheEdgePosition(bounds, x0, y0, pos => pos.X + 1, pos => (int)perp.CountY(pos.X + 1));
            }

            return Math.Sqrt(Math.Pow(right.X - left.X, 2) + Math.Pow(right.Y - left.Y, 2));
        }

        private Position GetTheEdgePosition(Bounds bounds, int x0, int y0,
                                            Func<Position, int> xUpdater,
                                            Func<Position, int> yUpdater) {
            Position edge, curPos = new(x0, y0);

            do {
                edge = curPos;
                curPos = new(xUpdater(curPos), yUpdater(curPos));
            } while (bounds.InBounds(curPos) && !pixels[curPos.Y, curPos.X].IsTransparent);

            return edge;
        }

        private List<Position> GetSkeletonPositions() {
            var positions = new List<Position>();

            for (int y = 0, endY = pixels.GetLength(0); y < endY; ++y) {
                for (int x = 0, endX = pixels.GetLength(1); x < endX; ++x) {
                    if (pixels[y, x].IsEqual(skeletonLineColor))
                        positions.Add(new(x, y));
                }
            }

            return positions;
        }
    }
}
