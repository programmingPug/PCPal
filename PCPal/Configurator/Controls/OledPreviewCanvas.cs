using Microsoft.Maui.Controls.Shapes;
using PCPal.Configurator.ViewModels;
using PCPal.Core.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PCPal.Configurator.Controls;

public class OledPreviewCanvas : GraphicsView
{
    // Bindable properties for the control
    public static readonly BindableProperty ElementsProperty = BindableProperty.Create(
        nameof(Elements),
        typeof(IList<PreviewElement>),
        typeof(OledPreviewCanvas),
        null,
        propertyChanged: OnElementsChanged);

    public static readonly BindableProperty SelectedElementProperty = BindableProperty.Create(
        nameof(SelectedElement),
        typeof(OledElement),
        typeof(OledPreviewCanvas),
        null,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedElementChanged);

    public static readonly BindableProperty IsEditableProperty = BindableProperty.Create(
        nameof(IsEditable),
        typeof(bool),
        typeof(OledPreviewCanvas),
        false);

    public static readonly BindableProperty ScaleProperty = BindableProperty.Create(
        nameof(Scale),
        typeof(float),
        typeof(OledPreviewCanvas),
        1.0f,
        propertyChanged: OnScaleChanged);

    public static readonly BindableProperty WidthProperty = BindableProperty.Create(
        nameof(Width),
        typeof(int),
        typeof(OledPreviewCanvas),
        256);

    public static readonly BindableProperty HeightProperty = BindableProperty.Create(
        nameof(Height),
        typeof(int),
        typeof(OledPreviewCanvas),
        64);

    // Property accessors
    public IList<PreviewElement> Elements
    {
        get => (IList<PreviewElement>)GetValue(ElementsProperty);
        set => SetValue(ElementsProperty, value);
    }

    public OledElement SelectedElement
    {
        get => (OledElement)GetValue(SelectedElementProperty);
        set => SetValue(SelectedElementProperty, value);
    }

    public bool IsEditable
    {
        get => (bool)GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
    }

    public float Scale
    {
        get => (float)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public new int Width
    {
        get => (int)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    public new int Height
    {
        get => (int)GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    // Constructor
    public OledPreviewCanvas()
    {
        // Set default drawing
        Drawable = new OledCanvasDrawable(this);

        // Set up interaction handlers if editable
        StartInteraction += OnStartInteraction;
        DragInteraction += OnDragInteraction;
        EndInteraction += OnEndInteraction;

        // Set up initial size
        WidthRequest = 256 * Scale;
        HeightRequest = 64 * Scale;
    }

    // Element collection change handler
    private static void OnElementsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var canvas = (OledPreviewCanvas)bindable;

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

    // Selected element change handler
    private static void OnSelectedElementChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var canvas = (OledPreviewCanvas)bindable;
        canvas.Invalidate();
    }

    // Scale change handler
    private static void OnScaleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var canvas = (OledPreviewCanvas)bindable;
        float scale = (float)newValue;

        // Update the size of the canvas based on the scale
        canvas.WidthRequest = canvas.Width * scale;
        canvas.HeightRequest = canvas.Height * scale;

        canvas.Invalidate();
    }

    // Collection changed event handler
    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Invalidate();
    }

    // Interaction handlers for element selection and manipulation
    private OledElement draggedElement;
    private Point dragStartPoint;

    private void OnStartInteraction(object sender, TouchEventArgs e)
    {
        if (!IsEditable) return;

        var point = e.Touches[0];
        dragStartPoint = point;

        // Check if an element was clicked
        if (Elements == null || Elements.Count == 0) return;

        // Need to adjust for scale
        float x = (float)point.X / Scale;
        float y = (float)point.Y / Scale;

        // Check in reverse order (top elements first)
        for (int i = Elements.Count - 1; i >= 0; i--)
        {
            var element = Elements[i];

            if (element is TextElement textElement)
            {
                // Simple bounding box check
                if (x >= textElement.X && x <= textElement.X + 100 &&
                    y >= textElement.Y - 20 && y <= textElement.Y)
                {
                    // Find the OledElement that corresponds to this PreviewElement
                    var oledElement = FindOledElementForPreviewElement(textElement);
                    if (oledElement != null)
                    {
                        draggedElement = oledElement;
                        SelectedElement = oledElement;
                        return;
                    }
                }
            }
            else if (element is BarElement barElement)
            {
                if (x >= barElement.X && x <= barElement.X + barElement.Width &&
                    y >= barElement.Y && y <= barElement.Y + barElement.Height)
                {
                    var oledElement = FindOledElementForPreviewElement(barElement);
                    if (oledElement != null)
                    {
                        draggedElement = oledElement;
                        SelectedElement = oledElement;
                        return;
                    }
                }
            }
            else if (element is RectElement rectElement)
            {
                if (x >= rectElement.X && x <= rectElement.X + rectElement.Width &&
                    y >= rectElement.Y && y <= rectElement.Y + rectElement.Height)
                {
                    var oledElement = FindOledElementForPreviewElement(rectElement);
                    if (oledElement != null)
                    {
                        draggedElement = oledElement;
                        SelectedElement = oledElement;
                        return;
                    }
                }
            }
            else if (element is LineElement lineElement)
            {
                // Simplified line hit detection
                float distance = DistancePointToLine(
                    x, y,
                    lineElement.X1, lineElement.Y1,
                    lineElement.X2, lineElement.Y2);

                if (distance < 10) // 10 pixel tolerance
                {
                    var oledElement = FindOledElementForPreviewElement(lineElement);
                    if (oledElement != null)
                    {
                        draggedElement = oledElement;
                        SelectedElement = oledElement;
                        return;
                    }
                }
            }
            else if (element is IconElement iconElement)
            {
                if (x >= iconElement.X && x <= iconElement.X + 24 &&
                    y >= iconElement.Y && y <= iconElement.Y + 24)
                {
                    var oledElement = FindOledElementForPreviewElement(iconElement);
                    if (oledElement != null)
                    {
                        draggedElement = oledElement;
                        SelectedElement = oledElement;
                        return;
                    }
                }
            }
        }

        // No element was clicked, deselect
        SelectedElement = null;
    }

