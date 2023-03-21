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
            var rgbPainting = RGBImage.CreateEmpty(toProcess.PixelWidth, toProcess.PixelHeight);
            var grayPainting = new GrayImage(rgbPainting);
            var template = new RGBImage(toProcess);

            uint height = 65, maxLength = 627;

            while (true) {
                var curDiff = await Task.Run(() => RGBImage.GetDifference(template, rgbPainting));
                var posWithMaxDiff = Task.Run(() => ImageTools.FindPosWithTheHighestDiff(curDiff, height));

                var gradient = Task.Run(() => Gradient.GetGradient(grayPainting));
                var strokePos = await Task.Run(async () =>
                    ImageTools.GetStroke(template, await gradient, await posWithMaxDiff, height, maxLength)
                );
                var rgbColor = await Task.Run(() => template.GetColor(strokePos, height));

                await Task.Run(() => rgbPainting.AddStroke(new(strokePos, rgbColor, height)));
                var newDiff = await Task.Run(() => RGBImage.GetDifference(template, rgbPainting));

                if (newDiff.SumDiff() >= curDiff.SumDiff()) {
                    rgbPainting.RemoveLastStroke();
                } else {
                    await Task.Run(() => grayPainting.AddStroke(new(strokePos, new(rgbColor), height)));
                    imageProcessorVM.Painting = rgbPainting.ToBitmap();
                    progressVM.CurProgress = GetProgress(niceDiff, newDiff.SumDiff());
                }

                if (curDiff.SumDiff() <= niceDiff) break;
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
