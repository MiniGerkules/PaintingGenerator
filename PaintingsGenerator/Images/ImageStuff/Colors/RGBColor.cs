namespace PaintingsGenerator.Colors {
    public struct RGBColor {
        public byte Red { get; }
        public byte Green { get; }
        public byte Blue { get; }

        public RGBColor(byte red, byte green, byte blue) {
            Red = red;
            Green = green;
            Blue = blue;
        }
    }
}
