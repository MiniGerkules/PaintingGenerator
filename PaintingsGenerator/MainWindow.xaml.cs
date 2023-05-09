using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

using Microsoft.Win32;
using PaintingsGenerator.Pages;
using PaintingsGenerator.StrokesLib;

namespace PaintingsGenerator {
    public partial class MainWindow : Window {
        private readonly ImageProcessor imageProcessor = new();
        private readonly Dictionary<MenuItem, IPage> pages;

        public MainWindow() {
            InitializeComponent();

            pages = new() {
                { imgGenButton, new PictureGeneratingPage(imageProcessor.imageProcessorVM) },
                { strokeGenButton, new ObservationOfStrokesPage(imageProcessor.strokeProcessorVM, imageProcessor.actionsVM) },
            };

            foreach (var (_, page) in pages) {
                page.SetInactive();
                pagePlaceholder.Children.Add(page as UserControl);
            }

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
                var template = new LocalBitmap(fileDialog.FileName,
                                               new BitmapImage(new(fileDialog.FileName)));
                if (template.CanFreeze) template.Freeze();

                ChangePage(imgGenButton);
                imageProcessor.Process(template, new());
            } else {
                MessageBox.Show("You don't choose a file!", "ERROR!",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangePage(object sender, RoutedEventArgs e) {
            var menuItem = (MenuItem)sender;
            ChangePage(menuItem);
        }

        private void ChangePage(MenuItem menuItem) {
            SetAllPagesInactive();
            pages[menuItem].SetActive();
        }

        private void SetAllPagesInactive() {
            foreach (var (_, page) in pages)
                page.SetInactive();
        }
    }
}