    private void OnDragInteraction(object sender, TouchEventArgs e)
    {
        if (!IsEditable || draggedElement == null) return;

        try
        {
            var point = e.Touches[0];

            // Calculate the delta from the start point
            float deltaX = (float)(point.X - dragStartPoint.X) / Scale;
            float deltaY = (float)(point.Y - dragStartPoint.Y) / Scale;

            // Update the position of the dragged element
            draggedElement.X += (int)deltaX;
            draggedElement.Y += (int)deltaY;

            // Keep element within bounds
            draggedElement.X = Math.Max(0, Math.Min(Width - 10, draggedElement.X));
            draggedElement.Y = Math.Max(0, Math.Min(Height - 10, draggedElement.Y));

            // Update the start point for the next move
            dragStartPoint = point;

            // Notify property changes
            var viewModel = BindingContext as PCPal.Configurator.ViewModels.OledConfigViewModel;
            if (viewModel != null)
            {
                // Update the view model properties to reflect the new position
                viewModel.OnPropertyChanged(nameof(viewModel.SelectedElementX));
                viewModel.OnPropertyChanged(nameof(viewModel.SelectedElementY));

                // Update the markup
                viewModel.UpdateMarkupFromElements();
            }

            // Invalidate the canvas to redraw
            Invalidate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in drag interaction: {ex.Message}");
        }
    }

    private void OnEndInteraction(object sender, TouchEventArgs e)
    {
        draggedElement = null;
    }

    // Helper methods
    private OledElement FindOledElementForPreviewElement(PreviewElement previewElement)
    {
        var viewModel = BindingContext as OledConfigViewModel;
        if (viewModel == null || viewModel.OledElements == null) return null;

        foreach (var oledElement in viewModel.OledElements)
        {
            // Match based on position and type
            if (previewElement is TextElement textElement && oledElement.Type == "text")
            {
                if (oledElement.X == textElement.X && oledElement.Y == textElement.Y)
                {
                    return oledElement;
                }
            }
            else if (previewElement is BarElement barElement && oledElement.Type == "bar")
            {
                if (oledElement.X == barElement.X && oledElement.Y == barElement.Y)
                {
                    return oledElement;
                }
            }
            else if (previewElement is RectElement rectElement)
            {
                if ((oledElement.Type == "rect" || oledElement.Type == "box") &&
                    oledElement.X == rectElement.X && oledElement.Y == rectElement.Y)
                {
                    return oledElement;
                }
            }
            else if (previewElement is LineElement lineElement && oledElement.Type == "line")
            {
                if (oledElement.X == lineElement.X1 && oledElement.Y == lineElement.Y1)
                {
                    return oledElement;
                }
            }
            else if (previewElement is IconElement iconElement && oledElement.Type == "icon")
            {
                if (oledElement.X == iconElement.X && oledElement.Y == iconElement.Y)
                {
                    return oledElement;
                }
            }
        }

        return null;
    }

    private float DistancePointToLine(float px, float py, float x1, float y1, float x2, float y2)
    {
        float lineLength = (float)Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        if (lineLength == 0) return (float)Math.Sqrt(Math.Pow(px - x1, 2) + Math.Pow(py - y1, 2));

        float t = ((px - x1) * (x2 - x1) + (py - y1) * (y2 - y1)) / (lineLength * lineLength);
        t = Math.Max(0, Math.Min(1, t));

        float projX = x1 + t * (x2 - x1);
        float projY = y1 + t * (y2 - y1);

        return (float)Math.Sqrt(Math.Pow(px - projX, 2) + Math.Pow(py - projY, 2));
    }
}

// The drawable that renders the OLED canvas
public class OledCanvasDrawable : IDrawable
{
    private readonly OledPreviewCanvas _canvas;

    public OledCanvasDrawable(OledPreviewCanvas canvas)
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

