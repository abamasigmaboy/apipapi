using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SupplyTrackingSystem.Views
{
    public partial class ProductsView : UserControl
    {
        public ProductsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
