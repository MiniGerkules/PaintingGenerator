namespace PaintingsGenerator.Images.ImageStuff {
    public record class StrokeParameters(double Length, double Width, double Curvature) {
        private readonly double widthToLength = Width / Length;
        public double WidthToLength => widthToLength;
    }
}
