using PCPal.Configurator.ViewModels;

namespace PCPal.Configurator.Views.OLED;

public partial class OledMarkupEditorView : ContentView
{
    private OledConfigViewModel _viewModel;

    public OledMarkupEditorView()
    {
        InitializeComponent();
        this.Loaded += OledMarkupEditorView_Loaded;
    }

    private void OledMarkupEditorView_Loaded(object sender, EventArgs e)
    {
        _viewModel = BindingContext as OledConfigViewModel;
    }

    private void Editor_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_viewModel != null)
        {
            // Notify the view model that the markup has changed
            _viewModel.UpdatePreviewFromMarkup();
        }
    }
}