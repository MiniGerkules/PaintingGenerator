using System.Linq;
using System.Windows.Media.Imaging;

using PaintingsGenerator.Images;

namespace PaintingsGenerator {
    internal class ImageProcessor {
        public readonly ImageProcessorVM imageProcessorVM;
        public readonly ProgressVM progressVM;

        public ImageProcessor() {
            imageProcessorVM = new(CreateEmptyBitmap());
            progressVM = new();
        }

        public async void Process(BitmapSource imageToProcess) {
            var rgbPainting = RGBImage.CreateEmpty(imageToProcess.PixelWidth, imageToProcess.PixelHeight);
            var grayPainting = new GrayImage(rgbPainting);
            var template = new RGBImage(imageToProcess);

            uint height = 10;           /// !!! Выбрать нормальный способ получения высоты квадрата !!!
            double curDiff, niceDiff = 5;        /// !!! Выбрать время, когда перестать рисовать             !!!

            do {
                curDiff = RGBImage.GetDifference(template, rgbPainting);

                var posWithMaxDiff = ImageTools.FindPosWithTheHighestDiff(template, rgbPainting, height);
                var gradient = ImageTools.GetGradient(grayPainting);
                var strokePos = template.GetStroke(gradient, posWithMaxDiff, height);
                var rgbColor = template.GetColor(strokePos);

                rgbPainting.AddStroke(new(strokePos, rgbColor));
                var newDiff = RGBImage.GetDifference(template, rgbPainting);

                if (newDiff > curDiff) {
                    rgbPainting.RemoveLastStroke();
                } else {
                    imageProcessorVM.Bitmap = rgbPainting.ToBitmap();
                    grayPainting.AddStroke(new(strokePos, new(rgbColor)));
                }
            } while (curDiff > niceDiff);
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
