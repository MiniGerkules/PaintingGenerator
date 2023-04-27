using System;
using System.Linq;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using PaintingsGenerator.Images;
using PaintingsGenerator.MathStuff;
using PaintingsGenerator.Images.ImageStuff;
using PaintingsGenerator.Images.ImageStuff.Colors;

namespace PaintingsGenerator {
    internal class ImageProcessor {
        public readonly ImageProcessorVM imageProcessorVM;
        public readonly ProgressVM progressVM;

        public ImageProcessor() {
            var emptyBitmap = CreateEmptyBitmap(PixelFormats.Gray8);
            imageProcessorVM = new(emptyBitmap, emptyBitmap);
            progressVM = new();
        }

        public async void Process(BitmapSource toProcess, Settings settings) {
            var template = new RGBImage(toProcess);
            var gradient = await Task.Run(() => Gradient.GetGradient(new(template)));
            var builder = new StrokeBuilder(template, gradient);

            var painting = RGBImage.CreateEmpty(toProcess.PixelWidth, toProcess.PixelHeight);
            var libStrokesImg = new ImageWithLibStrokes(toProcess.PixelWidth, toProcess.PixelHeight);

            var lastDiff = await Task.Run(() => RGBImage.GetDifference(template, painting));
            var diffToStop = lastDiff.ScaledDiff * settings.DiffWithTemplateToStopInPercent / 100;
            var maxDiff = lastDiff.ScaledDiff;

            while (true) {
                var strokePos = await Task.Run(() => builder.GetStroke(settings, lastDiff));
                if (strokePos.Length < strokePos.MaxWidht()*settings.RatioOfLenToWidthShortest)
                    continue;

                var rgbColor = await Task.Run(() => template.GetColor(strokePos));
                var newStroke = new Stroke<RGBColor>(strokePos, rgbColor);
                await Task.Run(() => painting.AddStroke(newStroke));

                var newDiff = await Task.Run(() => RGBImage.GetDifference(template, painting));
                if (newDiff.SumDiff >= lastDiff.SumDiff) {
                    await Task.Run(() => painting.RemoveLastStroke());
                } else {
                    lastDiff = newDiff;
                    await Task.Run(() => libStrokesImg.AddStroke(newStroke));
                    libStrokesImg.SaveStroke();
                    await UpdateView(painting, libStrokesImg, maxDiff, lastDiff.ScaledDiff, diffToStop);
                }

                if (lastDiff.ScaledDiff <= diffToStop) break;
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

        private static BitmapSource CreateEmptyBitmap(PixelFormat format,
                                                      int width = 1, int height = 1,
                                                      double dpiX = 96, double dpiY = 96,
                                                      BitmapPalette? palette = null) {
            int stride = width * (format.BitsPerPixel+7) / 8;
            byte[] pixels = Enumerable.Repeat(byte.MaxValue, height*stride).ToArray();

            var bitmap = BitmapSource.Create(
                width, height, dpiX, dpiY, format, palette, pixels, stride
            );
            bitmap.Freeze();

            return bitmap;
        }
    }
}
