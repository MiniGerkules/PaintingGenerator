using System;

using PaintingsGenerator.Images.ImageStuff;
using PaintingsGenerator.StrokesLib.ColorProducers;

namespace PaintingsGenerator.StrokesLib {
    internal static class LibStrokeParamsManager {
        public static StrokeParameters GetParameters(Uri pathToStroke) {
            var libStroke = LibStroke<RGBAProducer>.Create(pathToStroke);

            var length = Math.Max(libStroke.Width, libStroke.Height);
            var width = Math.Min(libStroke.Width, libStroke.Height);

            return new((double)length / width, libStroke.CountCurvature());
        }
    }
}
