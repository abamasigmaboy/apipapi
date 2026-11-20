using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SupplyTrackingSystem.ViewModels;

namespace SupplyTrackingSystem.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}