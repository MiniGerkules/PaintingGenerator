using System.Windows.Media.Imaging;

namespace PaintingsGenerator {
    public class ImageProcessorVM : NotifierOfPropertyChange {
        private BitmapSource paintingWithoutLibStrokes;
        public BitmapSource PaintingWithoutLibStrokes {
            get => paintingWithoutLibStrokes;
            set {
                paintingWithoutLibStrokes = value;
                NotifyPropertyChanged(nameof(PaintingWithoutLibStrokes));
            }
        }

        private BitmapSource paintingWithLibStrokes;
        public BitmapSource PaintingWithLibStrokes {
            get => paintingWithLibStrokes;
            set {
                paintingWithLibStrokes = value;
                NotifyPropertyChanged(nameof(PaintingWithLibStrokes));
            }
        }

        public ImageProcessorVM(BitmapSource paintingWithoutLibStrokes,
                                BitmapSource paintingWithLibStrokes) {
            this.paintingWithoutLibStrokes = paintingWithoutLibStrokes;
            this.paintingWithLibStrokes = paintingWithLibStrokes;
        }
    }
}
