#nullable enable
using CommunityToolkit.Mvvm.ComponentModel;

namespace Display.Models.Dto.Media;

public partial class LocalThumbnail(long timeStamp) : BaseImage
{
    [ObservableProperty]
    private long _timeStamp = timeStamp;
}