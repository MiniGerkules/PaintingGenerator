using System.Windows.Media;
using PaintingsGenerator.StrokesLib.Colors;

namespace PaintingsGenerator.StrokesLib.ColorProducers {
    internal class RGBAProducer : IColorProducer {
        public IStrokeColor FromBgra32(byte blue, byte green, byte red, byte alpha) {
            return new RGBAColor(red, green, blue, alpha);
        }

        public IStrokeColor FromColor(Color color) {
            return new RGBAColor(color.R, color.G, color.B, color.A);
        }

        public IStrokeColor Update(IStrokeColor oldColor, IStrokeColor newColor) {
            var oldRGBA = (RGBAColor)oldColor;
            var newRGBA = (RGBAColor)newColor;

            return new RGBAColor(newRGBA.Red, newRGBA.Green, newRGBA.Blue, oldRGBA.Alpha);
        }
    }
}
