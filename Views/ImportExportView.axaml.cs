using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SupplyTrackingSystem.Views
{
    public partial class ImportExportView : UserControl
    {
        public ImportExportView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
