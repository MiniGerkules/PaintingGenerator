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

        private static Dictionary<StrokeParameters, string> strokesLib = new();
        private static readonly PropertyInfo[] strokeParametersProps = typeof(StrokeParameters).GetProperties();

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
                double diff = GetDiff(strokeParams, toDraw);

                if (diff*diff < bestDiff*bestDiff) {
                    best = strokeParams;
                    bestDiff = diff;
                }
            }

            return LibStroke<ColorProducer>.Create(new(strokesLib[best], UriKind.Relative));
        }

        private static void LoadDatabaseFromFile(string pathToDatabase) {
            throw new NotImplementedException();
        }

        private static void CreateDatabaseFromSources(string pathToLib) {
            strokesLib = new();
            if (!Directory.Exists(pathToLib)) throw new FileLoadException("Can't load library of strokes!");

            foreach (var file in Directory.EnumerateFiles(pathToLib)) {
                if (!file.EndsWith(".png")) continue;

                var parameters = LibStrokeParamsManager.GetParameters(new(file, UriKind.Relative));
                strokesLib.Add(parameters, file);
            }

            if (strokesLib.Count == 0) throw new FileLoadException("There aren't any strokes files!");
        }

        private static double GetDiff(StrokeParameters params1, StrokeParameters params2) {
            double diff = 0;

            foreach (var prop in strokeParametersProps) {
                var val1 = prop.GetValue(params1);
                var val2 = prop.GetValue(params2);
                if (val1 == null || val2 == null) continue;

                diff += (double)val1 - (double)val2;
            }

            return diff;
        }
    }
}
