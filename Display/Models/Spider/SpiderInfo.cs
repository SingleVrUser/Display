using Display.Models.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Display.Helper.Network.Spider;
using static Display.Models.Spider.SpiderInfos;

namespace Display.Models.Spider;

public class SpiderInfos
{
    public enum SpiderSourceName
    {
        Javbus, Jav321, Avmoo, Avsox, Libredmm, Fc2club, Javdb, Local
    }

    public enum SpiderStates { Ready, Doing, Awaiting, Done }
        
}

public class SpiderInfo : INotifyPropertyChanged
{
    public SpiderSourceName SpiderSource { get; set; }

    public string SearchName { get; set; }

    public SpiderStates State { get; set; }
    public bool IsEnable { get; set; }
    public Brush EllipseColor
    {
        get
        {
            return State switch
            {
                SpiderStates.Ready => new SolidColorBrush(Colors.MediumSeaGreen),
                SpiderStates.Doing => new SolidColorBrush(Colors.MediumSeaGreen),
                SpiderStates.Awaiting => new SolidColorBrush(Colors.SkyBlue),
                SpiderStates.Done => new SolidColorBrush(Colors.LightGray),
                _ => new SolidColorBrush(Colors.OrangeRed)
            };
        }

    }

    /// <summary>
    /// 成功或失败是针对番号的搜刮的
    /// </summary>
    public RequestStates RequestStates { get; set; }

    public long SpiderCount { get; set; }

    public Visibility EllipseVisible => IsEnable ? Visibility.Visible : Visibility.Collapsed;

    private string _message;
    public string Message
    {
        get => _message;
        set
        {
            if (_message == value)
                return;
            _message = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EllipseColor));
            OnPropertyChanged(nameof(SpiderCount));
        }
    }

    //初始化
    public SpiderInfo(InfoSpider spider)
    {
        SpiderSource = spider.Name;

        IsEnable = spider.IsOn;

        if (!IsEnable)
            Message = "已禁用";
    }


    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}


