using System.Drawing;

namespace PCPalConfigurator.Rendering.Elements
{
    /// <summary>
    /// Represents a line element in the OLED preview
    /// </summary>
    public class LineElement : PreviewElement
    {
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }

        public override void Draw(Graphics g)
        {
            g.DrawLine(Pens.White, X1, Y1, X2, Y2);
        }
    }
}