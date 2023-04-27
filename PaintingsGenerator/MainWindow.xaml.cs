using System;
using System.Windows;
using System.Windows.Media.Imaging;

using Microsoft.Win32;
using PaintingsGenerator.StrokesLib;

namespace PaintingsGenerator {
    public partial class MainWindow : Window {
        private readonly ImageProcessor imageProcessor = new();

        public MainWindow() {
            InitializeComponent();

            imageDisplayer.DataContext = imageProcessor.imageProcessorVM;
            statusBar.DataContext = imageProcessor.progressVM;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            try {
                StrokeLibManager.LoadStrokesLib();
            } catch (Exception error) {
                MessageBox.Show(error.Message, "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void ChooseFileClick(object sender, RoutedEventArgs e) {
            var fileDialog = new OpenFileDialog() {
                Filter = "Image files|*.jpg;*jpeg;*.png;*.bmp",
            };

            if (fileDialog.ShowDialog() == true) {
                var template = new BitmapImage(new(fileDialog.FileName));
                if (template.CanFreeze) template.Freeze();

                reference.Source = template;
                imageProcessor.Process(template, new());
            } else {
                MessageBox.Show("You don't choose a file!", "ERROR!",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
