using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using LibreHardwareMonitor.Hardware;

namespace PCPalService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private SerialPort serialPort;
        private Computer computer;
        private readonly Dictionary<string, float> _sensorValues = new();

        private const string AppDataFolder = "PCPal";
        private const string ConfigFileName = "config.json";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service starting...");
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsMemoryEnabled = true,
                IsGpuEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsStorageEnabled = true
            };
            computer.Open();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    string configPath = GetConfigPath();
                    if (!File.Exists(configPath))
                    {
                        _logger.LogWarning("Config file not found. Retrying in 5 seconds...");
                        await Task.Delay(5000, stoppingToken);
                        continue;
                    }

                    var config = LoadConfig(configPath);

                    if (serialPort == null || !serialPort.IsOpen)
                    {
                        string port = AutoDetectESP32();
                        if (port != null)
                        {
                            serialPort = new SerialPort(port, 115200);
                            serialPort.Open();
                            _logger.LogInformation($"Connected to display device on {port}");
                        }
                        else
                        {
                            _logger.LogWarning("Display device not found. Retrying in 5 seconds...");
                            await Task.Delay(5000, stoppingToken);
                            continue;
                        }
                    }

                    // Update sensor values
                    UpdateSensorValues();

                    // Send appropriate commands based on screen type
                    if (config.ScreenType == "1602" || config.ScreenType == "TFT4_6")
                    {
                        // LCD display handling
                        string line1 = GetSensorLine(config.Line1Selection, config.Line1CustomText, config.Line1PostText);
                        string line2 = GetSensorLine(config.Line2Selection, config.Line2CustomText, config.Line2PostText);

                        serialPort.WriteLine($"CMD:LCD,0,{line1}");
                        serialPort.WriteLine($"CMD:LCD,1,{line2}");
                        _logger.LogDebug("Data sent to LCD.");
                    }
                    else if (config.ScreenType == "OLED")
                    {
                        // OLED display handling
                        string processedMarkup = ProcessVariablesInMarkup(config.OledMarkup);
                        serialPort.WriteLine($"CMD:OLED,{processedMarkup}");
                        _logger.LogDebug("Data sent to OLED.");
                    }
                    else
                    {
                        _logger.LogWarning($"Unknown screen type: {config.ScreenType}");
                    }

                    await Task.Delay(5000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in main loop");
                    DisconnectFromDevice();
                    await Task.Delay(5000, stoppingToken);
                }
            }

            DisconnectFromDevice();
            computer.Close();
            _logger.LogInformation("Service shutting down...");
        }

        private void DisconnectFromDevice()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    serialPort.Close();
                    serialPort.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disconnecting from device");
                }
                finally
                {
                    serialPort = null;
                }
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
                        _sensorValues[sensorId] = sensor.Value.Value;
                    }
                }
            }
        }

        private string GetConfigPath()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppDataFolder);
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, ConfigFileName);
        }

        private ConfigData LoadConfig(string path)
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ConfigData>(json) ?? new ConfigData();
        }

        private string AutoDetectESP32()
        {
            foreach (string port in SerialPort.GetPortNames())
            {
                try
                {
                    using (SerialPort test = new SerialPort(port, 115200))
                    {
                        test.Open();
                        test.WriteLine("CMD:GET_DISPLAY_TYPE");
                        Thread.Sleep(500);
                        string response = test.ReadExisting();

                        if (response.Contains("DISPLAY_TYPE:1602A") ||
                            response.Contains("DISPLAY_TYPE:OLED"))
                        {
                            return port;
                        }
                    }
                }
                catch { }
            }
            return null;
        }

        private string GetSensorLine(string selection, string prefix, string suffix)
        {
            if (selection == "Custom Text" || string.IsNullOrEmpty(selection))
                return prefix ?? string.Empty;

            var parsed = ParseSensorSelection(selection);
            if (parsed == null)
                return "N/A";

            string value = GetSensorValue(parsed.Value.hardwareType, parsed.Value.sensorName, parsed.Value.sensorType);
            return $"{prefix ?? ""}{value}{suffix ?? ""}";
        }

        private (HardwareType hardwareType, string sensorName, SensorType sensorType)? ParseSensorSelection(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            try
            {
                var parts = input.Split(':', 2);
                if (parts.Length != 2) return null;

                var hardwareType = Enum.Parse<HardwareType>(parts[0].Trim());
                var sensorInfo = parts[1].Trim();
                int idx = sensorInfo.LastIndexOf('(');
                if (idx == -1) return null;

                string name = sensorInfo[..idx].Trim();
                string typeStr = sensorInfo[(idx + 1)..].Trim(' ', ')');
                var sensorType = Enum.Parse<SensorType>(typeStr);

                return (hardwareType, name, sensorType);
            }
            catch
            {
                return null;
            }
        }

        private string GetSensorValue(HardwareType type, string name, SensorType sensorType)
        {
            foreach (var hardware in computer.Hardware)
            {
                if (hardware.HardwareType == type)
                {
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == sensorType && sensor.Name == name)
                        {
                            return sensor.Value?.ToString("0.0") ?? "N/A";
                        }
                    }
                }
            }
            return "N/A";
        }

        private string ProcessVariablesInMarkup(string markup)
        {
            if (string.IsNullOrEmpty(markup))
                return string.Empty;

            // Replace variables with actual values
            foreach (var sensor in _sensorValues)
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
    }

    public class ConfigData
    {
        // Existing properties
        public string LastUsedPort { get; set; }
        public string ScreenType { get; set; } // Could be "1602", "TFT4_6", or "OLED"
        public string Line1Selection { get; set; }
        public string Line1CustomText { get; set; }
        public string Line2Selection { get; set; }
        public string Line2CustomText { get; set; }
        public string Line1PostText { get; set; }
        public string Line2PostText { get; set; }

        // New properties for OLED support
        public string OledMarkup { get; set; } // Store the full markup for OLED
        public string LastIconDirectory { get; set; } // Remember last used icon directory
    }
}