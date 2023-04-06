using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using PaintingsGenerator.Images;
using PaintingsGenerator.MathStuff;

namespace PaintingsGenerator {
    internal class ImageProcessor {
        public readonly ImageProcessorVM imageProcessorVM;
        public readonly ProgressVM progressVM;

        public ImageProcessor() {
            imageProcessorVM = new(CreateEmptyBitmap());
            progressVM = new();
        }

        public async void Process(BitmapSource toProcess, double niceDiff = 5) {
            var painting = RGBImage.CreateEmpty(toProcess.PixelWidth, toProcess.PixelHeight);
            var template = new RGBImage(toProcess);
            var gradient = await Task.Run(() => Gradient.GetGradient(new(template)));

            uint height = 65, maxLength = 627;

            while (true) {
                var curDiff = await Task.Run(() => RGBImage.GetDifference(template, painting));
                var posWithMaxDiff = Task.Run(() => ImageTools.GetStrokeStart(curDiff, height));

                var strokePos = await Task.Run(async () =>
                    ImageTools.GetStroke(template, gradient, await posWithMaxDiff, height, maxLength)
                );
                var rgbColor = Task.Run(() => template.GetColor(strokePos, height));

                await Task.Run(async () => painting.AddStroke(new(strokePos, await rgbColor, height)));
                var newDiff = await Task.Run(() => RGBImage.GetDifference(template, painting));

                double newDiffVal = newDiff.SumDiff(), curDiffVal = curDiff.SumDiff();
                if (newDiffVal >= curDiffVal) {
                    painting.RemoveLastStroke();
                } else {
                    curDiffVal = newDiffVal;
                    imageProcessorVM.Painting = painting.ToBitmap();
                    progressVM.CurProgress = GetProgress(niceDiff, curDiffVal);
                }

                if (curDiffVal <= niceDiff) break;
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

        private static BitmapSource CreateEmptyBitmap(BitmapSource template) {
            return CreateEmptyBitmap(template.PixelWidth, template.PixelHeight,
                                     template.DpiX, template.DpiY, template.Palette);
        }
    }
}
