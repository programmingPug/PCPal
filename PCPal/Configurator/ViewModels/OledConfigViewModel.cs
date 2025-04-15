using PCPal.Core.Models;
using PCPal.Core.Services;
using PCPal.Configurator.Views.OLED;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Diagnostics;
//using Javax.Xml.Transform;

namespace PCPal.Configurator.ViewModels;

public class OledConfigViewModel : BaseViewModel
{
    private readonly ISensorService _sensorService;
    private readonly IConfigurationService _configService;
    private readonly ISerialPortService _serialPortService;

    // Tab selection
    private bool _isVisualEditorSelected;
    private bool _isMarkupEditorSelected;
    private bool _isTemplatesSelected;
    private ContentView _currentView;

    // Markup editor data
    private string _oledMarkup;
    private List<PreviewElement> _previewElements;

    // Visual editor data
    private ObservableCollection<OledElement> _oledElements;
    private OledElement _selectedElement;
    private bool _showGridLines;
    private float _zoomLevel;
    private string _currentSensorFilter;
    private ObservableCollection<SensorItem> _filteredSensors;

    // Common properties
    private ObservableCollection<SensorItem> _availableSensors;

    // Templates properties
    private ObservableCollection<Template> _templateList;
    private Template _selectedTemplate;
    private ObservableCollection<Template> _customTemplates;
    private string _newTemplateName;

    // Views
    private readonly OledVisualEditorView _visualEditorView;
    private readonly OledMarkupEditorView _markupEditorView;
    private readonly OledTemplatesView _templatesView;

    // Timer for sensor updates
    private Timer _sensorUpdateTimer;
    private CancellationTokenSource _sensorUpdateCts;

    #region Properties

    // Tab selection properties
    public bool IsVisualEditorSelected
    {
        get => _isVisualEditorSelected;
        set => SetProperty(ref _isVisualEditorSelected, value);
    }

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

    // Visual editor properties
    public ObservableCollection<OledElement> OledElements
    {
        get => _oledElements;
        set => SetProperty(ref _oledElements, value);
    }

    public OledElement SelectedElement
    {
        get => _selectedElement;
        set
        {
            if (SetProperty(ref _selectedElement, value))
            {
                OnPropertyChanged(nameof(HasSelectedElement));
                OnPropertyChanged(nameof(IsTextElementSelected));
                OnPropertyChanged(nameof(IsBarElementSelected));
                OnPropertyChanged(nameof(IsRectangleElementSelected));
                OnPropertyChanged(nameof(IsLineElementSelected));
                OnPropertyChanged(nameof(IsIconElementSelected));

                // Update element properties
                OnPropertyChanged(nameof(SelectedElementX));
                OnPropertyChanged(nameof(SelectedElementY));
                OnPropertyChanged(nameof(SelectedElementText));
                OnPropertyChanged(nameof(SelectedElementSize));
                OnPropertyChanged(nameof(SelectedElementWidth));
                OnPropertyChanged(nameof(SelectedElementHeight));
                OnPropertyChanged(nameof(SelectedElementValue));
                OnPropertyChanged(nameof(SelectedElementX2));
                OnPropertyChanged(nameof(SelectedElementY2));
                OnPropertyChanged(nameof(SelectedElementIconName));
                OnPropertyChanged(nameof(SelectedElementSensor));
            }
        }
    }

    public bool HasSelectedElement => SelectedElement != null;
    public bool IsTextElementSelected => SelectedElement?.Type == "text";
    public bool IsBarElementSelected => SelectedElement?.Type == "bar";
    public bool IsRectangleElementSelected => SelectedElement?.Type == "rect" || SelectedElement?.Type == "box";
    public bool IsLineElementSelected => SelectedElement?.Type == "line";
    public bool IsIconElementSelected => SelectedElement?.Type == "icon";

    public bool ShowGridLines
    {
        get => _showGridLines;
        set => SetProperty(ref _showGridLines, value);
    }

    public float ZoomLevel
    {
        get => _zoomLevel;
        set => SetProperty(ref _zoomLevel, value);
    }

