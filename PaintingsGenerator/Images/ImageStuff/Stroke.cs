using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Immutable;

using PaintingsGenerator.MathStuff;
using PaintingsGenerator.StrokesLib.Colors;

namespace PaintingsGenerator.Images.ImageStuff {
    public class Stroke : IBitmapConvertable {
        public StrokePositions PivotPositions { get; }
        public IStrokeColor Color { get; }

        private ImmutableHashSet<Position>? allPositions = null;
        public ImmutableHashSet<Position> AllPositions => allPositions ??= GetAllPositions();

        private ImmutableHashSet<Position> GetAllPositions() {
            var manager = new PositionManager();
            manager.StoreStrokePositions(PivotPositions);
            return manager.StoredPositions;
        }

        public Stroke(StrokePositions positions, IStrokeColor color) {
            PivotPositions = positions;
            Color = color;
        }

        public Bounds GetStrokeBounds() {
            int leftBound = int.MaxValue, rightBound = int.MinValue;
            int upBound = int.MinValue, downBound = int.MaxValue;

            foreach (var pos in PivotPositions) {
                leftBound = Math.Min(leftBound, pos.Position.X - (int)pos.Radius);
                rightBound = Math.Max(rightBound, pos.Position.X + (int)pos.Radius);
                upBound = Math.Max(upBound, pos.Position.Y + (int)pos.Radius);
                downBound = Math.Min(downBound, pos.Position.Y - (int)pos.Radius);
            }

            return new(leftBound, rightBound, downBound, upBound);
        }

        public StrokeParameters GetParameters() {
            var positions = PivotPositions.Select(elem => elem.Position).ToImmutableList();

            return new(
                (double)PivotPositions.Length / (2*PivotPositions.AvgRadius),
                Approximator.GetQuadraticApproximation(positions).Curvative
            );
        }

        public BitmapSource ToBitmap() {
            var bounds = GetStrokeBounds();
            var pixelFormat = PixelFormats.Bgra32;
            var bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;

            var height = bounds.UpY - bounds.DownY + 1;
            var width = bounds.RightX - bounds.LeftX + 1;
            var stride = width * bytesPerPixel;

            var vals = new byte[height * stride];
            foreach (var pos in AllPositions) {
                if (!bounds.InBounds(pos)) continue;

                var newPosX = pos.X - bounds.LeftX;
                var newPosY = pos.Y - bounds.DownY;

                var color = Color.ToColor();
                vals[newPosY*stride + newPosX*bytesPerPixel + 0] = color.B;
                vals[newPosY*stride + newPosX*bytesPerPixel + 1] = color.G;
                vals[newPosY*stride + newPosX*bytesPerPixel + 2] = color.R;
                vals[newPosY*stride + newPosX*bytesPerPixel + 3] = color.A;
            }

            var stroke = BitmapSource.Create(
                width, height, 96, 96, pixelFormat, null, vals, stride
            );
            stroke.Freeze();

            return stroke;
        }
    }
}
