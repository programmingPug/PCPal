using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using LibreHardwareMonitor.Hardware;

namespace PCPal.Core.Models;

public class DisplayConfig
{
    // Common settings
    public string LastUsedPort { get; set; }
    public string ScreenType { get; set; } // "1602", "TFT4_6", or "OLED"

    // LCD-specific settings
    public string Line1Selection { get; set; }
    public string Line1CustomText { get; set; }
    public string Line2Selection { get; set; }
    public string Line2CustomText { get; set; }
    public string Line1PostText { get; set; }
    public string Line2PostText { get; set; }

    // OLED-specific settings
    public string OledMarkup { get; set; }
    public string LastIconDirectory { get; set; }

    // TFT-specific settings (placeholder for future implementation)
    public string TftLayout { get; set; }

    // A new option for storing multiple display profiles
    public List<DisplayProfile> SavedProfiles { get; set; } = new List<DisplayProfile>();
}

public class DisplayProfile
{
    public string Name { get; set; }
    public string ScreenType { get; set; }
    public string ConfigData { get; set; } // JSON serialized configuration for this profile
}

public class SensorItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string HardwareName { get; set; }
    public float Value { get; set; }
    public SensorType SensorType { get; set; }
    public string FormattedValue { get; set; }
    public string Unit { get; set; }

    public string DisplayName => $"{HardwareName} {Name}";
    public string FullValueText => $"{FormattedValue} {Unit}";
}

public class SensorGroup
{
    public string Type { get; set; }
    public string Icon { get; set; }
    public ObservableCollection<SensorItem> Sensors { get; set; }
}

public class OledElement
{
    public string Type { get; set; } // text, bar, rect, box, line, icon
    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    public int X { get; set; }
    public int Y { get; set; }

    // Helper method to generate markup
    public string ToMarkup()
    {
        switch (Type.ToLower())
        {
            case "text":
                return $"<text x={X} y={Y} size={Properties.GetValueOrDefault("size", "1")}>{Properties.GetValueOrDefault("content", "")}</text>";

            case "bar":
                return $"<bar x={X} y={Y} w={Properties.GetValueOrDefault("width", "100")} h={Properties.GetValueOrDefault("height", "8")} val={Properties.GetValueOrDefault("value", "0")} />";

            case "rect":
                return $"<rect x={X} y={Y} w={Properties.GetValueOrDefault("width", "20")} h={Properties.GetValueOrDefault("height", "10")} />";

            case "box":
                return $"<box x={X} y={Y} w={Properties.GetValueOrDefault("width", "20")} h={Properties.GetValueOrDefault("height", "10")} />";

            case "line":
                return $"<line x1={X} y1={Y} x2={Properties.GetValueOrDefault("x2", "20")} y2={Properties.GetValueOrDefault("y2", "20")} />";

            case "icon":
                return $"<icon x={X} y={Y} name={Properties.GetValueOrDefault("name", "cpu")} />";

            default:
                return "";
        }
    }

    // Static method to parse markup into element
    public static OledElement FromMarkup(string markup)
    {
        var element = new OledElement();

        // Simple parsing for demonstration - would need more robust implementation
        if (markup.StartsWith("<text"))
        {
            element.Type = "text";
            // Parse x and y attributes
            var x = Regex.Match(markup, @"x=(\d+)").Groups[1].Value;
            var y = Regex.Match(markup, @"y=(\d+)").Groups[1].Value;
            var size = Regex.Match(markup, @"size=(\d+)").Groups[1].Value;
            var content = Regex.Match(markup, @">([^<]*)</text>").Groups[1].Value;

            element.X = int.Parse(x);
            element.Y = int.Parse(y);
            element.Properties["size"] = size;
            element.Properties["content"] = content;
        }
        // Similar parsing for other element types would go here

        return element;
    }
}