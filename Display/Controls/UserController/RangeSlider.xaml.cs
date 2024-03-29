using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Display.Controls.UserController;

public sealed partial class RangeSlider
{
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double RangeMin
    {
        get => (double)GetValue(RangeMinProperty);
        set => SetValue(RangeMinProperty, value);
    }

    public double RangeMax
    {
        get => (double)GetValue(RangeMaxProperty);
        set => SetValue(RangeMaxProperty, value);
    }

    private static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(RangeSlider), new PropertyMetadata(0.0));

    private static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(RangeSlider), new PropertyMetadata(1.0));

    private static readonly DependencyProperty RangeMinProperty = DependencyProperty.Register(nameof(RangeMin), typeof(double), typeof(RangeSlider), new PropertyMetadata(0.0, OnRangeMinPropertyChanged));

    private static readonly DependencyProperty RangeMaxProperty = DependencyProperty.Register(nameof(RangeMax), typeof(double), typeof(RangeSlider), new PropertyMetadata(1.0, OnRangeMaxPropertyChanged));


    private RangeSlider()
    {
        InitializeComponent();
    }


    private static void OnRangeMinPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        var newValue = (double)e.NewValue;

        if (newValue < slider.Minimum)
        {
            slider.RangeMin = slider.Minimum;
        }
        else if (newValue > slider.Maximum)
        {
            slider.RangeMin = slider.Maximum;
        }
        else
        {
            slider.RangeMin = newValue;
        }

        if (slider.RangeMin > slider.RangeMax)
        {
            slider.RangeMax = slider.RangeMin;
        }

        slider.UpdateMinThumb(slider.RangeMin);
    }

    private static void OnRangeMaxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        var newValue = (double)e.NewValue;

        if (newValue < slider.Minimum)
        {
            slider.RangeMax = slider.Minimum;
        }
        else if (newValue > slider.Maximum)
        {
            slider.RangeMax = slider.Maximum;
        }
        else
        {
            slider.RangeMax = newValue;
        }

        if (slider.RangeMax < slider.RangeMin)
        {
            slider.RangeMin = slider.RangeMax;
        }

        slider.UpdateMaxThumb(slider.RangeMax);
    }

    public void UpdateMinThumb(double min, bool update = false)
    {
        if (ContainerCanvas != null)
        {
            if (update || !MinThumb.IsDragging)
            {
                var relativeLeft = ((min - Minimum) / (Maximum - Minimum)) * ContainerCanvas.ActualWidth;

                Canvas.SetLeft(MinThumb, relativeLeft);
                Canvas.SetLeft(ActiveRectangle, relativeLeft);

                ActiveRectangle.Width = (RangeMax - min) / (Maximum - Minimum) * ContainerCanvas.ActualWidth;
            }
        }
    }

    public void UpdateMaxThumb(double max, bool update = false)
    {
        if (ContainerCanvas != null)
        {
            if (update || !MaxThumb.IsDragging)
            {
                var relativeRight = (max - Minimum) / (Maximum - Minimum) * ContainerCanvas.ActualWidth;

                Canvas.SetLeft(MaxThumb, relativeRight);

                ActiveRectangle.Width = (max - RangeMin) / (Maximum - Minimum) * ContainerCanvas.ActualWidth;
            }
        }
    }

    private void ContainerCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var relativeLeft = ((RangeMin - Minimum) / (Maximum - Minimum)) * ContainerCanvas.ActualWidth;
        var relativeRight = (RangeMax - Minimum) / (Maximum - Minimum) * ContainerCanvas.ActualWidth;

        Canvas.SetLeft(MinThumb, relativeLeft);
        Canvas.SetLeft(ActiveRectangle, relativeLeft);
        Canvas.SetLeft(MaxThumb, relativeRight);

        ActiveRectangle.Width = (RangeMax - RangeMin) / (Maximum - Minimum) * ContainerCanvas.ActualWidth;
    }

    private void MinThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var min = DragThumb(MinThumb, 0, Canvas.GetLeft(MaxThumb), e.HorizontalChange);
        UpdateMinThumb(min, true);
        RangeMin = Math.Round(min);
    }

    private void MaxThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var max = DragThumb(MaxThumb, Canvas.GetLeft(MinThumb), ContainerCanvas.ActualWidth, e.HorizontalChange);
        UpdateMaxThumb(max, true);
        RangeMax = Math.Round(max);
    }

    private double DragThumb(Thumb thumb, double min, double max, double offset)
    {
        var currentPos = Canvas.GetLeft(thumb);
        var nextPos = currentPos + offset;

        nextPos = Math.Max(min, nextPos);
        nextPos = Math.Min(max, nextPos);

        return (Minimum + (nextPos / ContainerCanvas.ActualWidth) * (Maximum - Minimum));
    }

    private void MinThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        UpdateMinThumb(RangeMin);
        Canvas.SetZIndex(MinThumb, 10);
        Canvas.SetZIndex(MaxThumb, 0);
    }

    private void MaxThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        UpdateMaxThumb(RangeMax);
        Canvas.SetZIndex(MinThumb, 0);
        Canvas.SetZIndex(MaxThumb, 10);
    }
}
