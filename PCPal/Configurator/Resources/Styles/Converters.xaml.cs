using System.Globalization;

namespace PCPal.Configurator.Converters;

// Converts a boolean to a color (used for selected tabs)
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            // Return the parameter color if true (selected)
            if (parameter is Color color)
            {
                return color;
            }
            return Application.Current.Resources["Primary"] as Color ?? Colors.Transparent;
        }

        // Return transparent if false (not selected)
        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converts a boolean to a text color (used for selected tabs)
public class BoolToTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            // Return white for selected items
            if (parameter is Color color)
            {
                return color;
            }
            return Colors.White;
        }

        // Return gray for non-selected items
        return Color.FromArgb("#555555");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converts connection status to a color
public class ConnectionStatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isConnected && isConnected)
        {
            // Green for connected
            return Colors.Green;
        }

        // Red for disconnected
        return Colors.Red;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converts a string to a color based on matching
public class StringMatchConverter : IValueConverter
{
    // Default navigation item
    private const string DefaultNavItem = "1602 LCD Display";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // For matching menu items
        if (parameter is string targetValue)
        {
            string currentValue = value as string;

            // Either it's explicitly selected, or it's the default item and nothing is selected yet
            if (currentValue == targetValue || (currentValue == null && targetValue == DefaultNavItem))
            {
                return Application.Current.Resources["PrimaryLight"] as Color ?? Colors.LightBlue;
            }
        }

        // Return transparent for non-selected items
        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Inverts a boolean
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return false;
    }
}

// Converts a menu name to an icon
public class MenuIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string menuItem)
        {
            return menuItem switch
            {
                "1602 LCD Display" => "icon_lcd.png",
                "4.6\" TFT Display" => "icon_tft.png",
                "OLED Display" => "icon_oled.png",
                "Settings" => "icon_settings.png",
                "Help" => "icon_help.png",
                _ => "icon_default.png"
            };
        }

        return "icon_default.png";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converts selected item to background color
public class SelectedItemColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            if (parameter is Color color)
            {
                return color;
            }
            return Application.Current.Resources["PrimaryLight"] as Color ?? Colors.LightBlue;
        }

        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converts selected item to text color
public class SelectedTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            if (parameter is Color color)
            {
                return color;
            }
            return Application.Current.Resources["Primary"] as Color ?? Colors.Blue;
        }

        return Application.Current.Resources["TextSecondary"] as Color ?? Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Formats bytes to human-readable size
public class BytesToSizeConverter : IValueConverter
{
    private static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long size)
        {
            return BytesToString(size);
        }

        return "0 B";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static string BytesToString(long value, int decimalPlaces = 1)
    {
        if (value < 0) { return "-" + BytesToString(-value, decimalPlaces); }

        int i = 0;
        decimal dValue = value;
        while (Math.Round(dValue, decimalPlaces) >= 1000)
        {
            dValue /= 1024;
            i++;
        }

        return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
    }
}

// Converts a date/time to relative time (e.g., "just now", "5 minutes ago")
public class RelativeTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            var elapsed = DateTime.Now - dateTime;

            if (elapsed.TotalSeconds < 60)
                return "just now";
            if (elapsed.TotalMinutes < 60)
                return $"{Math.Floor(elapsed.TotalMinutes)} minutes ago";
            if (elapsed.TotalHours < 24)
                return $"{Math.Floor(elapsed.TotalHours)} hours ago";
            if (elapsed.TotalDays < 7)
                return $"{Math.Floor(elapsed.TotalDays)} days ago";

            return dateTime.ToString("g");
        }

        return "unknown time";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Add this new converter for text color specifically
public class StringMatchTextConverter : IValueConverter
{
    // Default navigation item
    private const string DefaultNavItem = "1602 LCD Display";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string targetValue)
        {
            string currentValue = value as string;

            // Either it's explicitly selected, or it's the default item and nothing is selected yet
            if (currentValue == targetValue || (currentValue == null && targetValue == DefaultNavItem))
            {
                return Application.Current.Resources["Primary"] as Color ?? Colors.Blue;
            }
        }

        // Return default text color for non-selected items
        return Application.Current.Resources["TextSecondary"] as Color ?? Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}