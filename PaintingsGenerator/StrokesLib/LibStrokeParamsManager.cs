using PaintingsGenerator.Images.ImageStuff;
using PaintingsGenerator.StrokesLib.ColorProducers;

namespace PaintingsGenerator.StrokesLib {
    internal static class LibStrokeParamsManager {
        public static StrokeParameters GetParameters<ColorProducer>(LibStroke<ColorProducer> libStroke)
                where ColorProducer : IColorProducer, new() {
            return new(libStroke.Length, libStroke.Width, libStroke.Curvature);
        }
    }
}
