using Display.Models.Enums;
using Display.Setting.Interfaces;
using DefaultValue = Display.Constants.DefaultSettings;

namespace Display.Setting.Impl;

internal class PlayerSetting(ISettingProvider provider) : SettingBase(provider), IPlayerSetting
{
    public bool IsPlayBestQualityFirst
    {
        get => GetValue(DefaultValue.Player.IsPlayBestQualityFirst);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否自动播放视频
    /// </summary>
    public bool IsAutoPlayInVideoDisplay
    {
        get => GetValue(DefaultValue.Player.VideoDisplay.IsAutoPlay);
        set => SetValue(value);
    }

    /// <summary>
    /// 自动播放的位置
    /// </summary>
    public double AutoPlayPositionPercentage
    {
        get => GetValue(DefaultValue.Player.VideoDisplay.AutoPlayPositionPercentage);
        set => SetValue(value);
    }

    /// <summary>
    /// 视频最大播放数量
    /// </summary>
    public double MaxVideoPlayCount
    {
        get => GetValue(DefaultValue.Player.VideoDisplay.MaxVideoPlayCount);
        set => SetValue(value);
    }

    /// <summary>
    /// 播放方式
    /// </summary>
    public PlayerType PlayerSelection
    {
        get => GetValue(DefaultValue.Player.Selection);
        set => SetValue(value);
    }

    public PlayQuality DefaultPlayQuality
    {
        get => GetValue(DefaultValue.Player.DefaultQuality);
        set => SetValue(value);
    }

    /// <summary>
    /// 是否搜索字幕
    /// </summary>
    public bool IsFindSub
    {
        get => GetValue(DefaultValue.Network._115.IsFindSub);
        set => SetValue(value);
    }

}