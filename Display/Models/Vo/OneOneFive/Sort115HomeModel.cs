using CommunityToolkit.Mvvm.ComponentModel;
using Display.Models.Api.OneOneFive.File;
using Display.Models.Dto.OneOneFive;
using Display.Models.Enums;

namespace Display.Models.Vo.OneOneFive;

public partial class Sort115HomeModel(FilesInfo info) : ObservableObject
{
    public readonly FilesInfo Info = info;

    [ObservableProperty]
    private string _destinationName;

    [ObservableProperty]
    private string _destinationPathName;

    [ObservableProperty]
    private Status _status = Status.BeforeStart;

    public string Format;

    public int Index;
}