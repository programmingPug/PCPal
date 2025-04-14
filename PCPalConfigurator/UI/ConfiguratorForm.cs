using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;

using PCPalConfigurator.Core;
using PCPalConfigurator.Rendering;

namespace PCPalConfigurator.UI
{
    public partial class ConfiguratorForm : Form
    {
        private ConfigData config;
        private readonly string configFile;
        private SensorManager sensorManager;
        private System.Windows.Forms.Timer sensorUpdateTimer;
        private MarkupParser markupParser;
        private List<PreviewElement> previewElements = new List<PreviewElement>();

        public ConfiguratorForm()
        {
            InitializeComponent();

            // Initialize the configuration path
            configFile = GetConfigPath();

            // Initialize hardware monitoring
            sensorManager = new SensorManager();
            sensorManager.UpdateSensorValues();

            // Initialize markup parser
            markupParser = new MarkupParser(sensorManager.GetAllSensorValues());

            // Setup timer for sensor updates
            InitSensorTimer();

            // Load configuration and populate UI
            LoadConfig();
            PopulateSensorOptions();
            PopulateHelpContent();

            // Initial preview update
            UpdatePreview();
        }

        private void InitSensorTimer()
        {
            // Create a timer to regularly update sensor values for the preview
            sensorUpdateTimer = new System.Windows.Forms.Timer();
            sensorUpdateTimer.Interval = 1000; // Update every second
            sensorUpdateTimer.Tick += SensorUpdateTimer_Tick;
            sensorUpdateTimer.Start();
        }

        private void SensorUpdateTimer_Tick(object sender, EventArgs e)
        {
            // Only update when the OLED tab is selected to save resources
            if (tabControl.SelectedTab == tabOLED)
            {
                sensorManager.UpdateSensorValues();
                markupParser = new MarkupParser(sensorManager.GetAllSensorValues());
                UpdatePreview();
            }
        }

        private static string GetConfigPath()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PCPal");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "config.json");
        }

        private void LoadConfig()
        {
            if (File.Exists(configFile))
            {
                string json = File.ReadAllText(configFile);
                config = JsonConvert.DeserializeObject<ConfigData>(json) ?? new ConfigData();
                ApplyConfig();
            }
            else
            {
                config = new ConfigData();
            }
        }

        private void ApplyConfig()
        {
            // LCD settings
            cmbLine1.SelectedItem = config.Line1Selection;
            txtLine1.Text = config.Line1CustomText;
            txtLine1Post.Text = config.Line1PostText;
            cmbLine2.SelectedItem = config.Line2Selection;
            txtLine2.Text = config.Line2CustomText;
            txtLine2Post.Text = config.Line2PostText;

            // OLED settings
            txtOledMarkup.Text = config.OledMarkup;

            // Select the appropriate tab based on screen type
            if (config.ScreenType == "1602")
                tabControl.SelectedTab = tab1602;
            else if (config.ScreenType == "TFT4_6")
                tabControl.SelectedTab = tabTFT;
            else if (config.ScreenType == "OLED")
                tabControl.SelectedTab = tabOLED;
        }

        private void SaveConfig()
        {
            // Determine screen type based on selected tab
            if (tabControl.SelectedTab == tab1602)
                config.ScreenType = "1602";
            else if (tabControl.SelectedTab == tabTFT)
                config.ScreenType = "TFT4_6";
            else if (tabControl.SelectedTab == tabOLED)
                config.ScreenType = "OLED";

            // Save LCD settings
            config.Line1Selection = cmbLine1.SelectedItem?.ToString();
            config.Line1CustomText = txtLine1.Text;
            config.Line1PostText = txtLine1Post.Text;
            config.Line2Selection = cmbLine2.SelectedItem?.ToString();
            config.Line2CustomText = txtLine2.Text;
            config.Line2PostText = txtLine2Post.Text;

            // Save OLED settings
            config.OledMarkup = txtOledMarkup.Text;

            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configFile, json);
            MessageBox.Show("Settings saved!");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void PopulateSensorOptions()
        {
            var options = sensorManager.GetSensorOptionsForDropdown();

            cmbLine1.Items.Clear();
            cmbLine2.Items.Clear();

            foreach (var option in options)
            {
                cmbLine1.Items.Add(option);
                cmbLine2.Items.Add(option);
            }
        }

