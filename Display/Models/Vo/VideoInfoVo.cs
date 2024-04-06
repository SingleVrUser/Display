using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using DataAccess.Models.Entity;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace Display.Models.Vo;

/// <summary>
/// 视频封面缩略信息
/// </summary>
public class VideoInfoVo : VideoInfo, INotifyPropertyChanged
{
    private string _trueName;
    public new string TrueName
    {
        get => _trueName;
        set => SetField(ref _trueName, value);
    }

    private string _imagePath;
    public new string ImagePath
    {
        get => _imagePath;
        set => SetField(ref _imagePath, value);
    }

    private string _releaseTime;
    public new string ReleaseTime
    {
        get => _releaseTime;
        set => SetField(ref _releaseTime, value);
    }

    private string _actor;
    public new string Actor
    {
        get => _actor;
        set => SetField(ref _actor, value);
    }

    public VideoInfoVo()
    {
        
    }
    
    public VideoInfoVo(VideoInfo videoInfo)
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

        var showLabel = string.Empty;
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

        if (ReleaseTime != null && !string.IsNullOrEmpty(ReleaseTime) && videoInfo.ReleaseTime != null)
        {
            ReleaseYear = videoInfo.ReleaseTime.Contains('/') ? videoInfo.ReleaseTime.Split('/')[0] : videoInfo.ReleaseTime.Split('-')[0];
        }

        IsShowLabel = isShowLabel;
        ShowLabel = showLabel;
        Score = videoInfo.Score;
    }

    public VideoInfoVo(VideoInfo videoInfo, double imgWidth = 500) : this(videoInfo)
    {
        //图片大小
        ImageWidth = imgWidth;
        //ImageHeight = imgHeight;
    }

    public string ReleaseYear { get; set; }
    public Visibility IsShowLabel { get; set; }
    public string ShowLabel { get; set; }

    private Visibility _isDeleted = Visibility.Collapsed;
    public Visibility IsDeleted
    {
        get => _isDeleted;
        set => SetField(ref _isDeleted, value);
    }

    private double _imageWidth;
    public double ImageWidth
    {
        get => _imageWidth;
        set => SetField(ref _imageWidth, value);
    }

    //private double _imageHeight;
    //public double ImageHeight
    //{
    //    get => _imageHeight;
    //    set => SetField(ref _imageHeight, value);
    //}

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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