using Microsoft.Maui.Controls.Shapes;
using PCPal.Core.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PCPal.Configurator.Controls;

// This is a new control that only handles display functionality - no editing
public class OledDisplayCanvas : GraphicsView
{
    // Bindable properties for the control
    public static readonly BindableProperty ElementsProperty = BindableProperty.Create(
        nameof(Elements),
        typeof(IList<PreviewElement>),
        typeof(OledDisplayCanvas),
        null,
        propertyChanged: OnElementsChanged);

    public static readonly BindableProperty ScaleProperty = BindableProperty.Create(
        nameof(Scale),
        typeof(float),
        typeof(OledDisplayCanvas),
        1.0f,
        propertyChanged: OnScaleChanged);

    public static readonly BindableProperty CanvasWidthProperty = BindableProperty.Create(
        nameof(CanvasWidth),
        typeof(int),
        typeof(OledDisplayCanvas),
        256);

    public static readonly BindableProperty CanvasHeightProperty = BindableProperty.Create(
        nameof(CanvasHeight),
        typeof(int),
        typeof(OledDisplayCanvas),
        64);

    // Property accessors
    public IList<PreviewElement> Elements
    {
        get => (IList<PreviewElement>)GetValue(ElementsProperty);
        set => SetValue(ElementsProperty, value);
    }

    public float Scale
    {
        get => (float)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public int CanvasWidth
    {
        get => (int)GetValue(CanvasWidthProperty);
        set => SetValue(CanvasWidthProperty, value);
    }

    public int CanvasHeight
    {
        get => (int)GetValue(CanvasHeightProperty);
        set => SetValue(CanvasHeightProperty, value);
    }

    // Constructor
    public OledDisplayCanvas()
    {
        // Set default drawing
        Drawable = new OledDisplayDrawable(this);

        // Set up initial size
        WidthRequest = 256 * Scale;
        HeightRequest = 64 * Scale;
    }

    // Element collection change handler
    private static void OnElementsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var canvas = (OledDisplayCanvas)bindable;

        // If old value is INotifyCollectionChanged, unsubscribe
        if (oldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= canvas.OnCollectionChanged;
        }

        // If new value is INotifyCollectionChanged, subscribe
        if (newValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += canvas.OnCollectionChanged;
        }

        // Invalidate the canvas to redraw
        canvas.Invalidate();
    }

    // Scale change handler
    private static void OnScaleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var canvas = (OledDisplayCanvas)bindable;
        float scale = (float)newValue;

        // Update the size of the canvas based on the scale
        canvas.WidthRequest = canvas.CanvasWidth * scale;
        canvas.HeightRequest = canvas.CanvasHeight * scale;

        canvas.Invalidate();
    }

    // Collection changed event handler
    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Invalidate();
    }
}

// The drawable that renders the OLED canvas
public class OledDisplayDrawable : IDrawable
{
    private readonly OledDisplayCanvas _canvas;

    public OledDisplayDrawable(OledDisplayCanvas canvas)
    {
        _canvas = canvas;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        try
        {
            float scale = _canvas.Scale;

            // Clear background
            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(0, 0, dirtyRect.Width, dirtyRect.Height);

            // Draw elements
            if (_canvas.Elements != null)
            {
                foreach (var element in _canvas.Elements)
                {
                    DrawElement(canvas, element, scale);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error drawing canvas: {ex.Message}");
        }
    }

    private void DrawElement(ICanvas canvas, PreviewElement element, float scale)
    {
        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 1;
        canvas.FillColor = Colors.White;

        if (element is TextElement textElement)
        {
            float fontSize;
            switch (textElement.Size)
            {
                case 1: fontSize = 8 * scale; break;
                case 2: fontSize = 12 * scale; break;
                case 3: fontSize = 16 * scale; break;
                default: fontSize = 8 * scale; break;
            }

            canvas.FontSize = fontSize;
            canvas.FontColor = Colors.White;
            canvas.DrawString(
                textElement.Text,
                textElement.X * scale,
                textElement.Y * scale,
                HorizontalAlignment.Left);
        }
        else if (element is BarElement barElement)
        {
            // Draw outline
            canvas.DrawRectangle(
                barElement.X * scale,
                barElement.Y * scale,
                barElement.Width * scale,
                barElement.Height * scale);

            // Draw fill based on value
            int fillWidth = (int)(barElement.Width * (barElement.Value / 100.0));
            if (fillWidth > 0)
            {
                canvas.FillRectangle(
                    (barElement.X + 1) * scale,
                    (barElement.Y + 1) * scale,
                    (fillWidth - 1) * scale,
                    (barElement.Height - 2) * scale);
            }
        }
        else if (element is RectElement rectElement)
        {
            if (rectElement.Filled)
            {
                // Filled box
                canvas.FillRectangle(
                    rectElement.X * scale,
                    rectElement.Y * scale,
                    rectElement.Width * scale,
                    rectElement.Height * scale);
            }
            else
            {
                // Outline rectangle
                canvas.DrawRectangle(
                    rectElement.X * scale,
                    rectElement.Y * scale,
                    rectElement.Width * scale,
                    rectElement.Height * scale);
            }
        }
        else if (element is LineElement lineElement)
        {
            canvas.DrawLine(
                lineElement.X1 * scale,
                lineElement.Y1 * scale,
                lineElement.X2 * scale,
                lineElement.Y2 * scale);
        }
        else if (element is IconElement iconElement)
        {
            // Draw a placeholder for the icon
            canvas.DrawRectangle(
                iconElement.X * scale,
                iconElement.Y * scale,
                24 * scale,
                24 * scale);

            // Draw icon name as text
            canvas.FontSize = 8 * scale;
            canvas.DrawString(
                iconElement.Name,
                (iconElement.X + 2) * scale,
                (iconElement.Y + 12) * scale,
                HorizontalAlignment.Left);
        }
    }
}