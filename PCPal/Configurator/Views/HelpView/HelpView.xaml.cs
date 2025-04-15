using PCPal.Configurator.ViewModels;

namespace PCPal.Configurator.Views;

public partial class HelpView : ContentView
{
    public HelpView(HelpViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}