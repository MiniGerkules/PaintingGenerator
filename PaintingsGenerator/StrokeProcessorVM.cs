using System.Windows.Media.Imaging;

namespace PaintingsGenerator {
    public class StrokeProcessorVM : NotifierWithImageProp {
        private BitmapSource? generatedStroke;
        public BitmapSource? GeneratedStroke {
            get => generatedStroke;
            set {
                generatedStroke = value;
                NotifyPropertyChangedIfNeed(nameof(GeneratedStroke), value);
            }
        }

        private BitmapSource? libStroke;
        public BitmapSource? LibStroke {
            get => libStroke;
            set {
                libStroke = value;
                NotifyPropertyChangedIfNeed(nameof(LibStroke), value);
            }
        }
    }
}