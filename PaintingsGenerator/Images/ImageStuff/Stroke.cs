using System;

namespace PaintingsGenerator.Images.ImageStuff {
    public class Stroke<ColorType> {
        public StrokePositions Positions { get; }
        public ColorType Color { get; }

        public Stroke(StrokePositions positions, ColorType color) {
            Positions = positions;
            Color = color;
        }

        public Bounds GetStrokeBounds() {
            int leftBound = int.MaxValue, rightBound = int.MinValue;
            int upBound = int.MinValue, downBound = int.MaxValue;

            foreach (var pos in Positions) {
                leftBound = Math.Min(leftBound, pos.Position.X - (int)pos.Radius);
                rightBound = Math.Max(rightBound, pos.Position.X + (int)pos.Radius);
                upBound = Math.Max(upBound, pos.Position.Y + (int)pos.Radius);
                downBound = Math.Min(downBound, pos.Position.Y - (int)pos.Radius);
            }

            return new(leftBound, rightBound, downBound, upBound);
        }
    }
}