            // Draw grid if requested
            if (_canvas.IsEditable && _canvas.Parent?.BindingContext is OledConfigViewModel viewModel && viewModel.ShowGridLines)
            {
                canvas.StrokeColor = new Color(64, 64, 64, 64); // Semi-transparent gray
                canvas.StrokeSize = 1;

                // Draw vertical grid lines
                for (int x = 0; x <= _canvas.Width; x += 10)
                {
                    canvas.DrawLine(x * scale, 0, x * scale, _canvas.Height * scale);
                }

                // Draw horizontal grid lines
                for (int y = 0; y <= _canvas.Height; y += 10)
                {
                    canvas.DrawLine(0, y * scale, _canvas.Width * scale, y * scale);
                }
            }

            // Draw elements
            if (_canvas.Elements != null)
            {
                foreach (var element in _canvas.Elements)
                {
                    // Check if this element is selected
                    bool isSelected = false;
                    if (_canvas.SelectedElement != null && _canvas.IsEditable)
                    {
                        if (element is TextElement textElement && _canvas.SelectedElement.Type == "text")
                        {
                            isSelected = _canvas.SelectedElement.X == textElement.X && _canvas.SelectedElement.Y == textElement.Y;
                        }
                        else if (element is BarElement barElement && _canvas.SelectedElement.Type == "bar")
                        {
                            isSelected = _canvas.SelectedElement.X == barElement.X && _canvas.SelectedElement.Y == barElement.Y;
                        }
                        else if (element is RectElement rectElement &&
                                 (_canvas.SelectedElement.Type == "rect" || _canvas.SelectedElement.Type == "box"))
                        {
                            isSelected = _canvas.SelectedElement.X == rectElement.X && _canvas.SelectedElement.Y == rectElement.Y;
                        }
                        else if (element is LineElement lineElement && _canvas.SelectedElement.Type == "line")
                        {
                            isSelected = _canvas.SelectedElement.X == lineElement.X1 && _canvas.SelectedElement.Y == lineElement.Y1;
                        }
                        else if (element is IconElement iconElement && _canvas.SelectedElement.Type == "icon")
                        {
                            isSelected = _canvas.SelectedElement.X == iconElement.X && _canvas.SelectedElement.Y == iconElement.Y;
                        }
                    }

                    // Draw the element with appropriate styling
                    DrawElement(canvas, element, scale, isSelected);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error drawing canvas: {ex.Message}");
        }
    }

    private void DrawElement(ICanvas canvas, PreviewElement element, float scale, bool isSelected)
    {
        // Set selection highlighting if needed
        if (isSelected)
        {
            canvas.StrokeColor = Colors.Cyan;
            canvas.StrokeSize = 2;
        }
        else
        {
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 1;
        }

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

            // Draw selection indicator for text elements
            if (isSelected)
            {
                var metrics = canvas.GetStringSize(textElement.Text, Microsoft.Maui.Graphics.Font.Default, fontSize);
                canvas.DrawRectangle(
                    textElement.X * scale - 2,
                    textElement.Y * scale - metrics.Height - 2,
                    metrics.Width + 4,
                    metrics.Height + 4);
            }
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

            // Draw selection indicator
            if (isSelected)
            {
                canvas.StrokeColor = Colors.Cyan;
                canvas.DrawRectangle(
                    (barElement.X - 2) * scale,
                    (barElement.Y - 2) * scale,
                    (barElement.Width + 4) * scale,
                    (barElement.Height + 4) * scale);
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

            // Draw selection indicator
            if (isSelected)
            {
                canvas.StrokeColor = Colors.Cyan;
                canvas.DrawRectangle(
                    (rectElement.X - 2) * scale,
                    (rectElement.Y - 2) * scale,
                    (rectElement.Width + 4) * scale,
                    (rectElement.Height + 4) * scale);
            }
        }
        else if (element is LineElement lineElement)
        {
            canvas.DrawLine(
                lineElement.X1 * scale,
                lineElement.Y1 * scale,
                lineElement.X2 * scale,
                lineElement.Y2 * scale);

            // Draw selection indicator
            if (isSelected)
            {
                canvas.StrokeColor = Colors.Cyan;
                canvas.StrokeSize = 3;
                canvas.DrawLine(
                    lineElement.X1 * scale,
                    lineElement.Y1 * scale,
                    lineElement.X2 * scale,
                    lineElement.Y2 * scale);

                // Draw endpoints
                canvas.FillCircle(lineElement.X1 * scale, lineElement.Y1 * scale, 4);
                canvas.FillCircle(lineElement.X2 * scale, lineElement.Y2 * scale, 4);
            }
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

            // Draw selection indicator
            if (isSelected)
            {
                canvas.StrokeColor = Colors.Cyan;
                canvas.DrawRectangle(
                    (iconElement.X - 2) * scale,
                    (iconElement.Y - 2) * scale,
                    (24 + 4) * scale,
                    (24 + 4) * scale);
            }
        }
    }
}