using Display.Models.Data.Enums;

namespace Display.Setting.Interfaces;

internal interface IPlayerSetting
{
    public bool IsPlayBestQualityFirst { get; set; }

    /// <summary>
    /// 是否自动播放视频
    /// </summary>
    public bool IsAutoPlayInVideoDisplay { get; set; }

    /// <summary>
    /// 自动播放的位置
    /// </summary>
    public double AutoPlayPositionPercentage { get; set; }

    /// <summary>
    /// 视频最大播放数量
    /// </summary>
    public double MaxVideoPlayCount { get; set; }

    /// <summary>
    /// 播放方式
    /// </summary>
    public PlayerType PlayerSelection { get; set; }

    public PlayQuality DefaultPlayQuality { get; set; }

    /// <summary>
    /// 是否搜索字幕
    /// </summary>
    public bool IsFindSub { get; set; }
}