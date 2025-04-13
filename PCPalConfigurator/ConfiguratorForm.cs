using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using LibreHardwareMonitor.Hardware;

namespace PCPalConfigurator
{
    public partial class ConfiguratorForm : Form
    {
        private ConfigData config;
        private readonly string ConfigFile = GetConfigPath();
        private Computer computer;
        private System.Windows.Forms.Timer sensorUpdateTimer;

        // Preview rendering data
        private List<PreviewElement> previewElements = new List<PreviewElement>();
        private Dictionary<string, float> sensorValues = new Dictionary<string, float>();
        private bool autoUpdatePreview = true;
        private const int OledWidth = 256;
        private const int OledHeight = 64;

        // Classes for preview rendering
        private abstract class PreviewElement
        {
            public abstract void Draw(Graphics g);
        }

        private class TextElement : PreviewElement
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Size { get; set; }
            public string Text { get; set; }

            public override void Draw(Graphics g)
            {
                // Choose font size based on the size parameter
                Font font;
                switch (Size)
                {
                    case 1: font = new Font("Consolas", 8); break;
                    case 2: font = new Font("Consolas", 10); break;
                    case 3: font = new Font("Consolas", 12); break;
                    default: font = new Font("Consolas", 8); break;
                }

                // Draw text with white color
                g.DrawString(Text, font, Brushes.White, X, Y - font.Height);
            }
        }

        private class BarElement : PreviewElement
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int Value { get; set; }

