using Display.Models.Enums;
using Microsoft.UI.Xaml;

namespace Display.Models.Dto.Settings;

public class MoreMenuItem(string content, string description, string glyph, NavigationViewItemEnum pageEnum) : Dto.Settings.BaseMenuItem(content, glyph, pageEnum)
{
    public new bool CanSelected { get; set; }
    public string Description { get; } = description;
    public string Label { get; set; }
    public Visibility IsRightTopLabelShow => string.IsNullOrEmpty(Label) ? Visibility.Collapsed : Visibility.Visible;
}