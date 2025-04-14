using System.Drawing;

namespace PCPalConfigurator.Rendering
{
    /// <summary>
    /// Base class for all OLED preview elements
    /// </summary>
    public abstract class PreviewElement
    {
        /// <summary>
        /// Draws the element on the provided graphics context
        /// </summary>
        public abstract void Draw(Graphics g);
    }
}