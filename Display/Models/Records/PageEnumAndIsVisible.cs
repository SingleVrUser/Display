using Display.Models.Enums;

namespace Display.Models.Records;

public record PageEnumAndIsVisible(NavigationViewItemEnum PageEnum, bool IsVisible = true);