using System.Windows;

namespace PolyLauncher.Views
{
    public partial class ConfigurationWindow : Window
    {
        public ConfigurationWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.ConfigurationViewModel(
                new Services.SettingsService());
        }
    }
}
