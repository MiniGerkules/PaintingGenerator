using System;
using System.Linq;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Windows.Media.Imaging;

using PaintingsGenerator.MathStuff;
using PaintingsGenerator.StrokesLib.Colors;
using PaintingsGenerator.StrokesLib.ColorProducers;

namespace PaintingsGenerator.StrokesLib {
    internal class LibStroke<ColorProducer> where ColorProducer : IColorProducer, new() {
        private static readonly ColorProducer colorProducer = new();
        private static readonly byte alphaTolerance = 1;

        public ImmutableList<Position> Skeleton { get; }

        private double? curvature = null;
        public double Curvature => curvature ??= Approximator.GetQuadraticApproximation(Skeleton).GetCurvative();

        private double? length = null;
        public double Length => length ??= GetLength();

        private double? width = null;
        public double Width => width ??= GetWidth();

        private readonly IStrokeColor[,] pixels;
        public double ImgHeight => pixels.GetLength(0);
        public double ImgWidth => pixels.GetLength(1);

        private readonly double[,] nx, ny, nz;

        private LibStroke(IStrokeColor[,] pixels, double[,] nx, double[,] ny, double[,] nz)
                : this(pixels, GetSkeleton(pixels), nx, ny, nz) {
        }

        private LibStroke(IStrokeColor[,] pixels, ImmutableList<Position> skeleton,
                          double[,] nx, double[,] ny, double[,] nz) {
            this.pixels = pixels;
            Skeleton = skeleton;

            this.nx = nx;
            this.ny = ny;
            this.nz = nz;
        }

        public static LibStroke<ColorProducer> Create(Uri pathToStroke, Uri normalsPath) {
            BitmapSource stroke = new BitmapImage(pathToStroke), normals = new BitmapImage(normalsPath);
            if (stroke.Format != PixelFormats.Bgr32 || normals.Format != PixelFormats.Bgr32)
                throw new FormatException("ERROR! Image must be in Bgra32 format!");
            if (stroke.PixelWidth != normals.PixelWidth || stroke.PixelHeight != normals.PixelHeight)
                throw new FormatException("ERROR! Images must be the same size!");

            int height = stroke.PixelHeight, width = stroke.PixelWidth;
            var pixels = new IStrokeColor[height, width];

            int strokeBytesPerPixel = (stroke.Format.BitsPerPixel + 7) / 8;
            byte[] alphaVals = new byte[strokeBytesPerPixel * width * height];
            stroke.CopyPixels(alphaVals, strokeBytesPerPixel * width, 0);

            int normalsBytesPerPixel = (normals.Format.BitsPerPixel + 7) / 8;
            byte[] normalVals = new byte[normalsBytesPerPixel * width * height];
            normals.CopyPixels(normalVals, normalsBytesPerPixel * width, 0);
            var nx = new double[height, width];
            var ny = new double[height, width];
            var nz = new double[height, width];

            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    int strokeBlockStart = y*strokeBytesPerPixel*width + x*strokeBytesPerPixel;
                    byte blue = alphaVals[strokeBlockStart], green = alphaVals[strokeBlockStart + 1];
                    byte red = alphaVals[strokeBlockStart + 2], alpha = Math.Max(Math.Max(blue, green), red);
                    alpha = alpha > alphaTolerance ? alpha : (byte)0;
                    pixels[y, x] = colorProducer.FromBgra32(blue, green, red, alpha);

                    int normalsBlockStart = y*normalsBytesPerPixel*width + x*normalsBytesPerPixel;
                    nx[y, x] = (double)normalVals[normalsBlockStart]/byte.MaxValue - 1;
                    ny[y, x] = (double)normalVals[normalsBlockStart + 1]/byte.MaxValue - 1;
                    nz[y, x] = (double)normalVals[normalsBlockStart + 2]/byte.MaxValue - 1;
                }
            }

            return new(pixels, nx, ny, nz);
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

