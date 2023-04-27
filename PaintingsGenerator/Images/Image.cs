using System.Windows.Media;
using System.Windows.Media.Imaging;

using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator.Images {
    public abstract class Image<PixelColor> : Container<PixelColor>, IImage<PixelColor> {
        protected readonly PixelFormat FORMAT;
        protected readonly int BYTES_PER_PIXEL;

        public Image(PixelFormat format, PixelColor[,] pixels) : base(pixels) {
            FORMAT = format;
            BYTES_PER_PIXEL = (format.BitsPerPixel + 7) / 8;
        }

        public abstract void AddStroke(Stroke<PixelColor> stroke);
        public abstract BitmapSource ToBitmap();
    }
}
