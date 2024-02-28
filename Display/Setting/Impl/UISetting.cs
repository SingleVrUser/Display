using Display.Models.Data.Enums;
using Display.Setting.Interfaces;
using DefaultValue = Display.Models.Data.Constant.DefaultSettings;

namespace Display.Setting.Impl;

internal class UISetting : SettingBase, IUISetting
{
    public UISetting(ISettingProvider provider) : base(provider)
    {
    }

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