using Display.Data;
using Display.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using Microsoft.UI.Dispatching;

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

    private bool _isHandlerLikeAndLookLaterButton = false;
    private bool _isHandlerScreenShowButton = false;

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
        ;
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
        //画质选择按钮
        Button qualityButton = GetTemplateChild("QualityButton") as Button;
        if (qualityButton == null) return;
        qualityButton.Visibility = Visibility.Visible;
        
        if (QualityListView == null) return;
        QualityListView.ItemTemplate = qualityDataTemplate;

        QualityListView.SelectionChanged += QualityListView_SelectionChanged;
    }

    private bool isFirstQualityChanged;
    private bool isSettingQuslitySource;
    public void SetQualityListSource(List<Quality> qualityItemsSource,int qualityIndex)
    {
        if (QualityListView == null) return;

        isFirstQualityChanged = true;
        isSettingQuslitySource = true;

        QualityListView.ItemsSource = qualityItemsSource;

        int maxIndex = qualityItemsSource.Count - 1;
        if (qualityIndex > maxIndex)
        {
            qualityIndex = maxIndex;
        }
        QualityListView.SelectedIndex = qualityIndex;

        isSettingQuslitySource = false;

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
        if (isFirstQualityChanged)
        {
            isFirstQualityChanged = false;
            return;
        }

        if (isSettingQuslitySource) return;

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
    public WebApi.PlayMethod PlayMethod;

    public string Name => PlayMethod.ToString();


    public Player(WebApi.PlayMethod playerMethod)
    {
        this.PlayMethod = playerMethod;


    }
}