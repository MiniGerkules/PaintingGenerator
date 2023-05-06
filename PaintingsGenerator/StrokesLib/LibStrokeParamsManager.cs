using System;

using PaintingsGenerator.Images.ImageStuff;
using PaintingsGenerator.StrokesLib.ColorProducers;

namespace PaintingsGenerator.StrokesLib {
    internal static class LibStrokeParamsManager {
        public static StrokeParameters GetParameters(Uri pathToStroke) {
            var libStroke = LibStroke<RGBAProducer>.Create(pathToStroke);

            return new(
                (double)libStroke.Length / libStroke.Width,
                libStroke.CountCurvature()
            );
        }
    }
}
