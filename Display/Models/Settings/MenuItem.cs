using Display.Models.Data.Enums;

namespace Display.Models.Settings;

public class MenuItem(string content, string glyph, NavigationViewItemEnum pageEnum) : BaseMenuItem(content, glyph, pageEnum)
{

}