using PCPal.Configurator.ViewModels;

namespace PCPal.Configurator.Views.TFT;

public partial class TftConfigView : ContentView
{
    public TftConfigView(TftConfigViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
    }
}