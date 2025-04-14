using System.Drawing;

namespace PCPalConfigurator.Rendering.Elements
{
    /// <summary>
    /// Represents a text element in the OLED preview
    /// </summary>
    public class TextElement : PreviewElement
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Size { get; set; }
        public string Text { get; set; }

        public override void Draw(Graphics g)
        {
            // Choose font size based on the size parameter
            Font font;
            switch (Size)
            {
                case 1: font = new Font("Consolas", 8); break;
                case 2: font = new Font("Consolas", 10); break;
                case 3: font = new Font("Consolas", 12); break;
                default: font = new Font("Consolas", 8); break;
            }

            // Draw text with white color
            g.DrawString(Text, font, Brushes.White, X, Y - font.Height);
        }
    }
}