#nullable enable
using CommunityToolkit.Mvvm.ComponentModel;

namespace Display.Models.Image;

public class LocalThumbnail : BaseImage
{

    [field: ObservableProperty]
    public long TimeStamp { get; set; }


    public LocalThumbnail(long timeStamp)
    {
        TimeStamp = timeStamp;
    }

}