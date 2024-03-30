using Display.Models.Enums;

namespace Display.Models.Records;

public record PartNumCombo(PartNum PartNum, string Description)
{
    public override string ToString()
    {
        return Description;
    }
}