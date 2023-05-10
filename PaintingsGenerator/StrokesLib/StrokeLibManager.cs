using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using PaintingsGenerator.Images.ImageStuff;
using PaintingsGenerator.StrokesLib.ColorProducers;

namespace PaintingsGenerator.StrokesLib {
    internal static class StrokeLibManager {
        private static readonly string pathToLib = @"./StrokesLib/";
        private static readonly string pathToDatabase = @"./StrokesLib/library.strokes";

        private static readonly Dictionary<StrokeParameters, LibStroke<RGBAProducer>> strokesLib = new();

        public static void LoadStrokesLib() {
            if (File.Exists(pathToDatabase)) {
                LoadDatabaseFromFile(pathToDatabase);
            } else {
                CreateDatabaseFromSources(pathToLib);
            }
        }

        public static LibStroke<ColorProducer> GetLibStroke<ColorProducer>(StrokeParameters toDraw)
                    where ColorProducer : IColorProducer, new() {
            var allStrokeParams = strokesLib.Keys;
            var best = allStrokeParams.First();
            var bestDiff = GetDiff(best, toDraw);

            foreach (var strokeParams in allStrokeParams.Skip(1)) {
                double curDiff = GetDiff(strokeParams, toDraw);

                if (curDiff < bestDiff) {
                    best = strokeParams;
                    bestDiff = curDiff;
                }
            }

            return LibStroke<ColorProducer>.Copy(strokesLib[best]);
        }

        private static void LoadDatabaseFromFile(string pathToDatabase) {
            throw new NotImplementedException();
        }

        private static void CreateDatabaseFromSources(string pathToLib) {
            strokesLib.Clear();
            if (!Directory.Exists(pathToLib)) throw new FileLoadException("Can't load library of strokes!");

            foreach (var file in Directory.EnumerateFiles(pathToLib)) {
                if (!file.EndsWith(".jpg")) continue;

                var fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName.EndsWith("n")) continue;
                var normals = file.Replace(fileName, fileName + "n");

                var libStroke = LibStroke<RGBAProducer>.Create(new(file, UriKind.Relative), new(normals, UriKind.Relative));
                var parameters = LibStrokeParamsManager.GetParameters(libStroke);

                strokesLib.Add(parameters, libStroke);
            }

            if (strokesLib.Count == 0) throw new FileLoadException("There aren't any strokes files!");
        }

        private static double GetDiff(StrokeParameters params1, StrokeParameters params2) {
            double diff = 0;

            diff += Math.Pow(params1.WidthToLength - params2.WidthToLength, 2);
            diff += Math.Pow(params1.Curvature - params2.Curvature, 2);

            return diff;
        }
    }
}
