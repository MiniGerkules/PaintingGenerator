using System.Windows.Media.Imaging;

namespace PaintingsGenerator {
    public class ImageProcessorVM : NotifierWithImageProp {
        private BitmapSource? paintingWithoutLibStrokes;
        public BitmapSource? PaintingWithoutLibStrokes {
            get => paintingWithoutLibStrokes;
            set {
                paintingWithoutLibStrokes = value;
                NotifyPropertyChangedIfNeed(nameof(PaintingWithoutLibStrokes), value);
            }
        }

        private BitmapSource? paintingWithLibStrokes;
        public BitmapSource? PaintingWithLibStrokes {
            get => paintingWithLibStrokes;
            set {
                paintingWithLibStrokes = value;
                NotifyPropertyChangedIfNeed(nameof(PaintingWithLibStrokes), value);
            }
        }

        private BitmapSource? template;
        public BitmapSource? Template {
            get => template;
            set {
                template = value;
                NotifyPropertyChangedIfNeed(nameof(Template), value);
            }
        }
    }
}
