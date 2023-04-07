using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using Display.Data;

namespace Display.Controls;

public class CustomMediaTransportControls : MediaTransportControls
{
    public event EventHandler<RoutedEventArgs> FullWindow;
    public event EventHandler<RoutedEventArgs> LikeButtonClick;
    public event EventHandler<RoutedEventArgs> LookLaterButtonClick;
    public event EventHandler<RoutedEventArgs> ScreenShotButtonClick;

    public event SelectionChangedEventHandler QualityChanged;
    public event SelectionChangedEventHandler PlayerChanged;

    public CustomMediaTransportControls()
    {
        DefaultStyleKey = typeof(CustomMediaTransportControls);
    }

    protected override void OnApplyTemplate()
    {
        // This is where you would get your custom button and create an event handler for its click method.
        //全屏按钮
        var fullWindowButton = GetTemplateChild("FullWindowButton") as Button;
        if (fullWindowButton != null) fullWindowButton.Click += FullWindowButton_Click;

        base.OnApplyTemplate();
    }


    public void SetLike_LookLater(bool islike, bool look_later)
    {
        //显示喜欢/稍后观看
        AppBarToggleButton likeButton = GetTemplateChild("IsLikeButton") as AppBarToggleButton;
        if (likeButton != null)
        {
            likeButton.Click += LikeButton_Click;
            likeButton.IsEnabled = true;
            likeButton.IsChecked = islike;
        }

        AppBarToggleButton lookLaterButton = GetTemplateChild("LookLaterButton") as AppBarToggleButton;
        if (lookLaterButton != null)
        {
            lookLaterButton.Click += LookLaterButton_Click;
            lookLaterButton.IsEnabled = true;
            lookLaterButton.IsChecked = look_later;
        }
    }

    public void SetScreenButton()
    {
        Button screenshotButton = GetTemplateChild("ScreenshotButton") as Button;
        if (screenshotButton != null)
        {
            screenshotButton.Click += ScreenshotButton_Click;
            ;
            screenshotButton.Visibility = Visibility.Visible;
        }
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

        qualityListView.SelectedIndex = QualityItemsSource.Count switch
        {
            1 => 0,
            > 1 => 1,
            _ => qualityListView.SelectedIndex
        };

        qualityListView.SelectionChanged += QualityListView_SelectionChanged;
    }

    public void SetPlayer(List<Player> PlayerItemsSource, DataTemplate QualityDataTemplate)
    {
        Button playerButton = GetTemplateChild("PlayerButton") as Button;
        playerButton.Visibility = Visibility.Visible;

        //画质选择列表
        ListView playerListView = GetTemplateChild("PlayerListView") as ListView;
        playerListView.ItemTemplate = QualityDataTemplate;

        playerListView.ItemsSource = PlayerItemsSource;

        playerListView.SelectionChanged += PlayerListView_SelectionChanged;
    }

    private void PlayerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PlayerChanged?.Invoke(sender, e);
    }

    private void QualityListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        QualityChanged?.Invoke(sender, e);
    }

    private void FullWindowButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        FullWindow?.Invoke(sender, e);
    }

    private void LikeButton_Click(object sender, RoutedEventArgs e)
    {
        LikeButtonClick?.Invoke(sender, e);
    }
    private void LookLaterButton_Click(object sender, RoutedEventArgs e)
    {
        LookLaterButtonClick?.Invoke(sender, e);
    }
    private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
    {
        ScreenShotButtonClick?.Invoke(sender, e);
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

public class Player
{
    public WebApi.PlayMethod PlayMethod;

    public string Name => PlayMethod.ToString();

    public string Url { get; set; }
    public string PickCode { get; set; }

    public Player(WebApi.PlayMethod playerMethod, string url = null, string pickCode = null)
    {
        this.PlayMethod = playerMethod;

        if (url != null) this.Url = url;
        if (pickCode != null) this.PickCode = pickCode;

    }
}