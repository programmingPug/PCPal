using PCPal.Core.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows.Input;

namespace PCPal.Configurator.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly IConfigurationService _configService;
    private readonly ISerialPortService _serialPortService;

    private ObservableCollection<string> _availablePorts = new();
    private string _selectedPort;
    private bool _isAutoDetectEnabled;
    private string _connectionStatus;
    private bool _isConnected;
    private float _refreshRate;
    private bool _startWithWindows;
    private bool _minimizeToTray;
    private ObservableCollection<string> _availableThemes = new();
    private string _selectedTheme;
    private string _serviceStatus;
    private bool _isServiceRunning;

    public ObservableCollection<string> AvailablePorts
    {
        get => _availablePorts;
        set => SetProperty(ref _availablePorts, value);
    }

    public string SelectedPort
    {
        get => _selectedPort;
        set => SetProperty(ref _selectedPort, value);
    }

    public bool IsAutoDetectEnabled
    {
        get => _isAutoDetectEnabled;
        set => SetProperty(ref _isAutoDetectEnabled, value);
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => SetProperty(ref _connectionStatus, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    public float RefreshRate
    {
        get => _refreshRate;
        set => SetProperty(ref _refreshRate, value);
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => SetProperty(ref _startWithWindows, value);
    }

    public bool MinimizeToTray
    {
        get => _minimizeToTray;
        set => SetProperty(ref _minimizeToTray, value);
    }

    public ObservableCollection<string> AvailableThemes
    {
        get => _availableThemes;
        set => SetProperty(ref _availableThemes, value);
    }

    public string SelectedTheme
    {
        get => _selectedTheme;
        set => SetProperty(ref _selectedTheme, value);
    }

    public string ServiceStatus
    {
        get => _serviceStatus;
        set => SetProperty(ref _serviceStatus, value);
    }

    public bool IsServiceRunning
    {
        get => _isServiceRunning;
        set => SetProperty(ref _isServiceRunning, value);
    }

    // Commands
    public ICommand TestConnectionCommand { get; }
    public ICommand SaveSettingsCommand { get; }
    public ICommand ExportSettingsCommand { get; }
    public ICommand ImportSettingsCommand { get; }
    public ICommand ResetSettingsCommand { get; }
    public ICommand RefreshServiceStatusCommand { get; }
    public ICommand StartServiceCommand { get; }
    public ICommand StopServiceCommand { get; }
    public ICommand RestartServiceCommand { get; }
    
    public SettingsViewModel(IConfigurationService configService, ISerialPortService serialPortService)
    {
        Title = "Settings";

        _configService = configService;
        _serialPortService = serialPortService;

        // Default values
        IsConnected = false;
        ConnectionStatus = "Not connected";
        RefreshRate = 5.0f;
        IsAutoDetectEnabled = true;
        ServiceStatus = "Unknown";
        IsServiceRunning = false;

        // Initialize themes
        AvailableThemes = new ObservableCollection<string>
        {
            "System Default",
            "Light",
            "Dark"
        };
        SelectedTheme = "System Default";

        // Initialize commands
        TestConnectionCommand = new Command(async () => await TestConnectionAsync());
        SaveSettingsCommand = new Command(async () => await SaveSettingsAsync());
        ExportSettingsCommand = new Command(async () => await ExportSettingsAsync());
        ImportSettingsCommand = new Command(async () => await ImportSettingsAsync());
        ResetSettingsCommand = new Command(async () => await ResetSettingsAsync());
        RefreshServiceStatusCommand = new Command(async () => await RefreshServiceStatusAsync());
        StartServiceCommand = new Command(async () => await StartServiceAsync());
        StopServiceCommand = new Command(async () => await StopServiceAsync());
        RestartServiceCommand = new Command(async () => await RestartServiceAsync());
        
        // Subscribe to serial port connection changes
        _serialPortService.ConnectionStatusChanged += OnConnectionStatusChanged;
    }

    public async Task Initialize()
    {
        IsBusy = true;

        try
        {
            // Load available ports
            await RefreshPortsAsync();

            // Load settings
            await LoadSettingsAsync();

            // Check service status
            await RefreshServiceStatusAsync();

            // Check connection status
            await _serialPortService.CheckConnectionAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to initialize settings: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshPortsAsync()
    {
        try
        {
            var ports = SerialPort.GetPortNames();
            AvailablePorts = new ObservableCollection<string>(ports);

            if (AvailablePorts.Count > 0 && string.IsNullOrEmpty(SelectedPort))
            {
                SelectedPort = AvailablePorts[0];
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error refreshing ports: {ex.Message}");
        }
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            var config = await _configService.LoadConfigAsync();

            // Port settings
            if (!string.IsNullOrEmpty(config.LastUsedPort) && AvailablePorts.Contains(config.LastUsedPort))
            {
                SelectedPort = config.LastUsedPort;
            }

            // Other settings (these would be in extended config in a real implementation)
            // For now, we're using default values
            RefreshRate = 5.0f;
            StartWithWindows = false;
            MinimizeToTray = true;
            SelectedTheme = "System Default";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading settings: {ex.Message}");
        }
    }

    private async Task TestConnectionAsync()
    {
        try
        {
            IsBusy = true;

            bool success;
            if (IsAutoDetectEnabled)
            {
                success = await _serialPortService.ConnectToFirstAvailableAsync();
            }
            else
            {
                success = await _serialPortService.ConnectAsync(SelectedPort);
            }

            if (success)
            {
                await Shell.Current.DisplayAlert("Connection Test",
                    $"Successfully connected to PCPal device on {_serialPortService.CurrentPort}", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Connection Test",
                    "Failed to connect to PCPal device. Please check your connection and try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Connection test failed: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveSettingsAsync()
    {
        try
        {
            IsBusy = true;

            var config = await _configService.LoadConfigAsync();

            // Update settings
            config.LastUsedPort = IsAutoDetectEnabled ? null : SelectedPort;

            // Save settings
            await _configService.SaveConfigAsync(config);

            // In a real implementation, we would also:
            // - Save refresh rate to the service configuration
            // - Configure Windows startup settings
            // - Configure minimize to tray behavior
            // - Set the application theme

            await Shell.Current.DisplayAlert("Success", "Settings saved successfully!", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to save settings: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExportSettingsAsync()
    {
        try
        {
            // In a real implementation, we would:
            // - Show a file picker dialog
            // - Export settings to the selected file

            await Shell.Current.DisplayAlert("Export", "This feature is not implemented in the demo.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to export settings: {ex.Message}", "OK");
        }
    }

    private async Task ImportSettingsAsync()
    {
        try
        {
            // In a real implementation, we would:
            // - Show a file picker dialog
            // - Import settings from the selected file
            // - Apply the imported settings

            await Shell.Current.DisplayAlert("Import", "This feature is not implemented in the demo.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to import settings: {ex.Message}", "OK");
        }
    }

    private async Task ResetSettingsAsync()
    {
        try
        {
            bool confirm = await Shell.Current.DisplayAlert("Reset Settings",
                "Are you sure you want to reset all settings to default? This cannot be undone.",
                "Reset", "Cancel");

            if (!confirm)
                return;

            var config = new PCPal.Core.Models.DisplayConfig();
            await _configService.SaveConfigAsync(config);

            // Reload settings
            await LoadSettingsAsync();

            await Shell.Current.DisplayAlert("Success", "Settings have been reset to default.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to reset settings: {ex.Message}", "OK");
        }
    }

    private async Task RefreshServiceStatusAsync()
    {
        try
        {
            // In a real implementation, we would check the Windows Service status
            // For now, we'll just simulate it
            var random = new Random();
            IsServiceRunning = random.Next(2) == 1; // 50% chance of running

            ServiceStatus = IsServiceRunning ? "Running" : "Stopped";
        }
        catch (Exception ex)
        {
            ServiceStatus = "Error checking status";
            Debug.WriteLine($"Error checking service status: {ex.Message}");
        }
    }

    private async Task StartServiceAsync()
    {
        try
        {
            // In a real implementation, we would start the Windows Service
            // For now, we'll just simulate it
            await Task.Delay(500); // Simulate some delay

            IsServiceRunning = true;
            ServiceStatus = "Running";

            await Shell.Current.DisplayAlert("Success", "PCPal Service has been started.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to start service: {ex.Message}", "OK");
        }
    }

    private async Task StopServiceAsync()
    {
        try
        {
            // In a real implementation, we would stop the Windows Service
            // For now, we'll just simulate it
            await Task.Delay(500); // Simulate some delay

            IsServiceRunning = false;
            ServiceStatus = "Stopped";

            await Shell.Current.DisplayAlert("Success", "PCPal Service has been stopped.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to stop service: {ex.Message}", "OK");
        }
    }

    private async Task RestartServiceAsync()
    {
        try
        {
            // In a real implementation, we would restart the Windows Service
            // For now, we'll just simulate it
            await Task.Delay(1000); // Simulate some delay

            IsServiceRunning = true;
            ServiceStatus = "Running";

            await Shell.Current.DisplayAlert("Success", "PCPal Service has been restarted.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to restart service: {ex.Message}", "OK");
        }
    }

    private void OnConnectionStatusChanged(object sender, bool isConnected)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsConnected = isConnected;
            ConnectionStatus = isConnected ? $"Connected to {_serialPortService.CurrentPort}" : "Not connected";
        });
    }
}