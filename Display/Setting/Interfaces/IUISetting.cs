using Display.Models.Data.Enums;

namespace Display.Setting.Interfaces;

internal interface IUISetting
{
    /// <summary>
    /// 展示匹配失败的列表?
    /// </summary>
    public bool IsShowFailListInDisplay { get; set; }

    /// <summary>
    /// 图片宽度
    /// </summary>
    public double ImageWidth { get; set; }

    /// <summary>
    /// 图片高度
    /// </summary>
    public double ImageHeight { get; set; }

    /// <summary>
    /// 是否动态调整图片大小
    /// </summary>
    public bool IsAutoAdjustImageSize { get; set; }

    /// <summary>
    /// 缩略图的显示来源
    /// </summary>
    public ThumbnailOriginType ThumbnailOriginType { get; set; }
}