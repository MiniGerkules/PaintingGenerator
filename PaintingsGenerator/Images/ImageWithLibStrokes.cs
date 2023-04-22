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
            var strokeAngle = Math.Atan(approximation.K);

            var bounds = stroke.GetStrokeBounds();
            var (width, height) = (bounds.RightX - bounds.LeftX, bounds.UpY - bounds.DownY);
            var heightToWidth = (double)height / width;

            var libStroke = StrokeLibManager.GetLibStroke(heightToWidth);
            var strokeInImg = new ImageDrawing() {
                Rect = new(bounds.LeftX, bounds.DownY, width, height),
                ImageSource = libStroke,
            };
            lastStroke = new DrawingGroup() {
                Children = { strokeInImg },
                Transform = new RotateTransform(strokeAngle/Math.PI * 180),
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
                //drawingContext.PushTransform(new TranslateTransform(-drawing.Bounds.X, -drawing.Bounds.Y));
                drawingContext.DrawDrawing(freezedPainting);
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);
            bitmap.Freeze();

            return bitmap;
        }
    }
}
