using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DataAccess.Models.Entity;
using Display.Models.Enums;

namespace Display.Models.Vo;

public sealed class ThumbnailInfo : INotifyPropertyChanged
{
    public ThumbnailInfo(VideoInfo videoInfo)
    {
        Name = videoInfo.TrueName;

        if (videoInfo.SampleImageList != null)
            ThumbnailDownUrlList = videoInfo.SampleImageList.Split(',').ToArray();

        IsVr = (videoInfo.Category != null && videoInfo.Category.Contains("VR")) ||
               (videoInfo.Series != null && videoInfo.Series.Contains("VR"));
    }

    public readonly bool IsVr;

    public string Name { get; set; }
    public int Count;

    private string _photoPath = "ms-appx:///Assets/Svg/picture-o.svg";

    public string PhotoPath
    {
        get => _photoPath;
        set
        {
            _photoPath = value;
            OnPropertyChanged();
        }
    }

    public string[] ThumbnailDownUrlList { get; init; }

    private Status _status = Status.BeforeStart;
    public Status Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    private string _genderInfo;
    public string GenderInfo
    {
        get => _genderInfo;
        set => SetField(ref _genderInfo, value);
    }

    private string _ageInfo;
    public string AgeInfo
    {
        get => _ageInfo;
        set => SetField(ref _ageInfo, value);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}