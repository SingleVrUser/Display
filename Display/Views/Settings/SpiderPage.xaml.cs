using Display.Helper.Network.Spider;
using Display.Models.Data;
using Display.Models.Settings.Options;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.Settings;

public sealed partial class SpiderPage : Page
{
    private List<Spider> _spiders;

    public SpiderPage()
    {
        this.InitializeComponent();

        InitOptions();
    }

    private void InitOptions()
    {
        _spiders = [];
        // 搜刮源
        foreach (var spider in Manager.Spiders)
        {
            _spiders.Add(new Spider(spider)
            {
                SaveCookieAction = cookie => spider.Cookie = cookie
            });

        }

        _spiders.ForEach(i => Debug.WriteLine(i.Instance.DelayRanges));
    }


    /// <summary>
    /// 如果选中的搜刮源少于一个，则提示
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SpiderToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton) return;

        //至少选择一个搜刮源
        var isOneTurnOn = Manager.Spiders.Any(spider => spider.IsOn);

        if (isOneTurnOn) return;

        //ShowTeachingTip("请至少选择一个搜刮源，否则无法正常搜刮");
        Debug.WriteLine("请至少选择一个搜刮源，否则无法正常搜刮");
    }



}