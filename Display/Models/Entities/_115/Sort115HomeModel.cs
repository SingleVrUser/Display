using CommunityToolkit.Mvvm.ComponentModel;
using Display.Models.Dto.OneOneFive;
using Display.Models.Enums;
using Display.Models.Enums.OneOneFive;

namespace Display.Models.Entities._115;

public partial class Sort115HomeModel(FilesInfo info) : ObservableObject
{
    public FilesInfo Info = info;

    [ObservableProperty]
    private string _destinationName;

    [ObservableProperty]
    private string _destinationPathName;

    [ObservableProperty]
    private Status _status = Status.BeforeStart;

    public string Format;

    public int Index;
}