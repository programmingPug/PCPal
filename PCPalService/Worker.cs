using System;
using System.IO;
using System.IO.Ports;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PCPalService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private SerialPort serialPort;
        private Computer computer;
        private readonly string ConfigFile = GetConfigPath();
        private const string LogFile = "service_log.txt";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            computer = new Computer { IsCpuEnabled = true, IsMemoryEnabled = true };
            computer.Open();
        }

        private static string GetConfigPath()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCPal");
            Directory.CreateDirectory(folder); // Ensure folder exists
            return Path.Combine(folder, "config.json");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log("ESP32 Background Service Starting...");

            try
            {
                string portName = AutoDetectESP32();
                if (portName == null)
                {
                    Log("ESP32-C3 not found. Service stopping...");
                    throw new Exception("ESP32 not detected.");
                }

                serialPort = new SerialPort(portName, 115200);
                serialPort.Open();
                Log($"Connected to ESP32-C3 on {portName}");

                while (!stoppingToken.IsCancellationRequested)
                {
                    UpdateLCD();
                    await Task.Delay(5000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Log($"Fatal Error: {ex.Message}");
                Environment.Exit(1); // Force exit, triggering restart
            }
        }

        private string AutoDetectESP32()
        {
            foreach (string port in SerialPort.GetPortNames())
            {
                if (IsESP32Device(port)) return port;
            }
            return null;
        }

        private bool IsESP32Device(string portName)
        {
            try
            {
                using (SerialPort testPort = new SerialPort(portName, 115200))
                {
                    testPort.Open();
                    testPort.WriteLine("CMD:GET_LCD_TYPE");
                    Thread.Sleep(500);
                    string response = testPort.ReadExisting();
                    testPort.Close();
                    return response.Contains("LCD_TYPE:1602A");
                }
            }
            catch { return false; }
        }

        private void UpdateLCD()
        {
            ConfigData config = LoadConfig();
            string line1 = GetLCDContent(config.Line1Selection, config.Line1CustomText, config.Line1PostText);
            string line2 = GetLCDContent(config.Line2Selection, config.Line2CustomText, config.Line2PostText);


            SendCommand($"CMD:LCD,0,{line1}");
            SendCommand($"CMD:LCD,1,{line2}");
        }

        private string GetLCDContent(string selection, string prefix, string postfix)
        {
            var parsed = ParseSensorSelection(selection);
            if (parsed == null)
                return "N/A";

            string value = GetSensorValue(parsed.Value.hardwareType, parsed.Value.sensorName, parsed.Value.sensorType);
            return $"{prefix}{value}{postfix}";
        }


        private (HardwareType hardwareType, string sensorName, SensorType sensorType)? ParseSensorSelection(string input)
        {
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
            foreach (IHardware hardware in computer.Hardware)
            {
                if (hardware.HardwareType == type)
                {
                    hardware.Update();
                    foreach (ISensor sensor in hardware.Sensors)
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

        private ConfigData LoadConfig()
        {
            if (File.Exists(ConfigFile))
            {
                string json = File.ReadAllText(ConfigFile);
                return JsonConvert.DeserializeObject<ConfigData>(json) ?? new ConfigData();
            }
            return new ConfigData();
        }

        private void SendCommand(string command)
        {
            serialPort.WriteLine(command);
            Log($"Sent: {command}");
        }

        private void Log(string message)
        {
            string logEntry = $"{DateTime.Now}: {message}";
            File.AppendAllText(LogFile, logEntry + Environment.NewLine);
            _logger.LogInformation(message);
        }
    }

    class ConfigData
    {
        public string Line1Selection { get; set; }
        public string Line1CustomText { get; set; }
        public string Line2Selection { get; set; }
        public string Line2CustomText { get; set; }
        public string Line1PostText { get; set; }
        public string Line2PostText { get; set; }

    }
}
