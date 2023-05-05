using System.Windows.Media;
using PaintingsGenerator.Images.ImageStuff.Colors;

namespace PaintingsGenerator.StrokesLib.Colors {
    internal record struct RGBAColor : IStrokeColor {
        private readonly RGBColor rgb;

        public byte Red => rgb.Red;
        public byte Green => rgb.Green;
        public byte Blue => rgb.Blue;
        public byte Alpha { get; init; }

        public bool IsTransparent => Alpha == 0;

        public RGBAColor(byte red, byte green, byte blue, byte alpha) {
            rgb = new(red, green, blue);
            Alpha = alpha;
        }

        public Color ToColor() => Color.FromArgb(Alpha, Red, Green, Blue);

        public bool IsEqual(Color color) {
            return Alpha == color.A && Red == color.R && Green == color.G && Blue == color.B;
        }
    }
}
