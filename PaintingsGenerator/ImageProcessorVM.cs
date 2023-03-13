using System.Windows.Media.Imaging;

namespace PaintingsGenerator {
    public class ImageProcessorVM : NotifierOfPropertyChange {
        private BitmapSource bitmap;
        public BitmapSource Bitmap {
            get => bitmap;
            set {
                bitmap = value;
                NotifyPropertyChanged(nameof(Bitmap));
            }
        }

        public ImageProcessorVM(BitmapSource bitmap) {
            this.bitmap = bitmap;
        }
    }
}
