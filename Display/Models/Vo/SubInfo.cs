namespace Display.Models.Dto.OneOneFive;

public class SubInfo
{
    public string PickCode { get; set; }
    public string Name { get; set; }
    public string FileBelongPickCode { get; set; }

    public string TrueName { get; set; }

    public SubInfo(string pickCode, string name, string fileBelongPickCode, string trueName)
    {
        PickCode = pickCode;
        Name = name;
        FileBelongPickCode = fileBelongPickCode;
        TrueName = trueName;
    }
}