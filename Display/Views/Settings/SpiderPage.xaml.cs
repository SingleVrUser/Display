using Display.Managers;
using Display.Models.Settings.Options;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.Settings;

public sealed partial class SpiderPage : RootPage
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
        // �ѹ�Դ
        foreach (var spider in SpiderManager.Spiders)
        {
            _spiders.Add(new Spider(spider)
            {
                SaveCookieAction = cookie => spider.Cookie = cookie
            });

        }

        _spiders.ForEach(i => Debug.WriteLine(i.Instance.DelayRanges));
    }


    /// <summary>
    /// ���ѡ�е��ѹ�Դ����һ��������ʾ
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SpiderToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton) return;

        //����ѡ��һ���ѹ�Դ
        var isOneTurnOn = SpiderManager.Spiders.Any(spider => spider.IsOn);

        if (isOneTurnOn) return;

        ShowTeachingTip("������ѡ��һ���ѹ�Դ�������޷������ѹ�");
    }



}