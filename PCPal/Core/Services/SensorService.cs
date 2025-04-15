using LibreHardwareMonitor.Hardware;
using PCPal.Core.Models;
using System.Collections.ObjectModel;

namespace PCPal.Core.Services;

public interface ISensorService : IDisposable
{
    Task UpdateSensorValuesAsync();
    Dictionary<string, float> GetAllSensorValues();
    float? GetSensorValue(string sensorId);
    string FindFirstSensorOfType(HardwareType hardwareType, SensorType sensorType);
    ObservableCollection<SensorGroup> GetAllSensorsGrouped();
    string GetSensorVariableName(string hardwareName, string sensorName);
    Task<string> CreateExampleMarkupAsync();
    string ProcessVariablesInMarkup(string markup);
}

public class SensorService : ISensorService
{
    private readonly Computer _computer;
    private readonly Dictionary<string, float> _sensorValues = new();
    private bool _isDisposed = false;

    public event EventHandler SensorValuesUpdated;

    public SensorService()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsStorageEnabled = true
        };
        _computer.Open();
    }

    public async Task UpdateSensorValuesAsync()
    {
        await Task.Run(() =>
        {
            foreach (var hardware in _computer.Hardware)
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

            SensorValuesUpdated?.Invoke(this, EventArgs.Empty);
        });
    }

    public Dictionary<string, float> GetAllSensorValues()
    {
        return new Dictionary<string, float>(_sensorValues);
    }

    public float? GetSensorValue(string sensorId)
    {
        if (_sensorValues.TryGetValue(sensorId, out float value))
        {
            return value;
        }
        return null;
    }

    public string FindFirstSensorOfType(HardwareType hardwareType, SensorType sensorType)
    {
        foreach (var hardware in _computer.Hardware)
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

    public ObservableCollection<SensorGroup> GetAllSensorsGrouped()
    {
        var result = new ObservableCollection<SensorGroup>();

        // Group sensors by hardware type
        var hardwareGroups = new Dictionary<HardwareType, SensorGroup>();

        foreach (var hardware in _computer.Hardware)
        {
            hardware.Update();

            if (!hardwareGroups.ContainsKey(hardware.HardwareType))
            {
                var group = new SensorGroup
                {
                    Type = hardware.HardwareType.ToString(),
                    Icon = GetIconForHardwareType(hardware.HardwareType),
                    Sensors = new ObservableCollection<SensorItem>()
                };
                hardwareGroups[hardware.HardwareType] = group;
                result.Add(group);
            }

            var currentGroup = hardwareGroups[hardware.HardwareType];

            foreach (var sensor in hardware.Sensors)
            {
                if (sensor.Value.HasValue)
                {
                    string sensorId = GetSensorVariableName(hardware.Name, sensor.Name);
                    var sensorItem = new SensorItem
                    {
                        Id = sensorId,
                        Name = sensor.Name,
                        HardwareName = hardware.Name,
                        Value = sensor.Value.Value,
                        SensorType = sensor.SensorType,
                        FormattedValue = FormatSensorValue(sensor.Value.Value, sensor.SensorType),
                        Unit = GetSensorUnit(sensor.SensorType)
                    };

                    currentGroup.Sensors.Add(sensorItem);
                }
            }
        }

        return result;
    }

    public string GetSensorVariableName(string hardwareName, string sensorName)
    {
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

    public static string GetSensorUnit(SensorType sensorType)
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
            SensorType.Data => "GB",
            _ => ""
        };
    }

    public static string FormatSensorValue(float value, SensorType sensorType)
    {
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

    private string GetIconForHardwareType(HardwareType type)
    {
        return type switch
        {
            HardwareType.Cpu => "icon_cpu.png",
            HardwareType.GpuNvidia or HardwareType.GpuAmd => "icon_gpu.png",
            HardwareType.Memory => "icon_memory.png",
            HardwareType.Storage => "icon_storage.png",
            HardwareType.Motherboard => "icon_motherboard.png",
            HardwareType.Network => "icon_network.png",
            _ => "icon_hardware.png"
        };
    }

    public async Task<string> CreateExampleMarkupAsync()
    {
        await UpdateSensorValuesAsync();

        var sb = new System.Text.StringBuilder();

        string cpuLoad = FindFirstSensorOfType(HardwareType.Cpu, SensorType.Load);
        string cpuTemp = FindFirstSensorOfType(HardwareType.Cpu, SensorType.Temperature);
        string gpuLoad = FindFirstSensorOfType(HardwareType.GpuNvidia, SensorType.Load);
        string gpuTemp = FindFirstSensorOfType(HardwareType.GpuNvidia, SensorType.Temperature);
        string ramUsed = FindFirstSensorOfType(HardwareType.Memory, SensorType.Data);

        sb.AppendLine("<text x=0 y=12 size=2>System Monitor</text>");

        if (!string.IsNullOrEmpty(cpuLoad) && !string.IsNullOrEmpty(cpuTemp))
        {
            sb.AppendLine($"<text x=0 y=30 size=1>CPU: {{{cpuLoad}}}% ({{{cpuTemp}}}°C)</text>");
            sb.AppendLine($"<bar x=0 y=35 w=128 h=6 val={{{cpuLoad}}} />");
        }
        else if (!string.IsNullOrEmpty(cpuLoad))
        {
            sb.AppendLine($"<text x=0 y=30 size=1>CPU: {{{cpuLoad}}}%</text>");
            sb.AppendLine($"<bar x=0 y=35 w=128 h=6 val={{{cpuLoad}}} />");
        }

        if (!string.IsNullOrEmpty(gpuLoad) && !string.IsNullOrEmpty(gpuTemp))
        {
            sb.AppendLine($"<text x=130 y=30 size=1>GPU: {{{gpuLoad}}}% ({{{gpuTemp}}}°C)</text>");
            sb.AppendLine($"<bar x=130 y=35 w=120 h=6 val={{{gpuLoad}}} />");
        }
        else if (!string.IsNullOrEmpty(gpuLoad))
        {
            sb.AppendLine($"<text x=130 y=30 size=1>GPU: {{{gpuLoad}}}%</text>");
            sb.AppendLine($"<bar x=130 y=35 w=120 h=6 val={{{gpuLoad}}} />");
        }

        if (!string.IsNullOrEmpty(ramUsed))
        {
            sb.AppendLine($"<text x=0 y=50 size=1>RAM: {{{ramUsed}}} GB</text>");
        }

        return sb.ToString();
    }

    public string ProcessVariablesInMarkup(string markup)
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _computer.Close();
            }

            _isDisposed = true;
        }
    }
}