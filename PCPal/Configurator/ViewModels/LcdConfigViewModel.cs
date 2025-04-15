using PCPal.Core.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace PCPal.Configurator.ViewModels;

public class LcdConfigViewModel : BaseViewModel
{
    private readonly ISensorService _sensorService;
    private readonly IConfigurationService _configService;
    private readonly ISerialPortService _serialPortService;

    private ObservableCollection<string> _sensorOptions = new();
    private string _line1Selection;
    private string _line1CustomText;
    private string _line1PostText;
    private string _line2Selection;
    private string _line2CustomText;
    private string _line2PostText;
    private string _line1Preview;
    private string _line2Preview;

    private Timer _previewUpdateTimer;

    public ObservableCollection<string> SensorOptions
    {
        get => _sensorOptions;
        set => SetProperty(ref _sensorOptions, value);
    }

    public string Line1Selection
    {
        get => _line1Selection;
        set
        {
            if (SetProperty(ref _line1Selection, value))
            {
                OnPropertyChanged(nameof(IsLine1CustomTextEnabled));
                OnPropertyChanged(nameof(IsLine1PostTextEnabled));
                UpdatePreview();
            }
        }
    }

    public string Line1CustomText
    {
        get => _line1CustomText;
        set
        {
            if (SetProperty(ref _line1CustomText, value))
            {
                UpdatePreview();
            }
        }
    }

    public string Line1PostText
    {
        get => _line1PostText;
        set
        {
            if (SetProperty(ref _line1PostText, value))
            {
                UpdatePreview();
            }
        }
    }

    public string Line2Selection
    {
        get => _line2Selection;
        set
        {
            if (SetProperty(ref _line2Selection, value))
            {
                OnPropertyChanged(nameof(IsLine2CustomTextEnabled));
                OnPropertyChanged(nameof(IsLine2PostTextEnabled));
                UpdatePreview();
            }
        }
    }

    public string Line2CustomText
    {
        get => _line2CustomText;
        set
        {
            if (SetProperty(ref _line2CustomText, value))
            {
                UpdatePreview();
            }
        }
    }

    public string Line2PostText
    {
        get => _line2PostText;
        set
        {
            if (SetProperty(ref _line2PostText, value))
            {
                UpdatePreview();
            }
        }
    }

    public string Line1Preview
    {
        get => _line1Preview;
        set => SetProperty(ref _line1Preview, value);
    }

    public string Line2Preview
    {
        get => _line2Preview;
        set => SetProperty(ref _line2Preview, value);
    }

    public bool IsLine1CustomTextEnabled => Line1Selection == "Custom Text" || !string.IsNullOrEmpty(Line1Selection);
    public bool IsLine1PostTextEnabled => Line1Selection != "Custom Text" && !string.IsNullOrEmpty(Line1Selection);
    public bool IsLine2CustomTextEnabled => Line2Selection == "Custom Text" || !string.IsNullOrEmpty(Line2Selection);
    public bool IsLine2PostTextEnabled => Line2Selection != "Custom Text" && !string.IsNullOrEmpty(Line2Selection);

    public ICommand SaveConfigCommand { get; }
    public ICommand TestConnectionCommand { get; }

    public LcdConfigViewModel(
        ISensorService sensorService,
        IConfigurationService configService,
        ISerialPortService serialPortService)
    {
        _sensorService = sensorService;
        _configService = configService;
        _serialPortService = serialPortService;

        SaveConfigCommand = new Command(async () => await SaveConfigAsync());
        TestConnectionCommand = new Command(async () => await TestConnectionAsync());

        // Setup timer for preview updates
        _previewUpdateTimer = new Timer(async (_) => await UpdateSensorDataAsync(), null, Timeout.Infinite, Timeout.Infinite);
    }

