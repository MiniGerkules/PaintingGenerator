using System.Windows;
using System.Windows.Controls;

namespace PaintingsGenerator.Pages {
    internal interface IPage {
        protected Grid Container { get; }

        void SetActive() {
            Container.Visibility = Visibility.Visible;
        }

        void SetInactive() {
            Container.Visibility = Visibility.Collapsed;
        }
    }
}
