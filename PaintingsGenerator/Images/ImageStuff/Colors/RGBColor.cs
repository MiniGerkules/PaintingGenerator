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

        public static int Difference(RGBColor a, RGBColor b) {
            return Math.Abs(a.Red - b.Red) + Math.Abs(a.Green - b.Green) +
                   Math.Abs(a.Blue - b.Blue);
        }
    }
}
