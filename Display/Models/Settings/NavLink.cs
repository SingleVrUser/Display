using System;

namespace Display.Models.Settings;

internal record NavLink
{
    public string Label { get; init; }
    public string Glyph { get; init; }

    public Type NavPageType { get; init; }
}