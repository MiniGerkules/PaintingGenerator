using System.Windows.Controls;

namespace PaintingsGenerator.Pages {
    public partial class PictureGeneratingPage : UserControl, IPage {
        Grid IPage.Container => imageDisplayer;

        public PictureGeneratingPage(ImageProcessorVM imageProcessorVM) {
            InitializeComponent();
            imageDisplayer.DataContext = imageProcessorVM;
        }
    }
}
