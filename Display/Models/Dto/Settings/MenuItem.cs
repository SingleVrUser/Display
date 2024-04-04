using System;
using Display.Models.Enums;

namespace Display.Models.Dto.Settings;

public class MenuItem(NavigationViewItemEnum pageEnum, string content, string glyph, Type pageType) : BaseMenuItem(pageEnum, content, glyph, pageType)
{

}