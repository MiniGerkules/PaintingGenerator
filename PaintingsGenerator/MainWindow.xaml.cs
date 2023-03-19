using System.Windows;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

namespace PaintingsGenerator {
    public partial class MainWindow : Window {
        private readonly ImageProcessor imageProcessor = new();

        public MainWindow() {
            InitializeComponent();

            newImage.DataContext = imageProcessor.imageProcessorVM;
            progressBar.DataContext = imageProcessor.progressVM;
        }

        private void ChooseFileClick(object sender, RoutedEventArgs e) {
            var fileDialog = new OpenFileDialog() {
                Filter = "Image files|*.jpg;*.png;*.bmp",
            };

            if (fileDialog.ShowDialog() == true) {
                var bitmap = new BitmapImage(new (fileDialog.FileName));
                reference.Source = bitmap;
            } else {
                MessageBox.Show("You don't choose a file!", "ERROR!",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
