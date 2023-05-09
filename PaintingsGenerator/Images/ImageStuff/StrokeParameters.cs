namespace PaintingsGenerator.Images.ImageStuff {
    public record class StrokeParameters(double Length, double Width, double Curvature) {
        private readonly double lengthToWidth = Length / Width;
        public double LengthToWidth => lengthToWidth;
    }
}
