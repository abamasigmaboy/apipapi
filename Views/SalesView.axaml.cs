using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SupplyTrackingSystem.Views
{
    public partial class SalesView : UserControl
    {
        public SalesView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
