using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using PaintingsGenerator.Images;
using PaintingsGenerator.MathStuff;
using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator {
    internal class ImageProcessor {
        public readonly ImageProcessorVM imageProcessorVM;
        public readonly ProgressVM progressVM;

        public ImageProcessor() {
            imageProcessorVM = new(CreateEmptyBitmap());
            progressVM = new();
        }

        public async void Process(BitmapSource toProcess, Settings settings) {
            var painting = RGBImage.CreateEmpty(toProcess.PixelWidth, toProcess.PixelHeight);
            var template = new RGBImage(toProcess);
            var gradient = await Task.Run(() => Gradient.GetGradient(new(template)));
            var builder = new StrokeBuilder(template, gradient);

            var lastDiff = await Task.Run(() => RGBImage.GetDifference(template, painting));
            var diffToStop = lastDiff.ScaledDiff * settings.DiffWithTemplateToStopInPercent / 100;

            while (true) {
                var strokePos = await Task.Run(() => builder.GetStroke(settings, lastDiff));
                if (strokePos.GetLen() < strokePos.MaxWidht()*settings.RatioOfLenToWidthShortest)
                    continue;

                var rgbColor = await Task.Run(() => template.GetColor(strokePos));

                await Task.Run(() => painting.AddStroke(new(strokePos, rgbColor)));
                var newDiff = await Task.Run(() => RGBImage.GetDifference(template, painting));

                if (newDiff.SumDiff >= lastDiff.SumDiff) {
                    await Task.Run(() => painting.RemoveLastStroke());
                } else {
                    lastDiff = newDiff;
                    imageProcessorVM.Painting = painting.ToBitmap();
                    progressVM.CurProgress = GetProgress(diffToStop, lastDiff.ScaledDiff);
                }

                if (lastDiff.ScaledDiff <= diffToStop) break;
            }
        }

        private static uint GetProgress(double curDiff, double niceDiff) {
            // niceDiff == 100%
            // curDiff == x%

            return (uint)(curDiff * 100 / niceDiff);
        }

        private static BitmapSource CreateEmptyBitmap(int width = 100, int height = 100,
                                                      double dpiX = 96, double dpiY = 96,
                                                      BitmapPalette? palette = null) {
            int stride = width * RGBImage.BYTES_PER_PIXEL;
            byte[] pixels = Enumerable.Repeat(byte.MaxValue, height*stride).ToArray();

            return BitmapSource.Create(
                width, height, dpiX, dpiY,
                RGBImage.FORMAT, palette,
                pixels, stride
            );
        }
    }
}
