﻿using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Display.Models.Data.Enums;
using Microsoft.UI.Xaml.Controls;

namespace Display.Models.Settings;

public abstract partial class BaseMenuItem(string content, string glyph, NavigationViewItemEnum pageEnum) : ObservableObject
{
    public string Content { get; } = content;
    public NavigationViewItemEnum PageEnum { get; } = pageEnum;
    public string Glyph { get; } = glyph;

    public bool CanSelected { get; init; } = true;

    /// <summary>
    /// 图标，重复显示IconElement可能会报错
    /// </summary>
    ///
    [JsonIgnore]
    public IconElement Icon =>
        new FontIcon
        {
            Glyph = Glyph
        };

    [ObservableProperty]
    private bool _isVisible = true;
}