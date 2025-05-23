﻿using Microsoft.Maui.Controls.Shapes;
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