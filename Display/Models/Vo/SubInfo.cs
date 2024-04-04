namespace Display.Models.Vo;

public class SubInfo(string pickCode, string name, string trueName)
{
    public string PickCode { get; set; } = pickCode;
    public string Name { get; set; } = name;

    public string TrueName { get; set; } = trueName;
}