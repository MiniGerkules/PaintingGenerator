namespace PaintingsGenerator.Images.ImageStuff {
    public class Stroke<ColorType> {
        public StrokePositions Positions { get; }
        public ColorType Color { get; }
        public uint Height { get; }

        public Stroke(StrokePositions positions, ColorType color, uint height) {
            Positions = positions;
            Color = color;
            Height = height;
        }
    }
}
