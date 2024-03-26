#nullable enable
using CommunityToolkit.Mvvm.ComponentModel;

namespace Display.Models.Media;

public partial class LocalThumbnail(long timeStamp) : Media.BaseImage
{
    [ObservableProperty]
    private long _timeStamp = timeStamp;
}