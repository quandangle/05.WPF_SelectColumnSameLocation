using System.Windows;
using System.Windows.Input;

namespace QApps
{

    public partial class SelectColumnSamePlaceWindow
    {
        private SelectColumnSamePlaceViewModel _viewModel;

        public SelectColumnSamePlaceWindow(SelectColumnSamePlaceViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
           //DialogResult = true;
            Close();
            _viewModel.SelectColumnSamePlace();
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
           DialogResult = false;
            Close();
        }


        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                DialogResult = true;
                Close();
            }

            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Height = DockPanel.ActualHeight + 45;
            MinHeight = DockPanel.ActualHeight + 45;
        }

    }
}
