using System;
using Display.Models.Enums;
using Microsoft.UI.Xaml;

namespace Display.Models.Dto.Settings;

public class MoreMenuItem(NavigationViewItemEnum pageEnum ,string content, string description, string glyph, Type pageType) : BaseMenuItem(pageEnum, content, glyph, pageType)
{
    public string Description { get; } = description;
    public string Label { get; set; }
    public Visibility IsRightTopLabelShow => string.IsNullOrEmpty(Label) ? Visibility.Collapsed : Visibility.Visible;
}