using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PaintingsGenerator {
    public class NotifierOfPropertyChange : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
