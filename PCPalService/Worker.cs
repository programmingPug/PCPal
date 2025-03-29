using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LibreHardwareMonitor.Hardware;

namespace PCPalService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private SerialPort serialPort;
        private Computer computer;

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
                IsMotherboardEnabled = true
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
                            _logger.LogInformation($"Connected to ESP32 on {port}");
                        }
                        else
                        {
                            _logger.LogWarning("ESP32 not found. Retrying in 5 seconds...");
                            await Task.Delay(5000, stoppingToken);
                            continue;
                        }
                    }

                    string line1 = GetSensorLine(config.Line1Selection, config.Line1CustomText, config.Line1PostText);
                    string line2 = GetSensorLine(config.Line2Selection, config.Line2CustomText, config.Line2PostText);

                    serialPort.WriteLine($"CMD:LCD,0,{line1}");
                    serialPort.WriteLine($"CMD:LCD,1,{line2}");
                    _logger.LogInformation("Data sent to LCD.");

                    await Task.Delay(5000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in main loop");
                    await Task.Delay(5000, stoppingToken);
                }
            }

            serialPort?.Close();
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
                        test.WriteLine("CMD:GET_LCD_TYPE");
                        Thread.Sleep(500);
                        string response = test.ReadExisting();
                        if (response.Contains("LCD_TYPE:1602A"))
                            return port;
                    }
                }
                catch { }
            }
            return null;
        }

        private string GetSensorLine(string selection, string prefix, string suffix)
        {
            if (selection == "Custom Text")
                return prefix;

            var parsed = ParseSensorSelection(selection);
            if (parsed == null)
                return "N/A";

            string value = GetSensorValue(parsed.Value.hardwareType, parsed.Value.sensorName, parsed.Value.sensorType);
            return $"{prefix}{value}{suffix}";
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
                    hardware.Update();
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
    }

    public class ConfigData
    {
        public string Line1Selection { get; set; }
        public string Line1CustomText { get; set; }
        public string Line1PostText { get; set; }
        public string Line2Selection { get; set; }
        public string Line2CustomText { get; set; }
        public string Line2PostText { get; set; }
        public string ScreenType { get; set; }
    }
}