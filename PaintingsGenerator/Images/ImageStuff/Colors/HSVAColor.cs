using System;
using System.Windows.Media;

namespace PaintingsGenerator.Images.ImageStuff.Colors {
    internal record class HSVAColor(double Hue,         // From 0 to 1
                                    double Saturation,  // From 0 to 1
                                    double Value,       // From 0 to 1
                                    double Alpha) {     // From 0 to 1
        public static HSVAColor FromBgra32(byte blue, byte green, byte red, byte alpha) {
            double redFrom0To1 = (double)red / byte.MaxValue;
            double greenFrom0To1 = (double)green / byte.MaxValue;
            double blueFrom0To1 = (double)blue / byte.MaxValue;
            double aplpaFrom0To1 = (double)alpha / byte.MaxValue;

            var max = Math.Max(Math.Max(redFrom0To1, greenFrom0To1), blueFrom0To1);
            var min = Math.Min(Math.Min(redFrom0To1, greenFrom0To1), blueFrom0To1);

            double delta = max - min;
            double h;
            if (delta == 0)
                h = 0;
            else if (max == redFrom0To1)
                h = (60 * (greenFrom0To1 - blueFrom0To1) / delta + 360) % 360;
            else if (max == greenFrom0To1)
                h = 60 * (blueFrom0To1 - redFrom0To1) / delta + 120;
            else /* max == blueFrom0To1 */
                h = 60 * (redFrom0To1 - greenFrom0To1) / delta + 240;

            double s = max == 0 ? 0 : 1 - min/max;
            double v = max;

            return new(h, s, v, aplpaFrom0To1);
        }

        public Color ToRGB() {
            // https://en.wikipedia.org/wiki/HSL_and_HSV#HSV_to_RGB
            double hueInDegrees = Hue * 360;
            double C = Saturation * Value;
            double H = hueInDegrees / 60;
            double X = C * (1 - Math.Abs(H % 2 - 1));

            double r1, g1, b1;
            if (0 <= H && H < 1)
                (r1, g1, b1) = (C, X, 0);
            else if (1 <= H && H < 2)
                (r1, g1, b1) = (X, C, 0);
            else if (2 <= H && H < 3)
                (r1, g1, b1) = (0, C, X);
            else if (3 <= H && H < 4)
                (r1, g1, b1) = (0, X, C);
            else if (4 <= H && H < 5)
                (r1, g1, b1) = (X, 0, C);
            else /* 5 <= H && H < 6 */
                (r1, g1, b1) = (C, 0, X);

            double m = Value - C;
            byte red = (byte)((r1 + m) * 255);
            byte green = (byte)((g1 + m) * 255);
            byte blue = (byte)((b1 + m) * 255);

            return Color.FromArgb((byte)(Alpha*byte.MaxValue), red, green, blue);
        }
    }
}
