using PCPal.Configurator.ViewModels;

namespace PCPal.Configurator.Views;

public partial class SettingsView : ContentView
{
    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Initialize the view model when the view is loaded
        this.Loaded += (s, e) => viewModel.Initialize();
    }
}