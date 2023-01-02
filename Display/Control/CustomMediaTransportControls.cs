using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace Display.Control;

public class CustomMediaTransportControls : MediaTransportControls
{
    public event EventHandler<RoutedEventArgs> FullWindow;

    public event SelectionChangedEventHandler QualityChanged;

    public CustomMediaTransportControls()
    {
        DefaultStyleKey = typeof(CustomMediaTransportControls);
    }

    protected override void OnApplyTemplate()
    {
        // This is where you would get your custom button and create an event handler for its click method.
        //全屏按钮
        Button likeButton = GetTemplateChild("FullWindowButton") as Button;
        likeButton.Click += FullWindowButton_Click;

        base.OnApplyTemplate();
    }

    public void SetQuality(List<Quality> QualityItemsSource, DataTemplate QualityDataTemplate)
    {
        //画质选择按钮
        Button qualityButton = GetTemplateChild("QualityButton") as Button;
        qualityButton.Visibility = Visibility.Visible;

        //画质选择列表
        ListView qualityListView = GetTemplateChild("QualityListView") as ListView;
        qualityListView.ItemTemplate = QualityDataTemplate;

        qualityListView.ItemsSource = QualityItemsSource;

        if (QualityItemsSource.Count == 1)
        {
            qualityListView.SelectedIndex = 0;
        }
        else if (QualityItemsSource.Count > 1)
        {
            qualityListView.SelectedIndex = 1;
        }

        qualityListView.SelectionChanged += QualityListView_SelectionChanged;
    }

    private void QualityListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        QualityChanged?.Invoke(sender, e);
    }

    private void FullWindowButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        FullWindow?.Invoke(sender, e);
    }
}

public class Quality
{
    public string Name { get; set; }

    public string Url { get; set; }
    public string PickCode { get; set; }

    public Quality(string name, string url = null, string pickCode = null)
    {
        this.Name = name;

        if (url != null) this.Url = url;
        if (pickCode != null) this.PickCode = pickCode;
    }
}