using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SupplyTrackingSystem.Views
{
    public partial class DepartmentsView : UserControl
    {
        public DepartmentsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
