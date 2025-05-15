using PCPal.Core.Models;
using PCPal.Core.Services;
using PCPal.Configurator.Views.OLED;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace PCPal.Configurator.ViewModels;

public class OledConfigViewModel : BaseViewModel
{
    private readonly ISensorService _sensorService;
    private readonly IConfigurationService _configService;
    private readonly ISerialPortService _serialPortService;

    // Tab selection
    private bool _isMarkupEditorSelected;
    private bool _isTemplatesSelected;
    private ContentView _currentView;

    // Markup editor data
    private string _oledMarkup;
    private List<PreviewElement> _previewElements;

    // Common properties
    private ObservableCollection<SensorItem> _availableSensors;

    // Templates properties
    private ObservableCollection<Template> _templateList;
    private Template _selectedTemplate;
    private ObservableCollection<Template> _customTemplates;
    private string _newTemplateName;

    // Views
    private readonly OledMarkupEditorView _markupEditorView;
    private readonly OledTemplatesView _templatesView;

    // Timer for sensor updates
    private Timer _sensorUpdateTimer;
    private CancellationTokenSource _sensorUpdateCts;

    #region Properties

    // Tab selection properties
    public bool IsMarkupEditorSelected
    {
        get => _isMarkupEditorSelected;
        set => SetProperty(ref _isMarkupEditorSelected, value);
    }

    public bool IsTemplatesSelected
    {
        get => _isTemplatesSelected;
        set => SetProperty(ref _isTemplatesSelected, value);
    }

    public ContentView CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    // Markup editor properties
    public string OledMarkup
    {
        get => _oledMarkup;
        set => SetProperty(ref _oledMarkup, value);
    }

    public List<PreviewElement> PreviewElements
    {
        get => _previewElements;
        set => SetProperty(ref _previewElements, value);
    }

    // Common properties
    public ObservableCollection<SensorItem> AvailableSensors
    {
        get => _availableSensors;
        set => SetProperty(ref _availableSensors, value);
    }

    // Templates properties
    public ObservableCollection<Template> TemplateList
    {
        get => _templateList;
        set => SetProperty(ref _templateList, value);
    }

    public Template SelectedTemplate
    {
        get => _selectedTemplate;
        set
        {
            if (SetProperty(ref _selectedTemplate, value))
            {
                OnPropertyChanged(nameof(HasSelectedTemplate));
            }
        }
    }

    public bool HasSelectedTemplate => SelectedTemplate != null;

    public ObservableCollection<Template> CustomTemplates
    {
        get => _customTemplates;
        set => SetProperty(ref _customTemplates, value);
    }

    public string NewTemplateName
    {
        get => _newTemplateName;
        set => SetProperty(ref _newTemplateName, value);
    }

    #endregion

    #region Commands

    // Tab selection commands
    public ICommand SwitchToMarkupEditorCommand { get; }
    public ICommand SwitchToTemplatesCommand { get; }

    // Common commands
    public ICommand SaveConfigCommand { get; }
    public ICommand PreviewCommand { get; }
    public ICommand ResetCommand { get; }

    // Markup editor commands
    public ICommand InsertMarkupCommand { get; }
    public ICommand InsertSensorVariableCommand { get; }
    public ICommand LoadExampleCommand { get; }

    // Templates commands
    public ICommand UseTemplateCommand { get; }
    public ICommand SaveAsTemplateCommand { get; }
    public ICommand UseCustomTemplateCommand { get; }
    public ICommand DeleteCustomTemplateCommand { get; }

    #endregion

