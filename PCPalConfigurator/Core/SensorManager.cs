using System;
using System.Collections.Generic;
using LibreHardwareMonitor.Hardware;

namespace PCPalConfigurator.Core
{
    /// <summary>
    /// Manages hardware sensors and provides access to sensor data
    /// </summary>
    public class SensorManager : IDisposable
    {
        private readonly Computer computer;
        private readonly Dictionary<string, float> sensorValues = new Dictionary<string, float>();
        private bool isDisposed = false;

        public SensorManager()
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
        }

        /// <summary>
        /// Updates all sensor values from hardware
        /// </summary>
        public void UpdateSensorValues()
        {
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

        /// <summary>
        /// Gets a dictionary of all current sensor values
        /// </summary>
        public Dictionary<string, float> GetAllSensorValues()
        {
            return new Dictionary<string, float>(sensorValues);
        }

        /// <summary>
        /// Gets the value of a specific sensor
        /// </summary>
        public float? GetSensorValue(string sensorId)
        {
            if (sensorValues.TryGetValue(sensorId, out float value))
            {
                return value;
            }
            return null;
        }

        /// <summary>
        /// Finds the first available sensor of a specific type
        /// </summary>
        public string FindFirstSensorOfType(HardwareType hardwareType, SensorType sensorType)
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

        /// <summary>
        /// Creates a variable name for a sensor that's safe to use in markup
        /// </summary>
        public static string GetSensorVariableName(string hardwareName, string sensorName)
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

        /// <summary>
        /// Gets the appropriate unit for a sensor type
        /// </summary>
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
                _ => ""
            };
        }

        /// <summary>
        /// Formats a sensor value according to its type
        /// </summary>
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

        /// <summary>
        /// Gets all hardware sensors grouped by type
        /// </summary>
        public Dictionary<SensorType, List<SensorInfo>> GetAllSensorsGroupedByType()
        {
            var result = new Dictionary<SensorType, List<SensorInfo>>();

            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.Value.HasValue)
                    {
                        var sensorType = sensor.SensorType;
                        if (!result.ContainsKey(sensorType))
                        {
                            result[sensorType] = new List<SensorInfo>();
                        }

                        string sensorId = GetSensorVariableName(hardware.Name, sensor.Name);
                        result[sensorType].Add(new SensorInfo
                        {
                            Id = sensorId,
                            Name = $"{hardware.Name} {sensor.Name}",
                            Value = sensor.Value.Value,
                            SensorType = sensorType
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all hardware for populating dropdown lists
        /// </summary>
        public List<string> GetSensorOptionsForDropdown()
        {
            var options = new List<string>();

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
                        options.Add(option);
                    }
                }
            }

            options.Add("Custom Text");
            return options;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    computer.Close();
                }

                isDisposed = true;
            }
        }
    }

    /// <summary>
    /// Represents information about a sensor
    /// </summary>
    public class SensorInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public float Value { get; set; }
        public SensorType SensorType { get; set; }
    }
}