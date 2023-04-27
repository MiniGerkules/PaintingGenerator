using System;

namespace PaintingsGenerator.Images.ImageStuff.Colors {
    public record struct RGBColor(byte Red, byte Green, byte Blue) {
        public static uint Difference(RGBColor a, RGBColor b) {
            return (uint)Math.Abs(a.Red - b.Red) +
                   (uint)Math.Abs(a.Green - b.Green) +
                   (uint)Math.Abs(a.Blue - b.Blue);
        }
    }
}
