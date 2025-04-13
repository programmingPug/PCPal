namespace PCPalConfigurator
{
    public class ConfigData
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
    }
}