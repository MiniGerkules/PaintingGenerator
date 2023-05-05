using System.Windows.Media;

namespace PaintingsGenerator.StrokesLib.Colors {
    public interface IStrokeColor {
        bool IsTransparent { get; }

        bool IsEqual(Color color);
        Color ToColor();
    }
}
