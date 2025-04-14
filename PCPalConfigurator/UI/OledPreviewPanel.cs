using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PCPalConfigurator.Rendering;

namespace PCPalConfigurator.UI
{
    /// <summary>
    /// Custom panel for displaying OLED preview
    /// </summary>
    public class OledPreviewPanel : Panel
    {
        private List<PreviewElement> previewElements = new List<PreviewElement>();
        private readonly OledRenderer renderer = new OledRenderer();

        public OledPreviewPanel()
        {
            this.BackColor = Color.Black;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.DoubleBuffered = true; // Reduces flicker during repaints
        }

        /// <summary>
        /// Updates the preview elements to be rendered
        /// </summary>
        public void SetPreviewElements(List<PreviewElement> elements)
        {
            previewElements = elements ?? new List<PreviewElement>();
            this.Invalidate(); // Trigger a repaint
        }

        /// <summary>
        /// Handles the paint event to render the OLED preview
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Render the preview elements
            renderer.RenderPreview(e.Graphics, previewElements, this.Width, this.Height);
        }
    }
}