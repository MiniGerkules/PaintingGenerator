using System;
using System.Windows.Media;

using PaintingsGenerator.Images.ImageStuff;
using PaintingsGenerator.StrokesLib.ColorProducers;

namespace PaintingsGenerator.StrokesLib {
    internal static class LibStrokeParamsManager {
        private static Color lineColor = Color.FromArgb(255, 255, 0, 0);

        public static StrokeParameters GetParameters(Uri pathToStroke) {
            var libStroke = LibStroke<RGBAProducer>.Create(pathToStroke);

            var length = Math.Max(libStroke.Width, libStroke.Height);
            var width = Math.Min(libStroke.Width, libStroke.Height);

            return new((double)length / width, libStroke.CountCurvatureByLine(lineColor));
        }
    }
}
