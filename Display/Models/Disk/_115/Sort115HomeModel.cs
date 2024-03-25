using CommunityToolkit.Mvvm.ComponentModel;
using Display.Models.Data;

namespace Display.Models.Disk._115;

public partial class Sort115HomeModel : ObservableObject
{
    public FilesInfo Info;

    [ObservableProperty]
    private string _destinationName;

    [ObservableProperty]
    private string _destinationPathName;

    [ObservableProperty]
    private Status _status = Status.BeforeStart;

    public Sort115HomeModel(FilesInfo info)
    {
        Info = info;
    }

    public string Format;

    public int Index;
}