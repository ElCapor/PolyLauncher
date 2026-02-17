using System.Windows;

namespace PolyLauncher
{
    public partial class MainWindow : Window
    {
        private readonly ViewModels.MainViewModel _viewModel;

        public MainWindow(ViewModels.MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void ConfigureButton_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new Views.ConfigurationWindow();
            configWindow.ShowDialog();
            
            var settings = new Services.SettingsService().LoadSettings();
            if (!settings.FirstRun)
            {
                _viewModel.CloseConfigurationCommand.Execute(null);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}