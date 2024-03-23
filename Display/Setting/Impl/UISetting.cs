using Display.Models.Data.Enums;
using Display.Setting.Interfaces;
using DefaultValue = Display.Constants.DefaultSettings;

namespace Display.Setting.Impl;

internal class UISetting(ISettingProvider provider) : SettingBase(provider), IUISetting
{
    public bool IsShowFailListInDisplay
    {
        get => GetValue(DefaultValue.Ui.IsShowFailListInDisplay);
        set => SetValue(value);
    }
    public double ImageWidth
    {
        get => GetValue(DefaultValue.Ui.ImageSize.Width);
        set => SetValue(value);
    }
    public double ImageHeight
    {
        get => GetValue(DefaultValue.Ui.ImageSize.Height);
        set => SetValue(value);
    }
    public bool IsAutoAdjustImageSize
    {
        get => GetValue(DefaultValue.Ui.IsAutoAdjustImageSize);
        set => SetValue(value);
    }
    public ThumbnailOriginType ThumbnailOriginType
    {
        get => GetValue(DefaultValue.Ui.ThumbnailOrigin);
        set => SetValue(value);
    }
}