            return new(pixels, stroke.Skeleton, stroke.nx, stroke.ny, stroke.nz);
        }

        public void ChangeColor(IStrokeColor color) {
            var updatingColor = colorProducer.FromColor(color.ToColor());

            for (int y = 0, endY = pixels.GetLength(0); y < endY; ++y) {
                for (int x = 0, endX = pixels.GetLength(1); x < endX; ++x)
                    pixels[y, x] = colorProducer.Update(pixels[y, x], updatingColor);
            }

            ShadingByPhong();
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

        private static ImmutableList<Position> GetSkeleton(IStrokeColor[,] pixels) {
            var allPositions = GetAllStrokePositions(pixels);
            var approximation = Approximator.GetQuadraticApproximation(allPositions);
            var skeleton = new List<Position>() {
                new((int)approximation.CountX(approximation.Parameter.First()),
                    (int)approximation.CountY(approximation.Parameter.First()))
            };

            foreach (var param in approximation.Parameter) {
                var newX = (int)approximation.CountX(param);
                var newY = (int)approximation.CountX(param);

                if (skeleton[^1].X != newX || skeleton[^1].Y != newY)
                    skeleton.Add(new(newX, newY));
            }

            return skeleton.ToImmutableList();
        }

        private static ImmutableList<Position> GetAllStrokePositions(IStrokeColor[,] pixels) {
            int height = pixels.GetLength(0), width = pixels.GetLength(1);
            List<Position> positions = new();

            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    if (!pixels[y, x].IsTransparent)
                        positions.Add(new(x, y));
                }
            }

            return positions.ToImmutableList();
        }

        private double GetLength() {
            long xSum = 0, ySum = 0;

            for (int i = 1; i < Skeleton.Count; ++i) {
                xSum += Skeleton[i].X - Skeleton[i - 1].X;
                ySum += Skeleton[i].Y - Skeleton[i - 1].Y;
            }

            return Math.Sqrt(xSum*xSum + ySum*ySum);
        }

        private double GetWidth() {
            var quadratic = Approximator.GetQuadraticApproximation(Skeleton);
            var bounds = new Bounds(0, pixels.GetLength(1) - 1, 0, pixels.GetLength(0) - 1);

            double width = double.NegativeInfinity;
            foreach (var paramVal in quadratic.Parameter) {
                var derativeX0 = quadratic.DerivativeAt(paramVal);
                double x0 = quadratic.CountX(paramVal), y0 = quadratic.CountY(paramVal);

                // y = (-1/f'(x0))*x + (1/f'(x0))*x0 + f(x0)
                var perpFunc = new LineFunc(-1/derativeX0, 1/derativeX0*x0 + y0);
                var curWidth = GetWidthInPoint(bounds, perpFunc, (int)x0, (int)y0);
                width = Math.Max(curWidth, width);
            }

            return width;
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

        private void ShadingByPhong() {
            int height = pixels.GetLength(0), width = pixels.GetLength(1);

            double ks = 0.1; // Коэффициент бликов
            double kd = 0.3; // Коэффициент диффузного света
            double ka = 0.7; // Коэффициент окружающего цвета
            double alpha = 20; // степень отражения
            Color glareColor = Color.FromRgb(255, 255, 255); // цвет падающего на изображения цвета, который видно в бликах

            var Lm = new Vector(new double[] { 0.5, 0.5, -3 }); // вектор направления света
            Lm.Norm();
            var V = new Vector(new double[] { 0, 0, -1 }); // вектор на наблюдателя

            for (int i = 0; i < height; ++i) {
                for (int j = 0; j < width; ++j) {
                    if (pixels[i, j].IsTransparent) {
                        // сделаем вектор нормали для удобства матлабовским вектором
                        var nvect = new Vector(new double[] { nx[i, j], ny[i, j], nz[i, j] });
                        var NdotL = Vector.ScalarProd(Lm, nvect); // умножим скалярно nvect на Lm
                        NdotL = Math.Min(Math.Max(NdotL, 0), 1); // оставим в рамках [0; 1]
                        var Rv = 2*NdotL*nvect - Lm; // найдем вектор Rv по формуле

                        // посчитаем каждый из R,G,B компонентов отдельно по модели Фонга
                        var curColor = pixels[i, j].ToColor();
                        var multiplier = ks*Math.Pow(Vector.ScalarProd(Rv, V), alpha);
                        var newRed = ka*curColor.R + kd*NdotL*curColor.R + multiplier*glareColor.R;
                        var newGreen = ka*curColor.G + kd*NdotL*curColor.G + multiplier*glareColor.G;
                        var newBlue = ka*curColor.B + kd*NdotL*curColor.B + multiplier*glareColor.B;

                        pixels[i, j] = colorProducer.FromBgra32((byte)newBlue, (byte)newGreen, (byte)newRed, curColor.A);
                    }
                }
            }
        }
    }
}
