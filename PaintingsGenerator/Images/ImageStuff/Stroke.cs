namespace PaintingsGenerator.Images.ImageStuff {
    public class Stroke<ColorType> {
        public StrokePositions Positions { get; }
        public ColorType Color { get; }

        public Stroke(StrokePositions positions, ColorType color) {
            Positions = positions;
            Color = color;
        }
    }
}
