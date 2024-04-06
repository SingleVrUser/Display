using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.System;
using Display.Helper.Network;
using Display.Managers;
using Display.Models.Dto.OneOneFive;
using Display.Models.Enums;
using Display.Models.Enums.OneOneFive;
using Display.Providers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages.SpiderVideoInfo;

public sealed partial class ConditionalCheckPage
{
    private ObservableCollection<ConditionCheck> _conditionCheckItems;
    private ConditionCheck _imageItem;

    public ConditionalCheckPage()
    {
        InitializeComponent();

        InitializeView();
    }

    private void InitializeView()
    {
        _conditionCheckItems = [];

        //显示UI

        _imageItem = new ConditionCheck(condition: "图片存放地址", checkUrl: AppSettings.ImageSavePath,
            checkUrlRoutedEventHandler: ImageCheckButton_Click);

        _conditionCheckItems.Add(_imageItem);

        var spiders = SpiderManager.Spiders;

        var onSpiders = spiders.Where(spider => spider.IsOn);

        if (!onSpiders.Any())
        {
            spiderOrigin_TextBlock.Visibility = Visibility.Visible;
            return;
        }

        foreach (var spider in SpiderManager.Spiders)
        {
            _conditionCheckItems.Add(new ConditionCheck
            {
                Condition = $"访问{spider.Abbreviation}",
                CheckUrl = spider.BaseUrl
            });
        }

        Check_Condition();
    }

    private async void ImageCheckButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not HyperlinkButton { DataContext: ConditionCheck item }) return;

        var folder = await StorageFolder.GetFolderFromPathAsync(item.CheckUrl);

        await Launcher.LaunchFolderAsync(folder);
    }

    private async void Check_Condition()
    {
        var isAllSuccess = true;
        foreach (var item in _conditionCheckItems)
        {
            item.Status = Status.Doing;

            if (item.CheckUrl.Contains("http"))
            {
                var isUseful = await NetworkHelper.CheckUrlUseful(item.CheckUrl);

                //网络有用
                if (isUseful)
                {
                    item.Status = Status.Success;
                }
                else
                {
                    item.Status = Status.Error;
                    isAllSuccess = false;
                }
            }
            //图片路径检查
            else
            {
                var isExistsImageSavePath = Directory.Exists(item.CheckUrl);

                item.Status = isExistsImageSavePath ? Status.Success : Status.Error;
            }

        }

        if (!isAllSuccess) Error_TextBlock.Visibility = Visibility.Visible;
    }

    private void ClickOne_Click(object sender, RoutedEventArgs e)
    {
        Check_Condition();
    }

}


public class ConditionCheck : INotifyPropertyChanged
{

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string Condition { get; init; }
    public string CheckUrl { get; init; }

    private Status _status = Status.BeforeStart;
    public Status Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    public readonly RoutedEventHandler CheckUrlRoutedEventHandler;

    public ConditionCheck()
    {
    }

    public ConditionCheck(string condition, string checkUrl, RoutedEventHandler checkUrlRoutedEventHandler)
    {
        Condition = condition;
        CheckUrl = checkUrl;
        CheckUrlRoutedEventHandler = checkUrlRoutedEventHandler;
    }

    public void OnCheckUrlClicked(object sender, RoutedEventArgs e)
        => CheckUrlRoutedEventHandler?.Invoke(sender, e);
    
}