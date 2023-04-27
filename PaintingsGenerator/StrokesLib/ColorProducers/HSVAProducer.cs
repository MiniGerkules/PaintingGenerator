using System.Windows.Media;
using PaintingsGenerator.StrokesLib.Colors;

namespace PaintingsGenerator.StrokesLib.ColorProducers {
    internal class HSVAProducer : IColorProducer {
        IStrokeColor IColorProducer.FromColor(Color color) {
            return HSVAColor.FromBgra32(color.B, color.G, color.R, color.A);
        }

        IStrokeColor IColorProducer.FromBgra32(byte blue, byte green, byte red, byte alpha) {
            return HSVAColor.FromBgra32(blue, green, red, alpha);
        }

        IStrokeColor IColorProducer.Update(IStrokeColor oldColor, IStrokeColor newColor) {
            var oldHSVA = (HSVAColor)oldColor;
            var newHSVA = (HSVAColor)newColor;

            return new HSVAColor(newHSVA.Hue, newHSVA.Saturation, newHSVA.Value, oldHSVA.Alpha);
        }
    }
}
