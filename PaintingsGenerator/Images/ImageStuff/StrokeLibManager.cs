using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using System.Windows.Media.Imaging;

namespace PaintingsGenerator.Images.ImageStuff {
    internal class StrokeLibManager {
        private static readonly string pathToLib = @"./StrokesLib/";

        private static readonly Dictionary<double, string> strokesLib = new();
        private static double[] sortedKeys = Array.Empty<double>();

        public static void LoadStrokesLib() {
            strokesLib.Clear();
            if (!Directory.Exists(pathToLib)) throw new FileLoadException("Can't load library of strokes!");

            foreach (var file in Directory.EnumerateFiles(pathToLib)) {
                if (!file.EndsWith(".png")) continue;

                var trimmed = file[pathToLib.Length..^4];
                var sepIndex = trimmed.IndexOf('x');
                if (sepIndex == -1) continue;

                var width = int.Parse(trimmed[..sepIndex]);
                var height = int.Parse(trimmed[(sepIndex + 1)..]);

                strokesLib.Add((double)height / width, file);
            }

            if (strokesLib.Count == 0) throw new FileLoadException("There aren't any strokes files!");
            sortedKeys = strokesLib.Keys.ToArray();
            Array.Sort(sortedKeys);
        }

        public static BitmapImage GetLibStroke(double heightToWidth) {
            int keyIndex = sortedKeys.Length - 1;
            for (int i = 0; i < sortedKeys.Length; ++i) {
                if (sortedKeys[i] - heightToWidth > 0) {
                    keyIndex = i;
                    break;
                }
            }

            if (keyIndex > 0) {
                var diffLeft = Math.Abs(sortedKeys[keyIndex - 1] - heightToWidth);
                var diffRight = Math.Abs(sortedKeys[keyIndex] - heightToWidth);
                keyIndex = diffLeft < diffRight ? keyIndex - 1 : keyIndex;
            }

            return new(new Uri(strokesLib[sortedKeys[keyIndex]], UriKind.Relative));
        }
    }
}
