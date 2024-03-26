using Display.Models.Enums;

namespace Display.Models.Entities._115;

public class PartNumCombo(PartNum partNum, string description)
{
    public PartNum PartNum { get; } = partNum;

    public string Description { get; } = description;

    public override string ToString()
    {
        return Description;
    }
}