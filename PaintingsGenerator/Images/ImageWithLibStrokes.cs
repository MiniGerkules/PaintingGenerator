using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Immutable;

using PaintingsGenerator.MathStuff;
using PaintingsGenerator.StrokesLib;
using PaintingsGenerator.Images.ImageStuff;
using PaintingsGenerator.StrokesLib.ColorProducers;

namespace PaintingsGenerator.Images {
    internal class ImageWithLibStrokes : IImage {
        private readonly int width;
        private readonly int height;

        private readonly DrawingGroup painting = new();
        private DrawingGroup freezedPainting = new();
        private DrawingGroup lastStroke = new();

        public BitmapSource LastStroke => PackLastStrokeBitmap();

        private BitmapSource PackLastStrokeBitmap() {
            var group = new DrawingGroup() {
                Children = { lastStroke },
                Transform = new TranslateTransform(-lastStroke.Bounds.X, -lastStroke.Bounds.Y),
            };
            group.Freeze();

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen()) {
                drawingContext.DrawDrawing(group);
            }

            var width = (int)lastStroke.Bounds.Width + 1;
            var height = (int)lastStroke.Bounds.Height + 1;

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);
            bitmap.Freeze();

            return bitmap;
        }

        public ImageWithLibStrokes(int width, int height) {
            this.width = width;
            this.height = height;
        }

        public void AddStroke(Stroke stroke) {
            var strokePositions = stroke.PivotPositions.Select((elem) => elem.Position).ToImmutableList();
            var approximation = Approximator.GetLinearApproximation(strokePositions);

            var libStroke = StrokeLibManager.GetLibStroke<RGBAProducer>(stroke.GetParameters());
            libStroke.ChangeColor(stroke.Color);
            var halfLibStrokeWidth = stroke.PivotPositions.AvgRadius * libStroke.ImageWidth / libStroke.Width;

            var bounds = stroke.GetStrokeBounds();
            double xLeft = bounds.LeftX, xRight = bounds.RightX, yLeft = bounds.DownY, yRight = bounds.UpY;

            if (Math.Abs(approximation.K) <= 1e-5) { // horizontal
                yLeft += 2 * halfLibStrokeWidth;
                yRight += 2 * halfLibStrokeWidth;
            } else if (double.IsNormal(approximation.K)) {
                double angleForBias = Math.Atan(approximation.GetKForPerp());

                yLeft = approximation.CountY(bounds.LeftX);
                yRight = approximation.CountY(bounds.RightX);

                if (!bounds.YInBounds(yLeft)) {
                    yLeft = approximation.K < 0 ? bounds.UpY : bounds.DownY;
                    xLeft = approximation.CountX(yLeft);
                }
                if (!bounds.YInBounds(yRight)) {
                    yRight = approximation.K < 0 ? bounds.UpY : bounds.DownY;
                    xRight = approximation.CountX(yRight);
                }

                double yBias = Math.Abs(Math.Sin(angleForBias)) * halfLibStrokeWidth;
                double xBias = approximation.K < 0 ? Math.Cos(angleForBias) * halfLibStrokeWidth : 0;

                xLeft += xBias;
                xRight += xBias;
                yLeft += yBias;
                yRight += yBias;
            }

            double length = Math.Sqrt(Math.Pow(bounds.RightX - bounds.LeftX, 2) + Math.Pow(bounds.DownY - bounds.UpY, 2));
            double strokeAngle = -90 + Math.Atan(approximation.K)/Math.PI * 180;
            if (double.IsNaN(strokeAngle)) strokeAngle = 0;

            var strokeInImg = new ImageDrawing() {
                Rect = new(xLeft, yLeft, 2*halfLibStrokeWidth, length),
                ImageSource = libStroke.ToBitmap(),
            };
            lastStroke = new DrawingGroup() {
                Children = { strokeInImg },
                Transform = new RotateTransform(strokeAngle, xLeft, yLeft),
            };

            lastStroke.Freeze();
        }

        public void SaveStroke() {
            painting.Children.Add(lastStroke);
            freezedPainting = painting.Clone();
            freezedPainting.Freeze();
        }

        public BitmapSource ToBitmap() {
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen()) {
                drawingContext.DrawDrawing(freezedPainting);
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);
            bitmap.Freeze();

            return bitmap;
        }
    }
}
