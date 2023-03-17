using System;

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

        public static uint Difference(RGBColor a, RGBColor b) {
            return (uint)Math.Abs(a.Red - b.Red) +
                   (uint)Math.Abs(a.Green - b.Green) +
                   (uint)Math.Abs(a.Blue - b.Blue);
        }
    }
}
