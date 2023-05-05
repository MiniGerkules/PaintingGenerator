using System;
using System.Windows;
using System.Windows.Controls;

namespace PaintingsGenerator.Pages {
    public partial class ObservationOfStrokesPage : UserControl, IPage {
        Grid IPage.Container => strokeDisplayer;

        public ObservationOfStrokesPage(StrokeProcessorVM strokeProcessorVM, ActionsVM actionsVM) {
            InitializeComponent();

            strokeDisplayer.DataContext = strokeProcessorVM;
            actionDisplayer.DataContext = actionsVM;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var button = (Button)sender;
            ((Action)button.Tag)();
        }
    }
}
