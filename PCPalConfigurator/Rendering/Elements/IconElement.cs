using System.Drawing;

namespace PCPalConfigurator.Rendering.Elements
{
    /// <summary>
    /// Represents an icon element in the OLED preview
    /// </summary>
    public class IconElement : PreviewElement
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; }

        public override void Draw(Graphics g)
        {
            // For preview, just draw a placeholder rectangle with icon name
            g.DrawRectangle(Pens.Gray, X, Y, 24, 24);
            using (Font font = new Font("Arial", 6))
            {
                g.DrawString(Name, font, Brushes.White, X + 2, Y + 8);
            }
        }
    }
}