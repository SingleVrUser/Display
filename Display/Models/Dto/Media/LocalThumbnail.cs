#nullable enable
using CommunityToolkit.Mvvm.ComponentModel;

namespace Display.Models.Dto.Media;

public partial class LocalThumbnail(long timeStamp) : Dto.Media.BaseImage
{
    [ObservableProperty]
    private long _timeStamp = timeStamp;
}