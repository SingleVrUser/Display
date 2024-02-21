using Display.Models.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Display.Models.Spider.SpiderInfos;

namespace Display.Models.Spider;

public class SpiderInfos
{
    public enum SpiderSourceName { Javbus, Jav321, Avmoo, Avsox, Libredmm, Fc2club, Javdb, Local }

    public enum SpiderStates { ready, doing, awaiting, done }

}


public class SpiderInfo : INotifyPropertyChanged
{
    public SpiderSourceName SpiderSource { get; set; }

    public string Name { get; }

    public SpiderStates State { get; set; }
    public bool IsEnable { get; set; }
    public Brush EllipseColor
    {
        get
        {
            switch (State)
            {
                case SpiderStates.ready:
                    return new SolidColorBrush(Colors.MediumSeaGreen);
                case SpiderStates.doing:
                    return new SolidColorBrush(Colors.MediumSeaGreen);
                case SpiderStates.awaiting:
                    return new SolidColorBrush(Colors.SkyBlue);
                case SpiderStates.done:
                    return new SolidColorBrush(Colors.LightGray);
            }

            return new SolidColorBrush(Colors.OrangeRed);
        }

    }

    /// <summary>
    /// 成功或失败是针对番号的搜刮的
    /// </summary>
    public RequestStates RequestStates { get; set; }

    private long _spidercount;
    public long SpiderCount
    {
        get => _spidercount;
        set
        {
            if (_spidercount == value) return;

            _spidercount = value;
        }
    }

    public Visibility EllipseVisiable
    {
        get
        {
            if (IsEnable)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }
    }

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
    public SpiderInfo(SpiderSourceName spiderSource, bool isEnable)
    {
        SpiderSource = spiderSource;
        IsEnable = isEnable;

        if (!IsEnable)
            Message = "已禁用";
    }

    public SpiderInfo(SpiderSourceName spiderSource, string name)
    {
        SpiderSource = spiderSource;
        Name = name;
    }

    public SpiderInfo(SpiderSourceName spiderSource)
    {
        SpiderSource = spiderSource;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}


