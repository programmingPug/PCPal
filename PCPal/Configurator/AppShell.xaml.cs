using PCPal.Core.Services;
using PCPal.Configurator.ViewModels;
using PCPal.Configurator.Views;
using PCPal.Configurator.Views.LCD;
using PCPal.Configurator.Views.OLED;
using PCPal.Configurator.Views.TFT;
using System.ComponentModel;
//using UIKit;

namespace PCPal.Configurator;

public partial class AppShell : Shell, INotifyPropertyChanged
{
    private bool _isConnected;
    private string _connectionStatus;
    private DateTime _lastUpdateTime;

    private readonly IServiceProvider _serviceProvider;

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set
        {
            if (_connectionStatus != value)
            {
                _connectionStatus = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime LastUpdateTime
    {
        get => _lastUpdateTime;
        set
        {
            if (_lastUpdateTime != value)
            {
                _lastUpdateTime = value;
                OnPropertyChanged();
            }
        }
    }

    public AppShell()
    {
        InitializeComponent();

        _serviceProvider = IPlatformApplication.Current.Services;

        // Set initial connection status
        IsConnected = false;
        ConnectionStatus = "Not connected";
        LastUpdateTime = DateTime.Now;

        // Start with LCD view
        NavMenu.SelectedItem = "1602 LCD Display";

        // Start connection monitoring in the background
        StartConnectivityMonitoring();
    }

    private void OnNavMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string selection)
        {
            ContentView view = selection switch
            {
                "1602 LCD Display" => _serviceProvider.GetService<LcdConfigView>(),
                "4.6 TFT Display" => _serviceProvider.GetService<TftConfigView>(),
                "OLED Display" => _serviceProvider.GetService<OledConfigView>(),
                "Settings" => _serviceProvider.GetService<SettingsView>(),
                "Help" => _serviceProvider.GetService<HelpView>(),
                _ => null
            };

            if (view != null)
            {
                ContentContainer.Content = view;
            }
        }
    }

    private async void StartConnectivityMonitoring()
    {
        var serialPortService = _serviceProvider.GetService<ISerialPortService>();
        if (serialPortService != null)
        {
            // Subscribe to connection status changes
            serialPortService.ConnectionStatusChanged += (sender, isConnected) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsConnected = isConnected;
                    ConnectionStatus = isConnected ? "Connected to " + serialPortService.CurrentPort : "Not connected";
                    LastUpdateTime = DateTime.Now;
                });
            };

            // Start periodic connection check
            while (true)
            {
                await Task.Delay(5000);
                try
                {
                    await serialPortService.CheckConnectionAsync();
                }
                catch (Exception ex)
                {
                    // Log error but don't crash the app
                    System.Diagnostics.Debug.WriteLine($"Connection check error: {ex.Message}");
                }
            }
        }
    }
}