using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Windows.Media.Imaging;

using PaintingsGenerator.Images;
using PaintingsGenerator.MathStuff;
using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator {
    internal class ImageProcessor {
        private readonly List<ProcessAction> actions;
        private readonly List<bool> eventsFlags;

        public readonly ImageProcessorVM imageProcessorVM = new();
        public readonly StrokeProcessorVM strokeProcessorVM = new();
        public readonly ProgressVM progressVM = new();
        public readonly ActionsVM actionsVM;

        public readonly MetadataSaver saver = new();

        public ImageProcessor() {
            eventsFlags = new List<bool>() {
                false, // Terminate
                false, // Run
                false, // Step
            };
            actions = new() {
                new("Run", () => eventsFlags[(int)Event.Run] = true),
                new("Step", () => eventsFlags[(int)Event.Step] = true),
            };

            actionsVM = new(actions.ToImmutableList());
        }

        public async void Process(LocalBitmap toProcess, Settings settings) {
            var source = toProcess.Bitmap;

            imageProcessorVM.Template = source;
            for (int i = 0; i < eventsFlags.Count; ++i) eventsFlags[i] = false;
            eventsFlags[(int)Event.Run] = true;

            var template = new RGBImage(source);
            var gradient = await Task.Run(() => Gradient.GetGradient(new(template)));
            var builder = new StrokeBuilder(template, gradient);

            var painting = RGBImage.CreateEmpty(source.PixelWidth, source.PixelHeight);
            var libStrokesImg = new ImageWithLibStrokes(source.PixelWidth, source.PixelHeight);

            var lastDiff = await Task.Run(() => RGBImage.GetDifference(template, painting));
            var diffToStop = lastDiff.ScaledDiff * settings.DiffWithTemplateToStopInPercent / 100;
            var maxDiff = lastDiff.ScaledDiff;

            while (true) {
                var strokePos = await Task.Run(() => builder.GetStroke(settings, lastDiff));
                if (strokePos.Length < strokePos.MaxWidht()*settings.RatioOfLenToWidthShortest)
                    continue;

                var rgbColor = await Task.Run(() => template.GetColor(strokePos));
                var newStroke = new Stroke(strokePos, rgbColor);
                await Task.Run(() => painting.AddStroke(newStroke));

                var newDiff = await Task.Run(() => RGBImage.GetDifference(template, painting));
                if (newDiff.SumDiff >= lastDiff.SumDiff) {
                    await Task.Run(() => painting.RemoveLastStroke());
                } else {
                    lastDiff = newDiff;
                    await Task.Run(() => libStrokesImg.AddStroke(newStroke));
                    libStrokesImg.SaveStroke();

                    strokeProcessorVM.GeneratedStroke = newStroke.ToBitmap();
                    strokeProcessorVM.LibStroke = libStrokesImg.LastStroke;
                    await UpdateView(painting, libStrokesImg, maxDiff, lastDiff.ScaledDiff, diffToStop);
                }

                await WaitForEvent();
                if (eventsFlags[(int)Event.Terminate]) {
                    return;
                } else if (eventsFlags[(int)Event.Step]) {
                    eventsFlags[(int)Event.Step] = false;
                    eventsFlags[(int)Event.Run] = false;
                }

                if (lastDiff.ScaledDiff <= diffToStop) break;
            }
        }

        private async Task WaitForEvent() {
            while (!eventsFlags.Any(elem => elem)) {
                await Task.Run(() => Thread.Sleep(100));
            }
        }

        private async Task UpdateView(RGBImage paintingWithoutLib,
                                      ImageWithLibStrokes paintingWithLib,
                                      double maxDiff, double curDiff, double diffToStop) {
            var paintWithoutLib = Task.Run(() => paintingWithoutLib.ToBitmap());
            var paintWithLib = Task.Run(() => paintingWithLib.ToBitmap());

            progressVM.CurProgress = GetProgress(maxDiff, curDiff, diffToStop);

            imageProcessorVM.PaintingWithoutLibStrokes = await paintWithoutLib;
            imageProcessorVM.PaintingWithLibStrokes = await paintWithLib;
        }

        private static uint GetProgress(double maxDiff, double curDiff, double diffToStop) {
            var maxDelta = maxDiff - diffToStop;
            var deltaDiff = curDiff - diffToStop;

            // deltaDiff == x%
            // maxDelta == 100%
            var deltaPercent = deltaDiff * 100 / maxDelta;

            // Range = [0, maxDelta], 0 == 100%, maxDelta = 0%
            var realPercent = 100 - deltaPercent;

            return Math.Min((uint)realPercent, 100);
        }
    }
}
