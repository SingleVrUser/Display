﻿using Display.Models.Data.Enums;
using Microsoft.UI.Xaml;

namespace Display.Models.Settings;

public class MoreMenuItem(string content, string description, string glyph, NavigationViewItemEnum pageEnum) : BaseMenuItem(content, glyph, pageEnum)
{
    public new bool CanSelected { get; set; }
    public string Description { get; } = description;
    public string Label { get; set; }
    public Visibility IsRightTopLabelShow => string.IsNullOrEmpty(Label) ? Visibility.Collapsed : Visibility.Visible;
}