    public OledConfigViewModel(
        ISensorService sensorService,
        IConfigurationService configService,
        ISerialPortService serialPortService)
    {
        _sensorService = sensorService ?? throw new ArgumentNullException(nameof(sensorService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _serialPortService = serialPortService ?? throw new ArgumentNullException(nameof(serialPortService));

        // Initialize collections
        _previewElements = new List<PreviewElement>();
        _availableSensors = new ObservableCollection<SensorItem>();
        _templateList = new ObservableCollection<Template>();
        _customTemplates = new ObservableCollection<Template>();

        // Setup cancellation token source
        _sensorUpdateCts = new CancellationTokenSource();

        // Create views
        _markupEditorView = new OledMarkupEditorView { BindingContext = this };
        _templatesView = new OledTemplatesView { BindingContext = this };

        // Default values
        _isMarkupEditorSelected = true;
        _currentView = _markupEditorView;

        // Tab selection commands
        SwitchToMarkupEditorCommand = new Command(() => SwitchTab("markup"));
        SwitchToTemplatesCommand = new Command(() => SwitchTab("templates"));

        // Common commands
        SaveConfigCommand = new Command(async () => await SaveConfigAsync());
        PreviewCommand = new Command(async () => await PreviewOnDeviceAsync());
        ResetCommand = new Command(async () => await ResetLayoutAsync());

        // Markup editor commands
        InsertMarkupCommand = new Command<string>(type => InsertMarkupTemplate(type));
        InsertSensorVariableCommand = new Command(async () => await InsertSensorVariableAsync());
        LoadExampleCommand = new Command(async () => await LoadExampleMarkupAsync());

        // Templates commands
        UseTemplateCommand = new Command(async () => await UseSelectedTemplateAsync());
        SaveAsTemplateCommand = new Command(async () => await SaveAsTemplateAsync());
        UseCustomTemplateCommand = new Command<Template>(async (template) => await UseCustomTemplateAsync(template));
        DeleteCustomTemplateCommand = new Command<Template>(async (template) => await DeleteCustomTemplateAsync(template));

        // Setup timer for preview updates
        _sensorUpdateTimer = new Timer(async (_) => await UpdateSensorDataAsync(), null, Timeout.Infinite, Timeout.Infinite);
    }

    ~OledConfigViewModel()
    {
        CleanupResources();
    }

    private void CleanupResources()
    {
        try
        {
            _sensorUpdateCts?.Cancel();
            _sensorUpdateTimer?.Dispose();
            _sensorUpdateCts?.Dispose();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cleaning up resources: {ex.Message}");
        }
    }

    public async Task Initialize()
    {
        IsBusy = true;

        try
        {
            // Load sensor data
            await _sensorService.UpdateSensorValuesAsync();
            await LoadSensorsAsync();

            // Load configuration
            await LoadConfigAsync();

            // Load templates
            await LoadTemplatesAsync();

            // Switch to markup editor by default
            SwitchTab("markup");

            // Start sensor updates
            _sensorUpdateTimer.Change(0, 2000); // Update every 2 seconds
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Failed to initialize: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadSensorsAsync()
    {
        try
        {
            await _sensorService.UpdateSensorValuesAsync();

            var sensorGroups = _sensorService.GetAllSensorsGrouped();
            var sensors = new List<SensorItem>();

            foreach (var group in sensorGroups)
            {
                foreach (var sensor in group.Sensors)
                {
                    sensors.Add(sensor);
                }
            }

            // Update on main thread to ensure thread safety
            await MainThread.InvokeOnMainThreadAsync(() => {
                AvailableSensors = new ObservableCollection<SensorItem>(sensors);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading sensors: {ex.Message}");
        }
    }

    private async Task LoadConfigAsync()
    {
        try
        {
            var config = await _configService.LoadConfigAsync();

            // If OLED markup exists, load it
            if (!string.IsNullOrEmpty(config.OledMarkup))
            {
                OledMarkup = config.OledMarkup;
                UpdatePreviewFromMarkup();
            }
            else
            {
                // Load an example if no config exists
                await LoadExampleMarkupAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading configuration: {ex.Message}");
            await LoadExampleMarkupAsync(); // Fallback to example
        }
    }

    private async Task LoadTemplatesAsync()
    {
        try
        {
            // Predefined templates
            var templates = new List<Template>
            {
                new Template
                {
                    Name = "System Monitor",
                    Description = "Shows CPU, GPU, and memory usage with progress bars",
                    Markup = await CreateSystemMonitorTemplateAsync()
                },
                new Template
                {
                    Name = "Temperature Monitor",
                    Description = "Shows temperatures of key components",
                    Markup = await CreateTemperatureMonitorTemplateAsync()
                },
                new Template
                {
                    Name = "Network Monitor",
                    Description = "Shows network activity and throughput",
                    Markup = await CreateNetworkMonitorTemplateAsync()
                },
                new Template
                {
                    Name = "Storage Monitor",
                    Description = "Shows disk space usage and activity",
                    Markup = await CreateStorageMonitorTemplateAsync()
                }
            };

            // Process templates to create preview elements
            foreach (var template in templates)
            {
                template.PreviewElements = await ParseMarkupToPreviewElements(template.Markup);
            }

            TemplateList = new ObservableCollection<Template>(templates);

            // Load custom templates
            var config = await _configService.LoadConfigAsync();
            if (config.SavedProfiles != null && config.SavedProfiles.Any())
            {
                var customTemplates = new List<Template>();

                foreach (var profile in config.SavedProfiles)
                {
                    if (profile.ScreenType == "OLED")
                    {
                        var template = new Template
                        {
                            Name = profile.Name,
                            Description = "Custom template",
                            Markup = profile.ConfigData
                        };

                        template.PreviewElements = await ParseMarkupToPreviewElements(template.Markup);
                        customTemplates.Add(template);
                    }
                }

                CustomTemplates = new ObservableCollection<Template>(customTemplates);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading templates: {ex.Message}");
        }
    }

    private void SwitchTab(string tab)
    {
        IsMarkupEditorSelected = tab == "markup";
        IsTemplatesSelected = tab == "templates";

        OnPropertyChanged(nameof(IsMarkupEditorSelected));
        OnPropertyChanged(nameof(IsTemplatesSelected));

        switch (tab)
        {
            case "markup":
                CurrentView = _markupEditorView;
                break;
            case "templates":
                CurrentView = _templatesView;
                break;
        }
    }

    private async Task SaveConfigAsync()
    {
        try
        {
            IsBusy = true;

            var config = await _configService.LoadConfigAsync();

            // Update OLED configuration
            config.ScreenType = "OLED";
            config.OledMarkup = OledMarkup;

            await _configService.SaveConfigAsync(config);

            // Send configuration to device if connected
            if (_serialPortService.IsConnected)
            {
                string processedMarkup = _sensorService.ProcessVariablesInMarkup(OledMarkup);
                await _serialPortService.SendCommandAsync($"CMD:OLED,{processedMarkup}");
            }

            await Shell.Current.DisplayAlert("Success", "Configuration saved successfully!", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving configuration: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Failed to save configuration: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PreviewOnDeviceAsync()
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

            // Send markup to the OLED display
            string processedMarkup = _sensorService.ProcessVariablesInMarkup(OledMarkup);
            await _serialPortService.SendCommandAsync($"CMD:OLED,{processedMarkup}");

            await Shell.Current.DisplayAlert("Preview Sent",
                "Your design has been sent to the device. It has not been saved permanently.", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error previewing on device: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Preview failed: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ResetLayoutAsync()
    {
        try
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Reset Layout",
                "Are you sure you want to reset your layout? This will discard all your changes.",
                "Reset", "Cancel");

            if (confirm)
            {
                await LoadExampleMarkupAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error resetting layout: {ex.Message}");
        }
    }

    public void UpdatePreviewFromMarkup()
    {
        try
        {
            // Parse the markup into preview elements
            var markupParser = new MarkupParser(_sensorService.GetAllSensorValues());
            var elements = markupParser.ParseMarkup(OledMarkup);

            // Update on main thread to ensure thread safety
            MainThread.BeginInvokeOnMainThread(() => {
                PreviewElements = elements;
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error parsing markup: {ex.Message}");
        }
    }

    private async Task UpdateSensorDataAsync()
    {
        try
        {
            if (_sensorUpdateCts.Token.IsCancellationRequested)
                return;

            await _sensorService.UpdateSensorValuesAsync();
            UpdatePreviewFromMarkup();

            // Update available sensors
            await LoadSensorsAsync();
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
            Debug.WriteLine("Sensor update canceled");
        }
        catch (Exception ex)
        {
            // Log error but don't display to user since this happens in background
            Debug.WriteLine($"Error updating sensor data: {ex.Message}");
        }
    }

    private void InsertMarkupTemplate(string type)
    {
        try
        {
            string template = string.Empty;

            switch (type)
            {
                case "text":
                    template = "<text x=10 y=20 size=1>Sample Text</text>";
                    break;

                case "bar":
                    template = "<bar x=10 y=30 w=100 h=8 val=75 />";
                    break;

                case "rect":
                    template = "<rect x=10 y=40 w=50 h=20 />";
                    break;

                case "box":
                    template = "<box x=70 y=40 w=50 h=20 />";
                    break;

                case "line":
                    template = "<line x1=10 y1=50 x2=60 y2=50 />";
                    break;

                case "icon":
                    template = "<icon x=130 y=20 name=cpu />";
                    break;
            }

            if (!string.IsNullOrEmpty(template))
            {
                OledMarkup += Environment.NewLine + template;
                UpdatePreviewFromMarkup();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error inserting markup template: {ex.Message}");
        }
    }

    private async Task InsertSensorVariableAsync()
    {
        try
        {
            // Create a list of sensor options
            var options = AvailableSensors.Select(s => $"{s.DisplayName} ({s.Id})").ToArray();

            string result = await Shell.Current.DisplayActionSheet("Select a sensor", "Cancel", null, options);

            if (!string.IsNullOrEmpty(result) && result != "Cancel")
            {
                // Extract sensor ID
                string id = result.Substring(result.LastIndexOf('(') + 1).TrimEnd(')');

                // Insert the variable at the current cursor position (not implemented in this mock-up)
                // In a real implementation, you'd need to track the cursor position in the editor
                OledMarkup += $"{{{id}}}";
                UpdatePreviewFromMarkup();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error inserting sensor variable: {ex.Message}");
        }
    }

    private async Task LoadExampleMarkupAsync()
    {
        try
        {
            OledMarkup = await _sensorService.CreateExampleMarkupAsync();
            UpdatePreviewFromMarkup();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading example markup: {ex.Message}");
        }
    }

    private async Task<List<PreviewElement>> ParseMarkupToPreviewElements(string markup)
    {
        try
        {
            var markupParser = new MarkupParser(_sensorService.GetAllSensorValues());
            return markupParser.ParseMarkup(markup);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error parsing markup for preview: {ex.Message}");
            return new List<PreviewElement>();
        }
    }

    private async Task<string> CreateSystemMonitorTemplateAsync()
    {
        try
        {
            return await _sensorService.CreateExampleMarkupAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating system monitor template: {ex.Message}");
            return "<text x=10 y=20 size=1>System Monitor</text>";
        }
    }

    private async Task<string> CreateTemperatureMonitorTemplateAsync()
    {
        try
        {
            var sb = new System.Text.StringBuilder();

            string cpuTemp = _sensorService.FindFirstSensorOfType(LibreHardwareMonitor.Hardware.HardwareType.Cpu,
                LibreHardwareMonitor.Hardware.SensorType.Temperature);

            string gpuTemp = _sensorService.FindFirstSensorOfType(LibreHardwareMonitor.Hardware.HardwareType.GpuNvidia,
                LibreHardwareMonitor.Hardware.SensorType.Temperature);

            sb.AppendLine("<text x=0 y=12 size=2>Temperatures</text>");

            if (!string.IsNullOrEmpty(cpuTemp))
            {
                sb.AppendLine($"<text x=0 y=30 size=1>CPU: {{{cpuTemp}}}°C</text>");
            }

            if (!string.IsNullOrEmpty(gpuTemp))
            {
                sb.AppendLine($"<text x=0 y=45 size=1>GPU: {{{gpuTemp}}}°C</text>");
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating temperature monitor template: {ex.Message}");
            return "<text x=10 y=20 size=1>Temperature Monitor</text>";
        }
    }

    private async Task<string> CreateNetworkMonitorTemplateAsync()
    {
        try
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("<text x=0 y=12 size=2>Network Monitor</text>");
            sb.AppendLine("<text x=0 y=30 size=1>Coming soon!</text>");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating network monitor template: {ex.Message}");
            return "<text x=10 y=20 size=1>Network Monitor</text>";
        }
    }

    private async Task<string> CreateStorageMonitorTemplateAsync()
    {
        try
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("<text x=0 y=12 size=2>Storage Monitor</text>");
            sb.AppendLine("<text x=0 y=30 size=1>Coming soon!</text>");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating storage monitor template: {ex.Message}");
            return "<text x=10 y=20 size=1>Storage Monitor</text>";
        }
    }

    private async Task UseSelectedTemplateAsync()
    {
        try
        {
            if (SelectedTemplate == null)
            {
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Apply Template",
                "Are you sure you want to apply this template? This will replace your current design.",
                "Apply", "Cancel");

            if (confirm)
            {
                OledMarkup = SelectedTemplate.Markup;
                UpdatePreviewFromMarkup();
                SwitchTab("markup");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error using selected template: {ex.Message}");
        }
    }

    private async Task SaveAsTemplateAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NewTemplateName))
            {
                await Shell.Current.DisplayAlert("Error", "Please enter a name for your template.", "OK");
                return;
            }

            // Check for duplicate names
            if (CustomTemplates.Any(t => t.Name == NewTemplateName))
            {
                bool replace = await Shell.Current.DisplayAlert(
                    "Template Exists",
                    "A template with this name already exists. Do you want to replace it?",
                    "Replace", "Cancel");

                if (!replace)
                {
                    return;
                }

                // Remove the existing template
                var existingTemplate = CustomTemplates.First(t => t.Name == NewTemplateName);
                CustomTemplates.Remove(existingTemplate);
            }

            // Create new template
            var template = new Template
            {
                Name = NewTemplateName,
                Description = "Custom template",
                Markup = OledMarkup
            };

            template.PreviewElements = await ParseMarkupToPreviewElements(template.Markup);
            CustomTemplates.Add(template);

            // Save to configuration
            var config = await _configService.LoadConfigAsync();

            if (config.SavedProfiles == null)
            {
                config.SavedProfiles = new List<DisplayProfile>();
            }

            // Remove existing profile with same name
            config.SavedProfiles.RemoveAll(p => p.Name == NewTemplateName && p.ScreenType == "OLED");

            // Add new profile
            config.SavedProfiles.Add(new DisplayProfile
            {
                Name = NewTemplateName,
                ScreenType = "OLED",
                ConfigData = OledMarkup
            });

            await _configService.SaveConfigAsync(config);

            // Clear the input field
            NewTemplateName = string.Empty;

            await Shell.Current.DisplayAlert("Success", "Template saved successfully!", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving as template: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Failed to save template: {ex.Message}", "OK");
        }
    }

    private async Task UseCustomTemplateAsync(Template template)
    {
        try
        {
            if (template == null)
            {
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Apply Template",
                "Are you sure you want to apply this template? This will replace your current design.",
                "Apply", "Cancel");

            if (confirm)
            {
                OledMarkup = template.Markup;
                UpdatePreviewFromMarkup();
                SwitchTab("markup");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error using custom template: {ex.Message}");
        }
    }

    private async Task DeleteCustomTemplateAsync(Template template)
    {
        try
        {
            if (template == null)
            {
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Delete Template",
                $"Are you sure you want to delete the template '{template.Name}'?",
                "Delete", "Cancel");

            if (confirm)
            {
                CustomTemplates.Remove(template);

                // Update configuration
                var config = await _configService.LoadConfigAsync();

                if (config.SavedProfiles != null)
                {
                    config.SavedProfiles.RemoveAll(p => p.Name == template.Name && p.ScreenType == "OLED");
                    await _configService.SaveConfigAsync(config);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting custom template: {ex.Message}");
        }
    }
}

public class Template
{
    public Template()
    {
        // Initialize the PreviewElements collection
        PreviewElements = new List<PreviewElement>();
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public string Markup { get; set; }
    public List<PreviewElement> PreviewElements { get; set; }
    public bool IsSelected { get; set; }
}