            public override void Draw(Graphics g)
            {
                // Draw outline rectangle
                g.DrawRectangle(Pens.White, X, Y, Width, Height);

                // Calculate fill width based on value (0-100)
                int fillWidth = (int)(Width * (Value / 100.0));
                if (fillWidth > 0)
                {
                    // Draw filled portion
                    g.FillRectangle(Brushes.White, X + 1, Y + 1, fillWidth - 1, Height - 2);
                }
            }
        }

        private class RectElement : PreviewElement
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public bool Filled { get; set; }

            public override void Draw(Graphics g)
            {
                if (Filled)
                {
                    // Draw filled box
                    g.FillRectangle(Brushes.White, X, Y, Width, Height);
                }
                else
                {
                    // Draw outline rectangle
                    g.DrawRectangle(Pens.White, X, Y, Width, Height);
                }
            }
        }

        private class LineElement : PreviewElement
        {
            public int X1 { get; set; }
            public int Y1 { get; set; }
            public int X2 { get; set; }
            public int Y2 { get; set; }

            public override void Draw(Graphics g)
            {
                g.DrawLine(Pens.White, X1, Y1, X2, Y2);
            }
        }

        private class IconElement : PreviewElement
        {
            public int X { get; set; }
            public int Y { get; set; }
            public string Name { get; set; }

            public override void Draw(Graphics g)
            {
                // For preview, just draw a placeholder rectangle with icon name
                g.DrawRectangle(Pens.Gray, X, Y, 24, 24);
                using (Font font = new Font("Arial", 6))
                {
                    g.DrawString(Name, font, Brushes.White, X + 2, Y + 8);
                }
            }
        }

        public ConfiguratorForm()
        {
            InitializeComponent();
            InitHardware();
            InitSensorTimer();
            LoadConfig();
            PopulateHelpContent();
            UpdatePreview(); // Initial preview rendering
        }

        private void InitHardware()
        {
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsStorageEnabled = true
            };
            computer.Open();
            PopulateSensorOptions();
            UpdateSensorValues();
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
                UpdateSensorValues();
                UpdatePreview();
            }
        }

        private void UpdateSensorValues()
        {
            // Update all hardware readings
            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();

                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.Value.HasValue)
                    {
                        string sensorId = GetSensorVariableName(hardware.Name, sensor.Name);
                        sensorValues[sensorId] = sensor.Value.Value;
                    }
                }
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
            if (File.Exists(ConfigFile))
            {
                string json = File.ReadAllText(ConfigFile);
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
            File.WriteAllText(ConfigFile, json);
            MessageBox.Show("Settings saved!");
        }

        // Renamed from btnApply_Click to btnSave_Click
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void PopulateSensorOptions()
        {
            cmbLine1.Items.Clear();
            cmbLine2.Items.Clear();

            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load ||
                        sensor.SensorType == SensorType.Temperature ||
                        sensor.SensorType == SensorType.Data ||
                        sensor.SensorType == SensorType.Fan)
                    {
                        string option = $"{hardware.HardwareType}: {sensor.Name} ({sensor.SensorType})";
                        cmbLine1.Items.Add(option);
                        cmbLine2.Items.Add(option);
                    }
                }
            }

            cmbLine1.Items.Add("Custom Text");
            cmbLine2.Items.Add("Custom Text");
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

            var sensors = new Dictionary<string, Tuple<float, string, SensorType>>();

            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.Value.HasValue &&
                        (sensor.SensorType == SensorType.Load ||
                         sensor.SensorType == SensorType.Temperature ||
                         sensor.SensorType == SensorType.Clock ||
                         sensor.SensorType == SensorType.Fan ||
                         sensor.SensorType == SensorType.Power ||
                         sensor.SensorType == SensorType.Data))
                    {
                        string sensorId = GetSensorVariableName(hardware.Name, sensor.Name);
                        sensors[sensorId] = new Tuple<float, string, SensorType>(
                            sensor.Value.Value,
                            $"{hardware.Name} {sensor.Name}",
                            sensor.SensorType);
                    }
                }
            }

            // Group sensors by type for easier reading
            var sensorGroups = sensors.GroupBy(s => s.Value.Item3);
            foreach (var group in sensorGroups)
            {
                sb.AppendLine();
                sb.AppendLine($"-- {group.Key} Sensors --");

                foreach (var sensor in group)
                {
                    string unit = GetSensorUnit(group.Key);
                    string value = FormatSensorValue(sensor.Value.Item1, group.Key);
                    sb.AppendLine($"{{{sensor.Key}}} = {value}{unit} ({sensor.Value.Item2})");
                }
            }

            txtHelpContent.Text = sb.ToString();
        }

        private string GetSensorUnit(SensorType sensorType)
        {
            return sensorType switch
            {
                SensorType.Temperature => "°C",
                SensorType.Load => "%",
                SensorType.Clock => "MHz",
                SensorType.Power => "W",
                SensorType.Fan => "RPM",
                SensorType.Flow => "L/h",
                SensorType.Control => "%",
                SensorType.Level => "%",
                _ => ""
            };
        }

        private string FormatSensorValue(float value, SensorType sensorType)
        {
            // Format different sensor types appropriately
            return sensorType switch
            {
                SensorType.Temperature => value.ToString("F1"),
                SensorType.Clock => value.ToString("F0"),
                SensorType.Load => value.ToString("F1"),
                SensorType.Fan => value.ToString("F0"),
                SensorType.Power => value.ToString("F1"),
                SensorType.Data => (value > 1024) ? (value / 1024).ToString("F1") : value.ToString("F1"),
                _ => value.ToString("F1")
            };
        }

        private string GetSensorVariableName(string hardwareName, string sensorName)
        {
            // Create a simplified and safe variable name
            string name = $"{hardwareName}_{sensorName}"
                .Replace(" ", "_")
                .Replace("%", "Percent")
                .Replace("#", "Num")
                .Replace("/", "_")
                .Replace("\\", "_")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(",", "");

            return name;
        }

        private void btnInsertIcon_Click(object sender, EventArgs e)
        {
            using IconBrowser browser = new();
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

                    // Update preview
                    if (autoUpdatePreview)
                        UpdatePreview();
                }
            }
        }

        private void btnLoadOledExample_Click(object sender, EventArgs e)
        {
            // Create an appropriate example based on the user's hardware
            string exampleMarkup = "";
            string cpuLoad = FindFirstSensorOfType(HardwareType.Cpu, SensorType.Load);
            string cpuTemp = FindFirstSensorOfType(HardwareType.Cpu, SensorType.Temperature);
            string gpuLoad = FindFirstSensorOfType(HardwareType.GpuNvidia, SensorType.Load);
            string gpuTemp = FindFirstSensorOfType(HardwareType.GpuNvidia, SensorType.Temperature);
            string ramUsed = FindFirstSensorOfType(HardwareType.Memory, SensorType.Data);

            exampleMarkup = "<text x=0 y=12 size=2>System Monitor</text>\n";

            if (!string.IsNullOrEmpty(cpuLoad) && !string.IsNullOrEmpty(cpuTemp))
            {
                exampleMarkup += $"<text x=0 y=30 size=1>CPU: {{{cpuLoad}}}% ({{{cpuTemp}}}°C)</text>\n";
                exampleMarkup += $"<bar x=0 y=35 w=128 h=6 val={{{cpuLoad}}} />\n";
            }
            else if (!string.IsNullOrEmpty(cpuLoad))
            {
                exampleMarkup += $"<text x=0 y=30 size=1>CPU: {{{cpuLoad}}}%</text>\n";
                exampleMarkup += $"<bar x=0 y=35 w=128 h=6 val={{{cpuLoad}}} />\n";
            }

            if (!string.IsNullOrEmpty(gpuLoad) && !string.IsNullOrEmpty(gpuTemp))
            {
                exampleMarkup += $"<text x=130 y=30 size=1>GPU: {{{gpuLoad}}}% ({{{gpuTemp}}}°C)</text>\n";
                exampleMarkup += $"<bar x=130 y=35 w=120 h=6 val={{{gpuLoad}}} />\n";
            }
            else if (!string.IsNullOrEmpty(gpuLoad))
            {
                exampleMarkup += $"<text x=130 y=30 size=1>GPU: {{{gpuLoad}}}%</text>\n";
                exampleMarkup += $"<bar x=130 y=35 w=120 h=6 val={{{gpuLoad}}} />\n";
            }

            if (!string.IsNullOrEmpty(ramUsed))
            {
                exampleMarkup += $"<text x=0 y=50 size=1>RAM: {{{ramUsed}}} GB</text>\n";
            }

            txtOledMarkup.Text = exampleMarkup;

            // Update preview
            if (autoUpdatePreview)
                UpdatePreview();
        }

        private string FindFirstSensorOfType(HardwareType hardwareType, SensorType sensorType)
        {
            foreach (var hardware in computer.Hardware)
            {
                if (hardware.HardwareType == hardwareType)
                {
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == sensorType && sensor.Value.HasValue)
                        {
                            return GetSensorVariableName(hardware.Name, sensor.Name);
                        }
                    }
                }
            }
            return string.Empty;
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            UpdateSensorValues();
            UpdatePreview();
        }

        private void txtOledMarkup_TextChanged(object sender, EventArgs e)
        {
            // If auto-update is enabled, update the preview when the markup changes
            if (autoUpdatePreview)
            {
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            try
            {
                // Clear existing elements
                previewElements.Clear();

                // Parse the markup into preview elements
                string markup = ProcessVariablesInMarkup(txtOledMarkup.Text);
                ParseMarkup(markup);

                // Trigger repaint of the preview panel
                if (panelOledPreview != null)
                    panelOledPreview.Invalidate();
            }
            catch (Exception ex)
            {
                // Don't show errors during preview - just skip rendering
                Console.WriteLine($"Preview error: {ex.Message}");
            }
        }

        private string ProcessVariablesInMarkup(string markup)
        {
            if (string.IsNullOrEmpty(markup))
                return string.Empty;

            // Replace variables with actual values
            foreach (var sensor in sensorValues)
            {
                // Look for {variable} syntax in the markup
                string variablePattern = $"{{{sensor.Key}}}";

                // Format value based on type (integers vs decimals)
                string formattedValue;
                if (Math.Abs(sensor.Value - Math.Round(sensor.Value)) < 0.01)
                {
                    formattedValue = $"{sensor.Value:F0}";
                }
                else
                {
                    formattedValue = $"{sensor.Value:F1}";
                }

                markup = markup.Replace(variablePattern, formattedValue);
            }

            return markup;
        }

        private void ParseMarkup(string markup)
        {
            if (string.IsNullOrEmpty(markup))
                return;

            // Parse text elements - <text x=0 y=10 size=1>Hello</text>
            foreach (Match match in Regex.Matches(markup, @"<text\s+x=(\d+)\s+y=(\d+)(?:\s+size=(\d+))?>([^<]*)</text>"))
            {
                previewElements.Add(new TextElement
                {
                    X = int.Parse(match.Groups[1].Value),
                    Y = int.Parse(match.Groups[2].Value),
                    Size = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 1,
                    Text = match.Groups[4].Value
                });
            }

            // Parse bar elements - <bar x=0 y=20 w=100 h=8 val=75 />
            foreach (Match match in Regex.Matches(markup, @"<bar\s+x=(\d+)\s+y=(\d+)\s+w=(\d+)\s+h=(\d+)\s+val=(\d+)\s*/>"))
            {
                previewElements.Add(new BarElement
                {
                    X = int.Parse(match.Groups[1].Value),
                    Y = int.Parse(match.Groups[2].Value),
                    Width = int.Parse(match.Groups[3].Value),
                    Height = int.Parse(match.Groups[4].Value),
                    Value = int.Parse(match.Groups[5].Value)
                });
            }

            // Parse rect elements - <rect x=0 y=0 w=20 h=10 />
            foreach (Match match in Regex.Matches(markup, @"<rect\s+x=(\d+)\s+y=(\d+)\s+w=(\d+)\s+h=(\d+)\s*/>"))
            {
                previewElements.Add(new RectElement
                {
                    X = int.Parse(match.Groups[1].Value),
                    Y = int.Parse(match.Groups[2].Value),
                    Width = int.Parse(match.Groups[3].Value),
                    Height = int.Parse(match.Groups[4].Value),
                    Filled = false
                });
            }

            // Parse box elements - <box x=0 y=0 w=20 h=10 />
            foreach (Match match in Regex.Matches(markup, @"<box\s+x=(\d+)\s+y=(\d+)\s+w=(\d+)\s+h=(\d+)\s*/>"))
            {
                previewElements.Add(new RectElement
                {
                    X = int.Parse(match.Groups[1].Value),
                    Y = int.Parse(match.Groups[2].Value),
                    Width = int.Parse(match.Groups[3].Value),
                    Height = int.Parse(match.Groups[4].Value),
                    Filled = true
                });
            }

            // Parse line elements - <line x1=0 y1=0 x2=20 y2=20 />
            foreach (Match match in Regex.Matches(markup, @"<line\s+x1=(\d+)\s+y1=(\d+)\s+x2=(\d+)\s+y2=(\d+)\s*/>"))
            {
                previewElements.Add(new LineElement
                {
                    X1 = int.Parse(match.Groups[1].Value),
                    Y1 = int.Parse(match.Groups[2].Value),
                    X2 = int.Parse(match.Groups[3].Value),
                    Y2 = int.Parse(match.Groups[4].Value)
                });
            }

            // Parse icon elements - <icon x=0 y=0 name=cpu />
            foreach (Match match in Regex.Matches(markup, @"<icon\s+x=(\d+)\s+y=(\d+)\s+name=([a-zA-Z0-9_]+)\s*/>"))
            {
                previewElements.Add(new IconElement
                {
                    X = int.Parse(match.Groups[1].Value),
                    Y = int.Parse(match.Groups[2].Value),
                    Name = match.Groups[3].Value
                });
            }
        }

        private void panelOledPreview_Paint(object sender, PaintEventArgs e)
        {
            // Create graphics object with smooth rendering
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw OLED display boundary
            int panelWidth = panelOledPreview.Width;
            int panelHeight = panelOledPreview.Height;

            // Calculate scale to fit preview in panel while maintaining aspect ratio
            float scaleX = (float)panelWidth / OledWidth;
            float scaleY = (float)panelHeight / OledHeight;
            float scale = Math.Min(scaleX, scaleY);

            // Calculate centered position
            int displayWidth = (int)(OledWidth * scale);
            int displayHeight = (int)(OledHeight * scale);
            int offsetX = (panelWidth - displayWidth) / 2;
            int offsetY = (panelHeight - displayHeight) / 2;

            // Draw display outline
            Rectangle displayRect = new Rectangle(offsetX, offsetY, displayWidth, displayHeight);
            e.Graphics.DrawRectangle(Pens.DarkGray, displayRect);

            // Set up transformation to scale the preview elements
            e.Graphics.TranslateTransform(offsetX, offsetY);
            e.Graphics.ScaleTransform(scale, scale);

            // Draw all elements
            foreach (var element in previewElements)
            {
                element.Draw(e.Graphics);
            }

            // Reset transformation
            e.Graphics.ResetTransform();

            // Draw labels and guidelines
            e.Graphics.DrawString($"OLED: {OledWidth}x{OledHeight}", new Font("Arial", 8), Brushes.Gray, 5, 5);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Clean up resources
            sensorUpdateTimer?.Stop();
            sensorUpdateTimer?.Dispose();
            computer?.Close();
        }
    }
}