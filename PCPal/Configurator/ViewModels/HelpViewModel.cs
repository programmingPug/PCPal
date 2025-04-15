using System.Windows.Input;

namespace PCPal.Configurator.ViewModels;

public class HelpViewModel : BaseViewModel
{
    private string _searchQuery;

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public ICommand SearchCommand { get; }
    public ICommand OpenUrlCommand { get; }

    public HelpViewModel()
    {
        Title = "Help & Documentation";

        // Initialize commands
        SearchCommand = new Command<string>(Search);
        OpenUrlCommand = new Command<string>(OpenUrl);
    }

    private void Search(string query)
    {
        // In a real implementation, this would search through help topics
        // For now, we'll just update the search query property
        SearchQuery = query;
    }

    private async void OpenUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return;

        try
        {
            await Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            // Handle error or notify user
            await Shell.Current.DisplayAlert("Error", $"Cannot open URL: {ex.Message}", "OK");
        }
    }
}