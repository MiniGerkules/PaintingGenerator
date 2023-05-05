using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;

namespace PaintingsGenerator {
    public class NotifierWithImageProp : NotifierOfPropertyChange {
        protected void NotifyPropertyChangedIfNeed([CallerMemberName] string propertyName = "",
                                                   BitmapSource? bitmap = null) {
            if (bitmap != null)
                NotifyPropertyChanged(propertyName);
        }
    }
}
