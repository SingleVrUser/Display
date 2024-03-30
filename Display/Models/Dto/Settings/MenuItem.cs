using Display.Models.Enums;

namespace Display.Models.Dto.Settings;

public class MenuItem(string content, string glyph, NavigationViewItemEnum pageEnum) : Dto.Settings.BaseMenuItem(content, glyph, pageEnum)
{

}