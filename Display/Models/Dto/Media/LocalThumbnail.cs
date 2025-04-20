#nullable enable
using CommunityToolkit.Mvvm.ComponentModel;
using Display.Providers;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing;
using System.Threading.Tasks;

namespace Display.Models.Dto.Media;

public partial class LocalThumbnail(long timeStamp, string path) : ObservableObject
{
    public string Path => path;

    [ObservableProperty]
    private string? _dimensions;

    [ObservableProperty]
    private long _timeStamp = timeStamp;

}