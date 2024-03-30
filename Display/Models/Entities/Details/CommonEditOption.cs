namespace Display.Models.Entities.Details;

public class CommonEditOption
{
    public string Header;
    public string PlaceholderText => $"请输入{Header}";
    public double MinWidth { get; set; } = 150;

    public string Text;
}