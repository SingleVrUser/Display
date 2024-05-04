using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DataAccess.Models.Entity;
using Microsoft.UI.Xaml;

namespace Display.Models.Vo.Video;

/// <summary>
/// 视频封面缩略信息
/// </summary>
public partial class VideoCoverVo: ObservableObject
{
    public long Id { get; set; }
    
    public string Title { get; set; }
    
    public string Name { get; set; }
    public string ReleaseYear { get;}
    
    public string ReleaseTime { get; set; }
    public Visibility IsShowLabel { get;}
    public string ShowLabel { get;}
    
    [ObservableProperty]
    private string _imagePath = Constants.FileType.NoPicturePath;

    [ObservableProperty]
    private string _actorName;
    
    [ObservableProperty]
    private string _categoryName;

    [ObservableProperty]
    private double _score;
    
    [ObservableProperty]
    private Visibility _isDeleted = Visibility.Collapsed;
    
    [ObservableProperty]
    private double _imageWidth;

    [ObservableProperty]
    private bool _isLike;

    [ObservableProperty]
    private bool _isLookLater;

    public VideoCoverVo(){}

    public VideoCoverVo(VideoInfo videoInfo)
    {
        Id = videoInfo.Id;
        
        //名称
        Name = videoInfo.Name;

        Title = videoInfo.Title;

        ReleaseTime = videoInfo.ReleaseTime;
        
        //评分
        Score = videoInfo.Interest.Score ?? 0;

        if (videoInfo.CategoryList != null)
        {
            CategoryName = string.Join(",", videoInfo.CategoryList.Select(i => i.Name));
        }
        
        //演员
        ActorName = string.Join(",", videoInfo.ActorInfoList);
            
        //是否显示右上角的标签
        if (videoInfo.CategoryList != null)
        {
            foreach (var categoryInfo in videoInfo.CategoryList)
            {
                var category = categoryInfo.Name;
                if(string.IsNullOrEmpty(category)) continue;
                
                var isShowLabel = Visibility.Collapsed;
                var showLabel = string.Empty;
                
                if (category.Contains("VR") || videoInfo.Series != null && videoInfo.Series.Name.Contains("VR"))
                {
                    isShowLabel = Visibility.Visible;
                    showLabel = "VR";
                }
                else if (category.Contains("4K"))
                {
                    isShowLabel = Visibility.Visible;
                    showLabel = "4K";
                }
                

                IsShowLabel = isShowLabel;
                ShowLabel = showLabel;
                break;
            }
        }
        
        // 发布时间
        if (videoInfo.ReleaseTime != null)
        {
            ReleaseYear = videoInfo.ReleaseTime.Contains('/')
                ? videoInfo.ReleaseTime.Split('/')[0]
                : videoInfo.ReleaseTime.Split('-')[0];
        }
    }

    public VideoCoverVo(VideoInfo videoInfo, double imgWidth = 500) : this(videoInfo)
    {
        //图片大小
        ImageWidth = imgWidth;
    }

}