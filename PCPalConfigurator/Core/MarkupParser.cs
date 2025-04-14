using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using LibreHardwareMonitor.Hardware;
using PCPalConfigurator.Rendering;
using PCPalConfigurator.Rendering.Elements;

namespace PCPalConfigurator.Core
{
    /// <summary>
    /// Parses OLED markup into renderable elements
    /// </summary>
    public class MarkupParser
    {
        private readonly Dictionary<string, float> sensorValues;

        public MarkupParser(Dictionary<string, float> sensorValues)
        {
            this.sensorValues = sensorValues ?? new Dictionary<string, float>();
        }

        /// <summary>
        /// Processes sensor variables in markup
        /// </summary>
        public string ProcessVariables(string markup)
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

        /// <summary>
        /// Parses markup and returns a list of preview elements
        /// </summary>
        public List<PreviewElement> ParseMarkup(string markup)
        {
            var elements = new List<PreviewElement>();

            if (string.IsNullOrEmpty(markup))
                return elements;

            // Process variables in the markup first
            markup = ProcessVariables(markup);

            // Parse text elements - <text x=0 y=10 size=1>Hello</text>
            foreach (Match match in Regex.Matches(markup, @"<text\s+x=(\d+)\s+y=(\d+)(?:\s+size=(\d+))?>([^<]*)</text>"))
            {
                elements.Add(new TextElement
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
                elements.Add(new BarElement
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
                elements.Add(new RectElement
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
                elements.Add(new RectElement
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
                elements.Add(new LineElement
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
                elements.Add(new IconElement
                {
                    X = int.Parse(match.Groups[1].Value),
                    Y = int.Parse(match.Groups[2].Value),
                    Name = match.Groups[3].Value
                });
            }

            return elements;
        }

        /// <summary>
        /// Creates an example OLED markup using available sensors
        /// </summary>
        public static string CreateExampleMarkup(SensorManager sensorManager)
        {
            StringBuilder markup = new StringBuilder();

            string cpuLoad = sensorManager.FindFirstSensorOfType(HardwareType.Cpu, SensorType.Load);
            string cpuTemp = sensorManager.FindFirstSensorOfType(HardwareType.Cpu, SensorType.Temperature);
            string gpuLoad = sensorManager.FindFirstSensorOfType(HardwareType.GpuNvidia, SensorType.Load);
            string gpuTemp = sensorManager.FindFirstSensorOfType(HardwareType.GpuNvidia, SensorType.Temperature);
            string ramUsed = sensorManager.FindFirstSensorOfType(HardwareType.Memory, SensorType.Data);

            markup.AppendLine("<text x=0 y=12 size=2>System Monitor</text>");

            if (!string.IsNullOrEmpty(cpuLoad) && !string.IsNullOrEmpty(cpuTemp))
            {
                markup.AppendLine($"<text x=0 y=30 size=1>CPU: {{{cpuLoad}}}% ({{{cpuTemp}}}°C)</text>");
                markup.AppendLine($"<bar x=0 y=35 w=128 h=6 val={{{cpuLoad}}} />");
            }
            else if (!string.IsNullOrEmpty(cpuLoad))
            {
                markup.AppendLine($"<text x=0 y=30 size=1>CPU: {{{cpuLoad}}}%</text>");
                markup.AppendLine($"<bar x=0 y=35 w=128 h=6 val={{{cpuLoad}}} />");
            }

            if (!string.IsNullOrEmpty(gpuLoad) && !string.IsNullOrEmpty(gpuTemp))
            {
                markup.AppendLine($"<text x=130 y=30 size=1>GPU: {{{gpuLoad}}}% ({{{gpuTemp}}}°C)</text>");
                markup.AppendLine($"<bar x=130 y=35 w=120 h=6 val={{{gpuLoad}}} />");
            }
            else if (!string.IsNullOrEmpty(gpuLoad))
            {
                markup.AppendLine($"<text x=130 y=30 size=1>GPU: {{{gpuLoad}}}%</text>");
                markup.AppendLine($"<bar x=130 y=35 w=120 h=6 val={{{gpuLoad}}} />");
            }

            if (!string.IsNullOrEmpty(ramUsed))
            {
                markup.AppendLine($"<text x=0 y=50 size=1>RAM: {{{ramUsed}}} GB</text>");
            }

            return markup.ToString();
        }
    }
}