using System.Windows.Media.Imaging;
using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator.Images {
    internal interface IImage<PixelColor> {
        void AddStroke(Stroke<PixelColor> stroke);
        BitmapSource ToBitmap();
    }
}
