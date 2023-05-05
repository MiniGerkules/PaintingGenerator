using System;
using System.Windows.Media;

using PaintingsGenerator.StrokesLib.Colors;

namespace PaintingsGenerator.Images.ImageStuff.Colors {
    public record struct RGBColor(byte Red, byte Green, byte Blue) : IStrokeColor {
        public RGBColor(Color color) : this(color.R, color.G, color.B) {
        }

        public bool IsTransparent => false;

        public bool IsEqual(Color color) => Red == color.R && Green == color.G && Blue == color.B;
        public Color ToColor() => Color.FromRgb(Red, Green, Blue);

        public static uint Difference(RGBColor a, RGBColor b) {
            return (uint)Math.Abs(a.Red - b.Red) +
                   (uint)Math.Abs(a.Green - b.Green) +
                   (uint)Math.Abs(a.Blue - b.Blue);
        }
    }
}
