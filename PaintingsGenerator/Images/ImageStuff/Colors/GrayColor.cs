namespace PaintingsGenerator.Colors {
    public record struct GrayColor : IToDoubleConvertable {
        public byte Gray { get; }
        public double Value => Gray;

        public GrayColor(byte gray) {
            Gray = gray;
        }

        public GrayColor(RGBColor rgbColor) {
            Gray = (byte)(0.3*rgbColor.Red + 0.59*rgbColor.Green + 0.11*rgbColor.Blue);
        }
    }
}
