using System.Windows.Media;
using PaintingsGenerator.StrokesLib.Colors;

namespace PaintingsGenerator.StrokesLib.ColorProducers {
    internal interface IColorProducer {
        IStrokeColor FromColor(Color color);
        IStrokeColor FromBgra32(byte blue, byte green, byte red, byte alpha);
        IStrokeColor Update(IStrokeColor oldColor, IStrokeColor newColor);
    }
}
