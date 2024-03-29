using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Display.Models.Enums;
using Display.Models.Enums.OneOneFive;

namespace Display.Models.Dto.OneOneFive;

public class ThumbnailInfo : INotifyPropertyChanged
{
    public ThumbnailInfo(VideoInfo videoInfo)
    {
        Name = videoInfo.trueName;

        var tmpList = videoInfo.SampleImageList.Split(',').ToList();
        if (tmpList.Count > 1)
        {
            ThumbnailDownUrlList = tmpList;
        }

        if (videoInfo.Category.Contains("VR") || videoInfo.Series.Contains("VR"))
        {
            IsVr = true;
        }
    }

    public bool IsVr;

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

    public List<string> ThumbnailDownUrlList { get; set; } = [];

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

    private string _genderInfo;
    public string GenderInfo
    {
        get => _genderInfo;
        set
        {
            _genderInfo = value;
            OnPropertyChanged();
        }
    }

    private string _ageInfo;
    public string AgeInfo
    {
        get => _ageInfo;
        set
        {
            _ageInfo = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}