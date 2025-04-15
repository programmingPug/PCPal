using System.Text.RegularExpressions;

namespace PCPal.Core.Models;

// Base class for all preview elements
public abstract class PreviewElement
{
}

// Text element for displaying text on the OLED
public class TextElement : PreviewElement
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Size { get; set; } // 1, 2, or 3 for font size
    public string Text { get; set; }
}

// Bar element for progress bars
public class BarElement : PreviewElement
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public float Value { get; set; } // 0-100
}

// Rectangle element for drawing outlines
public class RectElement : PreviewElement
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Filled { get; set; } // True for filled box, false for outline
}

// Line element for drawing lines
public class LineElement : PreviewElement
{
    public int X1 { get; set; }
    public int Y1 { get; set; }
    public int X2 { get; set; }
    public int Y2 { get; set; }
}

// Icon element for displaying icons
public class IconElement : PreviewElement
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Name { get; set; }
}

// Simple markup parser for converting markup to preview elements
public class MarkupParser
{
    private readonly Dictionary<string, float> _sensorValues;

    public MarkupParser(Dictionary<string, float> sensorValues)
    {
        _sensorValues = sensorValues ?? new Dictionary<string, float>();
    }

    public string ProcessVariables(string markup)
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
}