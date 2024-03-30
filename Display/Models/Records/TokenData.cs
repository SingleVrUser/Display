using System.Linq;

namespace Display.Models.Records;

public record TokenData(string Name)
{
    public string Initials { get; } = string.Empty + Name.FirstOrDefault();

}