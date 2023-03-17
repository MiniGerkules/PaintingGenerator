using System.Windows.Media.Imaging;

namespace PaintingsGenerator {
    public class ImageProcessorVM : NotifierOfPropertyChange {
        private BitmapSource painting;
        public BitmapSource Painting {
            get => painting;
            set {
                painting = value;
                NotifyPropertyChanged(nameof(Painting));
            }
        }

        public ImageProcessorVM(BitmapSource painting) {
            this.painting = painting;
        }
    }
}
