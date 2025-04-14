using System.Drawing;

namespace PCPalConfigurator.Rendering.Elements
{
    /// <summary>
    /// Represents a bar graph element in the OLED preview
    /// </summary>
    public class BarElement : PreviewElement
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Value { get; set; }

        public override void Draw(Graphics g)
        {
            // Draw outline rectangle
            g.DrawRectangle(Pens.White, X, Y, Width, Height);

            // Calculate fill width based on value (0-100)
            int fillWidth = (int)(Width * (Value / 100.0));
            if (fillWidth > 0)
            {
                // Draw filled portion
                g.FillRectangle(Brushes.White, X + 1, Y + 1, fillWidth - 1, Height - 2);
            }
        }
    }
}