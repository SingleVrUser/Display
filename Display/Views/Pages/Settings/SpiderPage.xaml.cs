using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Display.Managers;
using Display.Models.Settings.Options;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Display.Views.Pages.Settings;

public sealed partial class SpiderPage
{
    private List<Spider> _spiders;

    public SpiderPage()
    {
        InitializeComponent();

        InitOptions();
    }

    private void InitOptions()
    {
        _spiders = [];
        
        foreach (var spider in SpiderManager.Spiders)
        {
            _spiders.Add(new Spider(spider)
            {
                SaveCookieAction = cookie => spider.Cookie = cookie
            });

        }

        _spiders.ForEach(i => Debug.WriteLine(i.Instance.DelayRanges));
    }


    private void SpiderToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton) return;

        var isOneTurnOn = SpiderManager.Spiders.Any(spider => spider.IsOn);

        if (isOneTurnOn) return;

        ShowTeachingTip("请至少选择一个搜刮源，否则无法正常搜刮");
    }



}