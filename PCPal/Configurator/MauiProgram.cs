using Microsoft.Extensions.Logging;
using PCPal.Core.Services;
using PCPal.Configurator.ViewModels;
using PCPal.Configurator.Views;
using PCPal.Configurator.Views.LCD;
using PCPal.Configurator.Views.OLED;
using PCPal.Configurator.Views.TFT;
using PCPal.Configurator.Converters;

namespace PCPal.Configurator;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Consolas.ttf", "Consolas");
            });

        // Register services
        builder.Services.AddSingleton<ISensorService, SensorService>();
        builder.Services.AddSingleton<ISerialPortService, SerialPortService>();
        builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

        // Register views and view models
        // LCD
        builder.Services.AddTransient<LcdConfigView>();
        builder.Services.AddTransient<LcdConfigViewModel>();

        // OLED
        builder.Services.AddTransient<OledConfigView>();
        builder.Services.AddTransient<OledConfigViewModel>();
        builder.Services.AddTransient<OledVisualEditorView>();
        builder.Services.AddTransient<OledMarkupEditorView>();
        builder.Services.AddTransient<OledTemplatesView>();

        // TFT
        builder.Services.AddTransient<TftConfigView>();
        builder.Services.AddTransient<TftConfigViewModel>();

        // Settings
        builder.Services.AddTransient<SettingsView>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Help
        builder.Services.AddTransient<HelpView>();
        builder.Services.AddTransient<HelpViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}