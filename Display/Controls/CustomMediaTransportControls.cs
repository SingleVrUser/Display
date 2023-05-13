using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using Display.Data;
using System.Linq;
using Display.Models;

namespace Display.Controls;

public class CustomMediaTransportControls : MediaTransportControls
{
    public event EventHandler<RoutedEventArgs> FullWindow;
    public event EventHandler<RoutedEventArgs> LikeButtonClick;
    public event EventHandler<RoutedEventArgs> LookLaterButtonClick;
    public event EventHandler<RoutedEventArgs> ScreenShotButtonClick;

    public event SelectionChangedEventHandler QualityChanged;
    public event SelectionChangedEventHandler PlayerChanged;

    public event EventHandler<EventArgs> OnApplyTemplateCompleted;

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

        OnApplyTemplateCompleted?.Invoke(sender:null,e:null);
    }

    private AppBarToggleButton _likeButton;

    private AppBarToggleButton likeButton
    {
        get
        {
            if (_likeButton == null)
            {
                _likeButton = GetTemplateChild("IsLikeButton") as AppBarToggleButton;
            }

            return _likeButton;
        }
    }

    private AppBarToggleButton _lookLaterButton;

    private AppBarToggleButton lookLaterButton
    {
        get
        {
            if (_lookLaterButton == null)
            {
                _lookLaterButton = GetTemplateChild("LookLaterButton") as AppBarToggleButton;
            }

            return _lookLaterButton;
        }
    }

    private bool _isHandlerLikeLookLaterButton = false;

    public void SetLike_LookLater(bool islike, bool look_later)
    {
        //显示喜欢/稍后观看
        if (likeButton != null)
        {
            if (!_isHandlerLikeLookLaterButton)
            {
                likeButton.Click += LikeButton_Click;
                _isHandlerLikeLookLaterButton = true;
            }
            likeButton.IsEnabled = true;
            likeButton.IsChecked = islike;
        }

        if (lookLaterButton != null)
        {
            if (!_isHandlerLikeLookLaterButton)
            {
                lookLaterButton.Click += LookLaterButton_Click;
                _isHandlerLikeLookLaterButton = true;
            }

            lookLaterButton.IsEnabled = true;
            lookLaterButton.IsChecked = look_later;
        }
    }

    public void DisableLikeLookAfterButton()
    {
        if (likeButton != null)
        {
            likeButton.IsEnabled = false;
        }

        if (lookLaterButton != null)
        {
            lookLaterButton.IsEnabled = false;
        }
    }

    private bool _isHandlerScreenShotButton = false;
    public void TrySetScreenButton()
    {
        if (_isHandlerScreenShotButton) return;

        var screenShotButton = GetTemplateChild("ScreenshotButton") as Button;
        if (screenShotButton != null)
        {
            screenShotButton.Click += ScreenshotButton_Click;
            ;
            screenShotButton.Visibility = Visibility.Visible;
        }

        _isHandlerScreenShotButton = true;
    }

    public void DisableScreenButton()
    {
        var screenShotButton = GetTemplateChild("ScreenshotButton") as Button;
        if (screenShotButton != null)
        {
            screenShotButton.Visibility = Visibility.Collapsed;
        }
    }

    public void InitQuality(DataTemplate qualityDataTemplate)
    {
        //画质选择按钮
        Button qualityButton = GetTemplateChild("QualityButton") as Button;
        if (qualityButton == null) return;
        qualityButton.Visibility = Visibility.Visible;

        ListView qualityListView = GetTemplateChild("QualityListView") as ListView;
        if (qualityListView == null) return;
        qualityListView.ItemTemplate = qualityDataTemplate;

        qualityListView.SelectionChanged += QualityListView_SelectionChanged;
    }

    public void SetQualityListSource(List<Quality> qualityItemsSource,int qualityIndex)
    {
        var qualityListView = GetTemplateChild("QualityListView") as ListView;
        if (qualityListView == null) return;

        qualityListView.ItemsSource = qualityItemsSource;
        int maxIndex = qualityItemsSource.Count - 1;
        if (qualityIndex > maxIndex)
        {
            qualityIndex = maxIndex;
        }

        qualityListView.SelectedIndex = qualityIndex;
    }

    public void InitPlayer(DataTemplate qualityDataTemplate)
    {
        var playerButton = GetTemplateChild("PlayerButton") as Button;
        var playerListView = GetTemplateChild("PlayerListView") as ListView;

        if (playerButton == null || playerListView == null) return;

        // 显示
        playerButton.Visibility = Visibility.Visible;

        // 样式
        playerListView.ItemTemplate = qualityDataTemplate;

        //设置播放源
        List<Player> playerItemsSource = new() {
            new Player(WebApi.PlayMethod.vlc),
            new Player(WebApi.PlayMethod.mpv),
            new Player(WebApi.PlayMethod.pot)};

        //画质选择列表
        playerListView.ItemsSource = playerItemsSource;
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


public class Player
{
    public WebApi.PlayMethod PlayMethod;

    public string Name => PlayMethod.ToString();


    public Player(WebApi.PlayMethod playerMethod)
    {
        this.PlayMethod = playerMethod;


    }
}