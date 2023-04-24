using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using PaintingsGenerator.Colors;
using PaintingsGenerator.MathStuff;
using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator.Images {
    internal class ImageWithLibStrokes : IImage<RGBColor> {
        private readonly int width;
        private readonly int height;

        private readonly DrawingGroup painting = new();
        private DrawingGroup freezedPainting = new();
        private DrawingGroup lastStroke = new();

        public ImageWithLibStrokes(int width, int height) {
            this.width = width;
            this.height = height;
        }

        public void AddStroke(Stroke<RGBColor> stroke) {
            var strokePositions = stroke.Positions.Select((elem) => elem.Position).ToList();
            var approximation = LinearApproximator.GetApproximation(strokePositions);

            var lenToWidth = stroke.Positions.Length / (2*stroke.Positions.AvgRadius);
            var libStroke = StrokeLibManager.GetLibStroke(lenToWidth);

            var bounds = stroke.GetStrokeBounds();
            double xLeft = bounds.LeftX, xRight = bounds.RightX, yLeft = bounds.DownY, yRight = bounds.UpY;

            if (Math.Abs(approximation.K) <= 1e-5) { // horizontal
                yLeft += 2 * stroke.Positions.AvgRadius;
                yRight += 2 * stroke.Positions.AvgRadius;
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

                double yBias = Math.Abs(Math.Sin(angleForBias)) * stroke.Positions.AvgRadius;
                double xBias = approximation.K < 0 ? Math.Cos(angleForBias) * stroke.Positions.AvgRadius : 0;

                xLeft += xBias;
                xRight += xBias;
                yLeft += yBias;
                yRight += yBias;
            }

            double length = Math.Sqrt(Math.Pow(bounds.RightX - bounds.LeftX, 2) + Math.Pow(bounds.DownY - bounds.UpY, 2));
            double strokeAngle = -90 + Math.Atan(approximation.K)/Math.PI * 180;
            if (double.IsNaN(strokeAngle)) strokeAngle = 0;

            var strokeInImg = new ImageDrawing() {
                Rect = new(xLeft, yLeft, 2*stroke.Positions.AvgRadius, length),
                ImageSource = libStroke,
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
