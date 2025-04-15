using PCPal.Configurator.ViewModels;

namespace PCPal.Configurator.Views.LCD;

public partial class LcdConfigView : ContentView
{
    public LcdConfigView(LcdConfigViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Initialize the view model when the view is loaded
        this.Loaded += (s, e) => viewModel.Initialize();
    }
}