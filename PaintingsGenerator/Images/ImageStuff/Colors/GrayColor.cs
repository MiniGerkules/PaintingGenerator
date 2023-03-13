namespace PaintingsGenerator.Colors {
    internal struct GrayColor {
        public byte Gray { get; }

        public GrayColor(byte gray) {
            Gray = gray;
        }

        public GrayColor(RGBColor rgbColor) {
            Gray = (byte)(0.3*rgbColor.Red + 0.59*rgbColor.Green + 0.11*rgbColor.Blue);
        }
    }
}
