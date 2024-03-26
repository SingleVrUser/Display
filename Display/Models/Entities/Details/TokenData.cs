using System.Linq;

namespace Display.Models.Entities.Details;

public class TokenData(string name)
{
    public string Initials => string.Empty + Name.FirstOrDefault();

    public string Name { get; set; } = name;
}