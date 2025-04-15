using System.Windows.Input;

namespace PCPal.Configurator.ViewModels;

public class TftConfigViewModel : BaseViewModel
{
    public ICommand NotifyCommand { get; }

    public TftConfigViewModel()
    {
        Title = "TFT Display Configuration";

        // Initialize commands
        NotifyCommand = new Command(async () => await NotifyWhenAvailableAsync());
    }

    private async Task NotifyWhenAvailableAsync()
    {
        // Display a prompt for the user's email
        string result = await Shell.Current.DisplayPromptAsync(
            "Notification Sign-up",
            "Enter your email to be notified when TFT display support becomes available:",
            "Subscribe",
            "Cancel",
            "email@example.com",
            keyboard: Keyboard.Email);

        if (!string.IsNullOrWhiteSpace(result))
        {
            // In a real app, we would save this email to a notification list

            // Display confirmation
            await Shell.Current.DisplayAlert(
                "Thank You!",
                "We'll notify you when TFT display support is ready. Your email has been registered for updates.",
                "OK");
        }
    }
}