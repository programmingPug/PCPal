using System.Drawing;

namespace PCPalConfigurator.Rendering.Elements
{
    /// <summary>
    /// Represents a rectangle or box element in the OLED preview
    /// </summary>
    public class RectElement : PreviewElement
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Filled { get; set; }

        public override void Draw(Graphics g)
        {
            if (Filled)
            {
                // Draw filled box
                g.FillRectangle(Brushes.White, X, Y, Width, Height);
            }
            else
            {
                // Draw outline rectangle
                g.DrawRectangle(Pens.White, X, Y, Width, Height);
            }
        }
    }
}