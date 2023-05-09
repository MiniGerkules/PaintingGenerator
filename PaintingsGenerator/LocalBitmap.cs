using System.Windows.Media.Imaging;

namespace PaintingsGenerator {
    public record class LocalBitmap(string Path, BitmapSource Bitmap) {
        public bool CanFreeze => Bitmap.CanFreeze;
        public void Freeze() { Bitmap.Freeze(); }

        public static implicit operator BitmapSource(LocalBitmap b) => b.Bitmap;
    }
}
