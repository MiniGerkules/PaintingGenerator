namespace PaintingsGenerator.Images.ImageStuff {
    public class Stroke<ColorType> {
        public StrokePositions Positions { get; }
        public ColorType Color { get; }
        public uint Radius { get; }

        public Stroke(StrokePositions positions, ColorType color, uint radius) {
            Positions = positions;
            Color = color;
            Radius = radius;
        }
    }
}
