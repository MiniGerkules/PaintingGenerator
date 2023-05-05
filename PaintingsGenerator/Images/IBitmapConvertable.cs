using System.Windows.Media.Imaging;

namespace PaintingsGenerator.Images {
    internal interface IBitmapConvertable {
        BitmapSource ToBitmap();
    }
}
