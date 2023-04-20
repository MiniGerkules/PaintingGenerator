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
                Filter = "Image files|*.jpg;*jpeg;*.png;*.bmp",
            };

            if (fileDialog.ShowDialog() == true) {
                var template = new BitmapImage(new (fileDialog.FileName));
                reference.Source = template;
                imageProcessor.Process(template, new());
            } else {
                MessageBox.Show("You don't choose a file!", "ERROR!",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
