namespace PCPal.Configurator;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = base.CreateWindow(activationState);

        // Configure window properties
        window.Title = "PCPal Configurator";
        window.MinimumWidth = 1000;
        window.MinimumHeight = 700;

        return window;
    }
}