    public async Task Initialize()
    {
        IsBusy = true;

        try
        {
            await _sensorService.UpdateSensorValuesAsync();

            // Load sensor options
            await LoadSensorOptionsAsync();

            // Load configuration
            await LoadConfigAsync();

            // Start preview updates
            _previewUpdateTimer.Change(0, 2000); // Update every 2 seconds

            // Initial preview update
            await UpdateSensorDataAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to initialize: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadSensorOptionsAsync()
    {
        await _sensorService.UpdateSensorValuesAsync();

        var sensorGroups = _sensorService.GetAllSensorsGrouped();
        var options = new List<string>();

        foreach (var group in sensorGroups)
        {
            foreach (var sensor in group.Sensors)
            {
                // Only include load, temperature, and data sensors for simplicity
                if (sensor.SensorType == LibreHardwareMonitor.Hardware.SensorType.Load ||
                    sensor.SensorType == LibreHardwareMonitor.Hardware.SensorType.Temperature ||
                    sensor.SensorType == LibreHardwareMonitor.Hardware.SensorType.Data)
                {
                    options.Add($"{group.Type}: {sensor.Name} ({sensor.SensorType})");
                }
            }
        }

        options.Add("Custom Text");

        SensorOptions = new ObservableCollection<string>(options);
    }

    private async Task LoadConfigAsync()
    {
        var config = await _configService.LoadConfigAsync();

        // If config is found, load into UI
        Line1Selection = config.Line1Selection;
        Line1CustomText = config.Line1CustomText;
        Line1PostText = config.Line1PostText;
        Line2Selection = config.Line2Selection;
        Line2CustomText = config.Line2CustomText;
        Line2PostText = config.Line2PostText;

        // Set defaults if not configured
        if (string.IsNullOrEmpty(Line1Selection) && SensorOptions.Count > 0)
        {
            // Try to find a CPU load sensor as default
            var cpuOption = SensorOptions.FirstOrDefault(s => s.Contains("Cpu") && s.Contains("Load"));
            Line1Selection = cpuOption ?? SensorOptions.First();
            Line1CustomText = "CPU ";
            Line1PostText = "%";
        }

        if (string.IsNullOrEmpty(Line2Selection) && SensorOptions.Count > 1)
        {
            // Try to find a Memory sensor as default
            var memoryOption = SensorOptions.FirstOrDefault(s => s.Contains("Memory") && s.Contains("Data"));
            Line2Selection = memoryOption ?? SensorOptions[1];
            Line2CustomText = "Memory ";
            Line2PostText = "GB";
        }
    }

    private async Task SaveConfigAsync()
    {
        try
        {
            IsBusy = true;

            var config = await _configService.LoadConfigAsync();

            // Update LCD configuration
            config.ScreenType = "1602";
            config.Line1Selection = Line1Selection;
            config.Line1CustomText = Line1CustomText;
            config.Line1PostText = Line1PostText;
            config.Line2Selection = Line2Selection;
            config.Line2CustomText = Line2CustomText;
            config.Line2PostText = Line2PostText;

            await _configService.SaveConfigAsync(config);

            // Send configuration to device if connected
            if (_serialPortService.IsConnected)
            {
                await _serialPortService.SendCommandAsync($"CMD:LCD,0,{Line1Preview}");
                await _serialPortService.SendCommandAsync($"CMD:LCD,1,{Line2Preview}");
            }

            await Shell.Current.DisplayAlert("Success", "Configuration saved successfully!", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to save configuration: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TestConnectionAsync()
    {
        try
        {
            IsBusy = true;

            if (!_serialPortService.IsConnected)
            {
                var result = await _serialPortService.ConnectToFirstAvailableAsync();

                if (!result)
                {
                    await Shell.Current.DisplayAlert("Connection Failed",
                        "Could not connect to PCPal device. Please check that it's connected to your computer.", "OK");
                    return;
                }
            }

            // Send test message to the LCD
            await _serialPortService.SendCommandAsync($"CMD:LCD,0,PCPal Test");
            await _serialPortService.SendCommandAsync($"CMD:LCD,1,Connection OK!");

            await Shell.Current.DisplayAlert("Connection Successful",
                $"Connected to PCPal device on {_serialPortService.CurrentPort}", "OK");

            // Restore actual content after 3 seconds
            await Task.Delay(3000);
            await _serialPortService.SendCommandAsync($"CMD:LCD,0,{Line1Preview}");
            await _serialPortService.SendCommandAsync($"CMD:LCD,1,{Line2Preview}");
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

    private async Task UpdateSensorDataAsync()
    {
        try
        {
            await _sensorService.UpdateSensorValuesAsync();
            UpdatePreview();
        }
        catch (Exception ex)
        {
            // Log error but don't display to user since this happens in background
            Debug.WriteLine($"Error updating sensor data: {ex.Message}");
        }
    }

    private void UpdatePreview()
    {
        Line1Preview = GetSensorLinePreview(Line1Selection, Line1CustomText, Line1PostText);
        Line2Preview = GetSensorLinePreview(Line2Selection, Line2CustomText, Line2PostText);
    }

    private string GetSensorLinePreview(string selection, string prefix, string suffix)
    {
        if (string.IsNullOrEmpty(selection))
        {
            return string.Empty;
        }

        if (selection == "Custom Text")
        {
            return prefix ?? string.Empty;
        }

        try
        {
            // Parse sensor selection to get hardware type, sensor name and type
            var parts = selection.Split(':', 2);
            if (parts.Length != 2)
            {
                return "Error: Invalid selection";
            }

            var hardwareTypeStr = parts[0].Trim();
            var sensorInfo = parts[1].Trim();

            int idx = sensorInfo.LastIndexOf('(');
            if (idx == -1)
            {
                return "Error: Invalid format";
            }

            string sensorName = sensorInfo.Substring(0, idx).Trim();
            string sensorTypeStr = sensorInfo.Substring(idx + 1).Trim().TrimEnd(')');

            // Get sensor value
            var sensorGroups = _sensorService.GetAllSensorsGrouped();
            var sensor = sensorGroups
                .SelectMany(g => g.Sensors)
                .FirstOrDefault(s =>
                    s.SensorType.ToString() == sensorTypeStr &&
                    s.Name == sensorName);

            if (sensor != null)
            {
                return $"{prefix ?? ""}{sensor.FormattedValue}{suffix ?? ""}";
            }

            return "N/A";
        }
        catch
        {
            return "Error";
        }
    }
}