    public string CurrentSensorFilter
    {
        get => _currentSensorFilter;
        set
        {
            if (SetProperty(ref _currentSensorFilter, value))
            {
                ApplySensorFilter();
            }
        }
    }

    public ObservableCollection<SensorItem> FilteredSensors
    {
        get => _filteredSensors;
        set => SetProperty(ref _filteredSensors, value);
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

    // Selected element properties
    public string SelectedElementX
    {
        get => SelectedElement?.X.ToString() ?? string.Empty;
        set
        {
            if (SelectedElement != null && int.TryParse(value, out int x))
            {
                SelectedElement.X = x;
                UpdateMarkupFromElements();
                OnPropertyChanged(nameof(SelectedElementX));
            }
        }
    }

    public string SelectedElementY
    {
        get => SelectedElement?.Y.ToString() ?? string.Empty;
        set
        {
            if (SelectedElement != null && int.TryParse(value, out int y))
            {
                SelectedElement.Y = y;
                UpdateMarkupFromElements();
                OnPropertyChanged(nameof(SelectedElementY));
            }
        }
    }

    public string SelectedElementText
    {
        get => SelectedElement?.Properties.GetValueOrDefault("content") ?? string.Empty;
        set
        {
            if (SelectedElement != null)
            {
                SelectedElement.Properties["content"] = value;
                UpdateMarkupFromElements();
                OnPropertyChanged(nameof(SelectedElementText));
            }
        }
    }

    public string SelectedElementSize
    {
        get => SelectedElement?.Properties.GetValueOrDefault("size") ?? "1";
        set
        {
            if (SelectedElement != null)
            {
                SelectedElement.Properties["size"] = value;
                UpdateMarkupFromElements();
                OnPropertyChanged(nameof(SelectedElementSize));
            }
        }
    }

    public string SelectedElementWidth
    {
        get => SelectedElement?.Properties.GetValueOrDefault("width") ?? string.Empty;
        set
        {
            if (SelectedElement != null && int.TryParse(value, out int width))
            {
                SelectedElement.Properties["width"] = width.ToString();
                UpdateMarkupFromElements();
                OnPropertyChanged(nameof(SelectedElementWidth));
            }
        }
    }

    public string SelectedElementHeight
    {
        get => SelectedElement?.Properties.GetValueOrDefault("height") ?? string.Empty;
        set
        {
            if (SelectedElement != null && int.TryParse(value, out int height))
            {
                SelectedElement.Properties["height"] = height.ToString();
                UpdateMarkupFromElements();
                OnPropertyChanged(nameof(SelectedElementHeight));
            }
        }
    }

    public float SelectedElementValue
    {
        get
        {
            if (SelectedElement != null && float.TryParse(SelectedElement.Properties.GetValueOrDefault("value"), out float value))
            {
                return value;
            }
            return 0;
        }
        set
        {
            if (SelectedElement != null)
            {
                SelectedElement.Properties["value"] = value.ToString("F0");
                UpdateMarkupFromElements();
                OnPropertyChanged(nameof(SelectedElementValue));
            }
        }
    }

    public string SelectedElementX2
    {
        get => SelectedElement?.Properties.GetValueOrDefault("x2") ?? string.Empty;
        set
        {
            if (SelectedElement != null && int.TryParse(value, out int x2))
            {
                SelectedElement.Properties["x2"] = x2.ToString();
                UpdateMarkupFromElements();
                OnPropertyChanged(nameof(SelectedElementX2));
            }
        }
    }

    public string SelectedElementY2
    {
        get => SelectedElement?.Properties.GetValueOrDefault("y2") ?? string.Empty;
        set
        {
            if (SelectedElement != null && int.TryParse(value, out int y2))
            {
                SelectedElement.Properties["y2"] = y2.ToString();
                UpdateMarkupFromElements();
                OnPropertyChanged(nameof(SelectedElementY2));
            }
        }
    }

    public string SelectedElementIconName
    {
        get => SelectedElement?.Properties.GetValueOrDefault("name") ?? string.Empty;
        set
        {
            if (SelectedElement != null)
            {
                SelectedElement.Properties["name"] = value;
                UpdateMarkupFromElements();
                OnPropertyChanged(nameof(SelectedElementIconName));
            }
        }
    }

    public string SelectedElementSensor
    {
        get => SelectedElement?.Properties.GetValueOrDefault("sensor") ?? string.Empty;
        set
        {
            if (SelectedElement != null)
            {
                SelectedElement.Properties["sensor"] = value;
                UpdateMarkupFromElements();
                OnPropertyChanged(nameof(SelectedElementSensor));
            }
        }
    }

    // Lists for populating pickers
    public List<string> FontSizes => new List<string> { "1", "2", "3" };

    #endregion

    #region Commands

    // Tab selection commands
    public ICommand SwitchToVisualEditorCommand { get; }
    public ICommand SwitchToMarkupEditorCommand { get; }
    public ICommand SwitchToTemplatesCommand { get; }

    // Common commands
    public ICommand SaveConfigCommand { get; }
    public ICommand PreviewCommand { get; }
    public ICommand ResetCommand { get; }

    // Visual editor commands
    public ICommand AddElementCommand { get; }
    public ICommand DeleteElementCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand FilterSensorsCommand { get; }
    public ICommand AddSensorToDisplayCommand { get; }
    public ICommand BrowseIconsCommand { get; }

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
        _oledElements = new ObservableCollection<OledElement>();
        _previewElements = new List<PreviewElement>();
        _availableSensors = new ObservableCollection<SensorItem>();
        _filteredSensors = new ObservableCollection<SensorItem>();
        _templateList = new ObservableCollection<Template>();
        _customTemplates = new ObservableCollection<Template>();

        // Setup cancellation token source
        _sensorUpdateCts = new CancellationTokenSource();

        // Create views
        _visualEditorView = new OledVisualEditorView { BindingContext = this };
        _markupEditorView = new OledMarkupEditorView { BindingContext = this };
        _templatesView = new OledTemplatesView { BindingContext = this };

        // Default values
        _isVisualEditorSelected = true;
        _currentView = _visualEditorView;
        _showGridLines = false;
        _zoomLevel = 3.0f;
        _currentSensorFilter = "All";

        // Tab selection commands
        SwitchToVisualEditorCommand = new Command(() => SwitchTab("visual"));
        SwitchToMarkupEditorCommand = new Command(() => SwitchTab("markup"));
        SwitchToTemplatesCommand = new Command(() => SwitchTab("templates"));

        // Common commands
        SaveConfigCommand = new Command(async () => await SaveConfigAsync());
        PreviewCommand = new Command(async () => await PreviewOnDeviceAsync());
        ResetCommand = new Command(async () => await ResetLayoutAsync());

        // Visual editor commands
        AddElementCommand = new Command<string>(type => AddElement(type));
        DeleteElementCommand = new Command(DeleteSelectedElement);
        ZoomInCommand = new Command(ZoomIn);
        ZoomOutCommand = new Command(ZoomOut);
        FilterSensorsCommand = new Command<string>(filter => CurrentSensorFilter = filter);
        AddSensorToDisplayCommand = new Command<string>(sensorId => AddSensorToDisplay(sensorId));
        BrowseIconsCommand = new Command(async () => await BrowseIconsAsync());

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

            // Switch to visual editor by default
            SwitchTab("visual");

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
                FilteredSensors = new ObservableCollection<SensorItem>(sensors);
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
                await ParseMarkupToElementsAsync(OledMarkup);
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
        IsVisualEditorSelected = tab == "visual";
        IsMarkupEditorSelected = tab == "markup";
        IsTemplatesSelected = tab == "templates";

        switch (tab)
        {
            case "visual":
                CurrentView = _visualEditorView;
                break;
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

    public void UpdateMarkupFromElements()
    {
        try
        {
            var sb = new System.Text.StringBuilder();

            foreach (var element in OledElements)
            {
                sb.AppendLine(element.ToMarkup());
            }

            OledMarkup = sb.ToString();
            UpdatePreviewFromMarkup();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating markup from elements: {ex.Message}");
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

    private void ApplySensorFilter()
    {
        try
        {
            if (string.IsNullOrEmpty(CurrentSensorFilter) || CurrentSensorFilter == "All")
            {
                MainThread.BeginInvokeOnMainThread(() => {
                    FilteredSensors = new ObservableCollection<SensorItem>(AvailableSensors);
                });
                return;
            }

            var filtered = AvailableSensors.Where(s =>
                s.HardwareName.Contains(CurrentSensorFilter, StringComparison.OrdinalIgnoreCase)).ToList();

            MainThread.BeginInvokeOnMainThread(() => {
                FilteredSensors = new ObservableCollection<SensorItem>(filtered);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error applying sensor filter: {ex.Message}");
        }
    }

    private void AddElement(string type)
    {
        try
        {
            var element = new OledElement
            {
                Type = type,
                X = 10,
                Y = 10
            };

            // Set default properties based on type
            switch (type)
            {
                case "text":
                    element.Properties["size"] = "1";
                    element.Properties["content"] = "New Text";
                    break;

                case "bar":
                    element.Properties["width"] = "100";
                    element.Properties["height"] = "8";
                    element.Properties["value"] = "50";
                    break;

                case "rect":
                case "box":
                    element.Properties["width"] = "20";
                    element.Properties["height"] = "10";
                    break;

                case "line":
                    element.Properties["x2"] = "30";
                    element.Properties["y2"] = "30";
                    break;

                case "icon":
                    element.Properties["name"] = "cpu";
                    break;
            }

            OledElements.Add(element);
            SelectedElement = element;

            // Update markup
            UpdateMarkupFromElements();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error adding element: {ex.Message}");
        }
    }

    private void DeleteSelectedElement()
    {
        try
        {
            if (SelectedElement != null)
            {
                OledElements.Remove(SelectedElement);
                SelectedElement = null;

                // Update markup
                UpdateMarkupFromElements();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting element: {ex.Message}");
        }
    }

    private async Task ParseMarkupToElementsAsync(string markup)
    {
        if (string.IsNullOrEmpty(markup))
        {
            await MainThread.InvokeOnMainThreadAsync(() => {
                OledElements.Clear();
            });
            return;
        }

        var elements = new List<OledElement>();

        try
        {
            // Parse text elements
            foreach (Match match in Regex.Matches(markup, @"<text\s+x=(\d+)\s+y=(\d+)(?:\s+size=(\d+))?>([^<]*)</text>"))
            {
                var element = new OledElement { Type = "text" };
                element.X = int.Parse(match.Groups[1].Value);
                element.Y = int.Parse(match.Groups[2].Value);

                if (match.Groups[3].Success)
                {
                    element.Properties["size"] = match.Groups[3].Value;
                }
                else
                {
                    element.Properties["size"] = "1";
                }

                element.Properties["content"] = match.Groups[4].Value;
                elements.Add(element);
            }

            // Parse bar elements
            foreach (Match match in Regex.Matches(markup, @"<bar\s+x=(\d+)\s+y=(\d+)\s+w=(\d+)\s+h=(\d+)\s+val=(\d+|\{[^}]+\})\s*/>"))
            {
                var element = new OledElement { Type = "bar" };
                element.X = int.Parse(match.Groups[1].Value);
                element.Y = int.Parse(match.Groups[2].Value);
                element.Properties["width"] = match.Groups[3].Value;
                element.Properties["height"] = match.Groups[4].Value;
                element.Properties["value"] = match.Groups[5].Value;
                elements.Add(element);
            }

            // Parse rect elements
            foreach (Match match in Regex.Matches(markup, @"<rect\s+x=(\d+)\s+y=(\d+)\s+w=(\d+)\s+h=(\d+)\s*/>"))
            {
                var element = new OledElement { Type = "rect" };
                element.X = int.Parse(match.Groups[1].Value);
                element.Y = int.Parse(match.Groups[2].Value);
                element.Properties["width"] = match.Groups[3].Value;
                element.Properties["height"] = match.Groups[4].Value;
                elements.Add(element);
            }

            // Parse box elements
            foreach (Match match in Regex.Matches(markup, @"<box\s+x=(\d+)\s+y=(\d+)\s+w=(\d+)\s+h=(\d+)\s*/>"))
            {
                var element = new OledElement { Type = "box" };
                element.X = int.Parse(match.Groups[1].Value);
                element.Y = int.Parse(match.Groups[2].Value);
                element.Properties["width"] = match.Groups[3].Value;
                element.Properties["height"] = match.Groups[4].Value;
                elements.Add(element);
            }

            // Parse line elements
            foreach (Match match in Regex.Matches(markup, @"<line\s+x1=(\d+)\s+y1=(\d+)\s+x2=(\d+)\s+y2=(\d+)\s*/>"))
            {
                var element = new OledElement { Type = "line" };
                element.X = int.Parse(match.Groups[1].Value);
                element.Y = int.Parse(match.Groups[2].Value);
                element.Properties["x2"] = match.Groups[3].Value;
                element.Properties["y2"] = match.Groups[4].Value;
                elements.Add(element);
            }

            // Parse icon elements
            foreach (Match match in Regex.Matches(markup, @"<icon\s+x=(\d+)\s+y=(\d+)\s+name=([a-zA-Z0-9_]+)\s*/>"))
            {
                var element = new OledElement { Type = "icon" };
                element.X = int.Parse(match.Groups[1].Value);
                element.Y = int.Parse(match.Groups[2].Value);
                element.Properties["name"] = match.Groups[3].Value;
                elements.Add(element);
            }

            // Update the collection on the UI thread
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                OledElements.Clear();
                foreach (var element in elements)
                {
                    OledElements.Add(element);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error parsing markup: {ex.Message}");
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

    private void ZoomIn()
    {
        if (ZoomLevel < 5.0f)
        {
            ZoomLevel += 0.5f;
        }
    }

    private void ZoomOut()
    {
        if (ZoomLevel > 1.0f)
        {
            ZoomLevel -= 0.5f;
        }
    }

    private void AddSensorToDisplay(string sensorId)
    {
        try
        {
            // Find the sensor
            var sensor = AvailableSensors.FirstOrDefault(s => s.Id == sensorId);

            if (sensor == null)
            {
                return;
            }

            // Determine appropriate Y position (avoid overlap)
            int yPos = 15;
            if (OledElements.Any())
            {
                yPos = OledElements.Max(e => e.Y) + 15;
            }

            // Create text element with the sensor variable
            var element = new OledElement
            {
                Type = "text",
                X = 10,
                Y = yPos
            };

            element.Properties["size"] = "1";
            element.Properties["content"] = $"{sensor.Name}: {{{sensorId}}} {sensor.Unit}";

            OledElements.Add(element);
            SelectedElement = element;

            // Create a progress bar if it's a load/percentage sensor
            if (sensor.SensorType == LibreHardwareMonitor.Hardware.SensorType.Load ||
                sensor.SensorType == LibreHardwareMonitor.Hardware.SensorType.Level)
            {
                var barElement = new OledElement
                {
                    Type = "bar",
                    X = 10,
                    Y = yPos + 5
                };

                barElement.Properties["width"] = "100";
                barElement.Properties["height"] = "8";
                barElement.Properties["value"] = $"{{{sensorId}}}";

                OledElements.Add(barElement);
            }

            // Update markup
            UpdateMarkupFromElements();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error adding sensor to display: {ex.Message}");
        }
    }

    private async Task BrowseIconsAsync()
    {
        try
        {
            // A mock implementation - in a real app, you'd implement a proper icon browser
            var icons = new string[] { "cpu", "gpu", "ram", "disk", "network", "fan" };
            string result = await Shell.Current.DisplayActionSheet("Select an icon", "Cancel", null, icons);

            if (!string.IsNullOrEmpty(result) && result != "Cancel")
            {
                SelectedElementIconName = result;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error browsing icons: {ex.Message}");
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
            await ParseMarkupToElementsAsync(OledMarkup);
            UpdatePreviewFromMarkup();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading example markup: {ex.Message}");
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
                await ParseMarkupToElementsAsync(OledMarkup);
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
                await ParseMarkupToElementsAsync(OledMarkup);
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
    public string Name { get; set; }
    public string Description { get; set; }
    public string Markup { get; set; }
    public List<PreviewElement> PreviewElements { get; set; } = new List<PreviewElement>();
    public bool IsSelected { get; set; }
}