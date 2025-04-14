using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PCPalConfigurator.Rendering
{
    /// <summary>
    /// Renders OLED preview elements to a graphics context
    /// </summary>
    public class OledRenderer
    {
        private const int OledWidth = 256;
        private const int OledHeight = 64;

        /// <summary>
        /// Renders a list of preview elements to a graphics context
        /// </summary>
        public void RenderPreview(Graphics g, List<PreviewElement> elements, int panelWidth, int panelHeight)
        {
            // Create graphics object with smooth rendering
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Calculate scale to fit preview in panel while maintaining aspect ratio
            float scaleX = (float)panelWidth / OledWidth;
            float scaleY = (float)panelHeight / OledHeight;
            float scale = Math.Min(scaleX, scaleY);

            // Calculate centered position
            int displayWidth = (int)(OledWidth * scale);
            int displayHeight = (int)(OledHeight * scale);
            int offsetX = (panelWidth - displayWidth) / 2;
            int offsetY = (panelHeight - displayHeight) / 2;

            // Draw display outline
            Rectangle displayRect = new Rectangle(offsetX, offsetY, displayWidth, displayHeight);
            g.DrawRectangle(Pens.DarkGray, displayRect);

            // Set up transformation to scale the preview elements
            g.TranslateTransform(offsetX, offsetY);
            g.ScaleTransform(scale, scale);

            // Draw all elements
            foreach (var element in elements)
            {
                element.Draw(g);
            }

            // Reset transformation
            g.ResetTransform();

            // Draw labels and guidelines
            g.DrawString($"OLED: {OledWidth}x{OledHeight}", new Font("Arial", 8), Brushes.Gray, 5, 5);
        }

        /// <summary>
        /// Gets the OLED display dimensions
        /// </summary>
        public static (int Width, int Height) GetDisplayDimensions()
        {
            return (OledWidth, OledHeight);
        }
    }
}