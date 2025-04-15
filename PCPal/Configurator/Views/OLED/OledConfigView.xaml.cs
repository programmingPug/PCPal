using PCPal.Configurator.ViewModels;

namespace PCPal.Configurator.Views.OLED;

public partial class OledConfigView : ContentView
{
    public OledConfigView(OledConfigViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Initialize the view model when the view is loaded
        this.Loaded += (s, e) => viewModel.Initialize();
    }
}