using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Display.Models.Data;
using Display.Models.Data.Enums;
using Display.Models.Media;
using static Display.Controls.CustomMediaPlayerElement;

namespace Display.Controls;

public class CustomMediaTransportControls : MediaTransportControls
{
    public event EventHandler<RoutedEventArgs> FullWindow;
    public event EventHandler<RoutedEventArgs> LikeButtonClick;
    public event EventHandler<RoutedEventArgs> LookLaterButtonClick;
    public event EventHandler<RoutedEventArgs> ScreenShotButtonClick;
    public event EventHandler<RoutedEventArgs> RightButtonClick;

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

    private AppBarToggleButton LikeButton
    {
        get
        {
            if (_likeButton == null)
            {
                _likeButton = GetTemplateChild("LikeButton") as AppBarToggleButton;
            }

            return _likeButton;
        }
    }

    private AppBarToggleButton _lookLaterButton;

    private AppBarToggleButton LookLaterButton
    {
        get
        {
            if (_lookLaterButton == null)
            {
                _lookLaterButton = GetTemplateChild("LookAfterButton") as AppBarToggleButton;
            }

            return _lookLaterButton;
        }
    }

    private AppBarButton _screenShotButton;

    private AppBarButton ScreenShotButton
    {
        get
        {
            if (_screenShotButton == null)
            {
                _screenShotButton = GetTemplateChild("ScreenShotButton") as AppBarButton;
            }

            return _screenShotButton;
        }
    }

    private Button _rightButton;

    private Button RightButton
    {
        get
        {
            if (_rightButton == null)
            {
                _rightButton = GetTemplateChild("RightButton") as Button;
            }

            return _rightButton;
        }
    }


    private TextBlock _titleTextBlock;

    private TextBlock TitleTextBlock
    {
        get
        {
            if (_titleTextBlock == null)
            {
                _titleTextBlock = GetTemplateChild("TitleTextBlock") as TextBlock;
            }

            return _titleTextBlock;
        }
    }


    private ListView _qualityListView;

    private ListView QualityListView
    {
        get
        {
            if (_qualityListView == null)
            {
                _qualityListView = GetTemplateChild("QualityListView") as ListView;
            }

            return _qualityListView;
        }
    }

    private bool _isHandlerLikeAndLookLaterButton;
    private bool _isHandlerScreenShowButton;

    public void SetRightButton()
    {
        if (RightButton == null) return;

        RightButton.Visibility = Visibility.Visible;

        RightButton.Click += RightButton_Click;
    }

    public void SetLike_LookLater(bool isLike, bool lookLater)
    {
        if (LikeButton == null || LookLaterButton == null) return;

        //显示喜欢/稍后观看
        LikeButton.IsEnabled = true;
        LikeButton.IsChecked = isLike;
        
        LookLaterButton.IsEnabled = true;
        LookLaterButton.IsChecked = lookLater;

        if (_isHandlerLikeAndLookLaterButton) return;

        LikeButton.Click += LikeButton_Click;
        LookLaterButton.Click += LookLaterButton_Click;

        _isHandlerLikeAndLookLaterButton = true;
    }

    public void SetTitle(string title)
    {
        if (TitleTextBlock == null) return;
        TitleTextBlock.Text = title;

    }

    public void DisableLikeLookAfterButton()
    {
        if (LikeButton == null || LookLaterButton == null) return;

        LikeButton.IsEnabled = false;
        LookLaterButton.IsEnabled = false;
    }


    public void TrySetScreenButton()
    {
        if (ScreenShotButton == null) return;

        if (!_isHandlerScreenShowButton)
        {
            ScreenShotButton.Click += ScreenShotButton_Click;
            _isHandlerScreenShowButton = true;
        }
        ScreenShotButton.Visibility = Visibility.Visible;
    }

    public void DisableScreenButton()
    {
        if (ScreenShotButton != null)
        {
            ScreenShotButton.Visibility = Visibility.Collapsed;
        }
    }

    public void InitQuality(DataTemplate qualityDataTemplate)
    {
        if (QualityListView == null) return;

        QualityListView.ItemTemplate = qualityDataTemplate;
        QualityListView.SelectionChanged += QualityListView_SelectionChanged;
    }

    //private bool _isFirstQualityChanged;
    private bool _isSettingQualitySource;
    public void SetQualityListSource(List<Quality> qualityItemsSource,int qualityIndex)
    {
        if (QualityListView == null) return;

        //_isFirstQualityChanged = true;

        _isSettingQualitySource = true;

        var maxIndex = qualityItemsSource.Count - 1;
        var newIndex = qualityIndex > maxIndex ? maxIndex : qualityIndex;
        QualityListView.ItemsSource = qualityItemsSource;

        Debug.WriteLine($"正在设置了画质Index:{QualityListView.SelectedIndex}->{newIndex}");
        QualityListView.SelectedIndex = newIndex;
        Debug.WriteLine("完成修改QualityListView.SelectedIndex");

        _isSettingQualitySource = false;
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
            new Player(PlayerType.Vlc),
            new Player(PlayerType.Mpv),
            new Player(PlayerType.PotPlayer)};

        //播放器选择列表
        playerListView.ItemsSource = playerItemsSource;
        playerListView.SelectionChanged += PlayerListView_SelectionChanged;
    }

    private void PlayerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PlayerChanged?.Invoke(sender, e);

    }

    private void QualityListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Debug.WriteLine("画质选项发生改变");
        if (_isSettingQualitySource) return;

        Debug.WriteLine("点击了切换画质按钮");

        QualityChanged?.Invoke(sender, e);
    }

    private void FullWindowButton_Click(object sender, RoutedEventArgs e)
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
    private void ScreenShotButton_Click(object sender, RoutedEventArgs e)
    {
        ScreenShotButtonClick?.Invoke(sender, e);
    }


    private bool _isRightButtonPointRight = true;
    private void RightButton_Click(object sender, RoutedEventArgs e)
    {
        _isRightButtonPointRight = !_isRightButtonPointRight;

        VisualStateManager.GoToState(this, _isRightButtonPointRight ? "RightButtonPointRight" : "RightButtonPointLeft",
            true);

        RightButtonClick?.Invoke(sender, e);
    }
}


public class Player
{
    public PlayerType PlayerType;

    public string Name => PlayerType.ToString();

    public Player(PlayerType playerType)
    {
        this.PlayerType = playerType;
    }
}