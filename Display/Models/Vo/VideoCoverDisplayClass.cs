using Display.Models.Entities.OneOneFive;
using Display.Models.Vo;
using Microsoft.UI.Xaml;

namespace Display.Models.Dto.OneOneFive;

/// <summary>
/// 视频封面缩略信息
/// </summary>
public class VideoCoverDisplayClass : VideoInfo
{
    public VideoCoverDisplayClass()
    {
        OnPropertyChanged();
    }

    public VideoCoverDisplayClass(VideoInfo videoInfo)
    {
        foreach (var videoInfoItem in videoInfo.GetType().GetProperties())
        {
            var name = videoInfoItem.Name;
            var value = videoInfoItem.GetValue(videoInfo);

            var newItem = GetType().GetProperty(name);
            newItem?.SetValue(this, value);
        }

        //标题
        Title = videoInfo.Title;

        //是否显示右上角的标签
        var category = videoInfo.Category;
        var isShowLabel = Visibility.Collapsed;

        string showLabel = string.Empty;
        if (!string.IsNullOrEmpty(category))
        {
            if (category.Contains("VR") || !string.IsNullOrEmpty(videoInfo.Series) && videoInfo.Series.Contains("VR"))
            {
                isShowLabel = Visibility.Visible;
                showLabel = "VR";
            }
            else if (category.Contains("4K"))
            {
                isShowLabel = Visibility.Visible;
                showLabel = "4K";
            }
        }

        if (!string.IsNullOrEmpty(ReleaseTime))
        {
            if (videoInfo.ReleaseTime.Contains("/"))
            {
                ReleaseYear = videoInfo.ReleaseTime.Split('/')[0];
            }
            else
            {
                ReleaseYear = videoInfo.ReleaseTime.Split('-')[0];
            }
        }

        this.isShowLabel = isShowLabel;
        ShowLabel = showLabel;
        Score = videoInfo.Score;
    }

    public VideoCoverDisplayClass(VideoInfo videoInfo, double imgWidth, double imgHeight) : this(videoInfo)
    {
        //图片大小
        imageHeight = imgHeight;
        ImageWidth = imgWidth;
    }

    public string ReleaseYear { get; set; }
    public Visibility isShowLabel { get; set; } = Visibility.Collapsed;
    public string ShowLabel { get; set; }

    private Visibility _isDeleted = Visibility.Collapsed;
    public Visibility IsDeleted
    {
        get => _isDeleted;
        set
        {
            _isDeleted = value;
            OnPropertyChanged();
        }
    }

    private double _imageWidth;
    public double ImageWidth
    {
        get => _imageWidth;
        set
        {
            _imageWidth = value;
            OnPropertyChanged();
        }
    }

    private double _imageHeight;
    public double imageHeight
    {
        get
        {
            return _imageHeight;
        }
        set
        {
            _imageHeight = value;
            OnPropertyChanged();
        }
    }

    //public event PropertyChangedEventHandler PropertyChanged;
    //protected void RaisePropertyChanged([CallerMemberName] string Name = "")
    //{
    //    if (PropertyChanged != null)
    //    {
    //        PropertyChanged(this, new PropertyChangedEventArgs(Name));
    //    }
    //}

}