        private void PopulateHelpContent()
        {
            // Create help content for both LCD and OLED displays
            StringBuilder sb = new StringBuilder();

            // General Information
            sb.AppendLine("PCPal Display Configurator Help");
            sb.AppendLine("==============================");
            sb.AppendLine();

            // LCD Display Help
            sb.AppendLine("LCD DISPLAY CONFIGURATION");
            sb.AppendLine("------------------------");
            sb.AppendLine("1. Select a sensor for each line or choose 'Custom Text'");
            sb.AppendLine("2. Set prefix text to appear before the sensor value");
            sb.AppendLine("3. Set suffix/units to appear after the sensor value");
            sb.AppendLine("4. Click 'Save' to apply your settings");
            sb.AppendLine();

            // OLED Display Help
            sb.AppendLine("OLED MARKUP REFERENCE");
            sb.AppendLine("---------------------");
            sb.AppendLine("The OLED display accepts markup tags to create your layout.");
            sb.AppendLine();
            sb.AppendLine("Text: <text x=0 y=10 size=1>Hello World</text>");
            sb.AppendLine("  - x, y: position coordinates (0,0 is top-left)");
            sb.AppendLine("  - size: 1-3 (small to large)");
            sb.AppendLine();
            sb.AppendLine("Progress Bar: <bar x=0 y=20 w=100 h=8 val=75 />");
            sb.AppendLine("  - x, y: position");
            sb.AppendLine("  - w, h: width and height");
            sb.AppendLine("  - val: value 0-100");
            sb.AppendLine();
            sb.AppendLine("Icon: <icon x=0 y=0 name=cpu />");
            sb.AppendLine("  - Inserts a bitmap icon from the SD card");
            sb.AppendLine("  - Use the 'Insert Icon' button to browse available icons");
            sb.AppendLine();
            sb.AppendLine("Rectangle (outline): <rect x=0 y=0 w=20 h=10 />");
            sb.AppendLine();
            sb.AppendLine("Filled Box: <box x=0 y=0 w=20 h=10 />");
            sb.AppendLine();
            sb.AppendLine("Line: <line x1=0 y1=0 x2=20 y2=20 />");
            sb.AppendLine();

            // Sensor variables
            sb.AppendLine("SENSOR VARIABLES");
            sb.AppendLine("----------------");
            sb.AppendLine("Use {SensorName} syntax to include sensor values.");
            sb.AppendLine("Example: <text x=0 y=10>CPU: {CPU_Core_i7_Total_Load}%</text>");

            // Add available sensor variables with their current values
            sb.AppendLine();
            sb.AppendLine("Available sensor variables:");

            var sensorGroups = sensorManager.GetAllSensorsGroupedByType();
            foreach (var group in sensorGroups)
            {
                sb.AppendLine();
                sb.AppendLine($"-- {group.Key} Sensors --");

                foreach (var sensor in group.Value)  // Change here - iterate over group.Value
                {
                    string unit = SensorManager.GetSensorUnit(group.Key);
                    string value = SensorManager.FormatSensorValue(sensor.Value, group.Key);
                    sb.AppendLine($"{{{sensor.Id}}} = {value}{unit} ({sensor.Name})");
                }
            }

            txtHelpContent.Text = sb.ToString();
        }

        private void btnInsertIcon_Click(object sender, EventArgs e)
        {
            using (var browser = new IconBrowser())
            {
                if (browser.ShowDialog() == DialogResult.OK)
                {
                    // Get the icon markup and insert it at the cursor position
                    string iconMarkup = browser.GetIconMarkup();
                    if (!string.IsNullOrEmpty(iconMarkup))
                    {
                        int selectionStart = txtOledMarkup.SelectionStart;
                        txtOledMarkup.Text = txtOledMarkup.Text.Insert(selectionStart, iconMarkup);
                        txtOledMarkup.SelectionStart = selectionStart + iconMarkup.Length;
                        txtOledMarkup.Focus();
                    }
                }
            }
        }

        private void btnLoadOledExample_Click(object sender, EventArgs e)
        {
            // Create an example with sensors from the current system
            string exampleMarkup = MarkupParser.CreateExampleMarkup(sensorManager);
            txtOledMarkup.Text = exampleMarkup;
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            sensorManager.UpdateSensorValues();
            markupParser = new MarkupParser(sensorManager.GetAllSensorValues());
            UpdatePreview();
        }

        private void txtOledMarkup_TextChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            try
            {
                // Parse the markup into preview elements
                previewElements = markupParser.ParseMarkup(txtOledMarkup.Text);

                // Update the preview panel
                oledPreviewPanel.SetPreviewElements(previewElements);
            }
            catch (Exception ex)
            {
                // Don't show errors during preview - just log them
                Console.WriteLine($"Preview error: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Clean up resources
            sensorUpdateTimer?.Stop();
            sensorUpdateTimer?.Dispose();
            sensorManager?.Dispose();
        }
    }
}