using System.Windows.Media;
using PaintingsGenerator.StrokesLib.Colors;

namespace PaintingsGenerator.Images.ImageStuff.Colors {
    public record struct GrayColor : IToDoubleConvertable, IStrokeColor {
        public byte Gray { get; }
        public double Value => Gray;

        public bool IsTransparent => false;

        public GrayColor(byte gray) {
            Gray = gray;
        }

        public GrayColor(byte red, byte green, byte blue) {
            Gray = (byte)(0.3*red + 0.59*green + 0.11*blue);
        }

        public GrayColor(RGBColor rgbColor) : this(rgbColor.Red, rgbColor.Green, rgbColor.Blue) {
        }

        public GrayColor(Color color) : this(color.R, color.G, color.B) {
        }

        public GrayColor(IStrokeColor color) : this(new RGBColor(color.ToColor())) {
        }

        public bool IsEqual(Color color) {
            var grayColor = new GrayColor(color);
            return Gray == grayColor.Gray;
        }

        public Color ToColor() => Color.FromRgb(Gray, Gray, Gray);
    }
}
