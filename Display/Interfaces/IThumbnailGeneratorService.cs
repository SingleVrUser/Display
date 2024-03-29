using Display.Models.Media;
using FFmpeg.AutoGen.Abstractions;
using System;
using System.Threading.Tasks;
using Display.Models.Dto.Media;
using LocalThumbnail = Display.Models.Dto.Media.LocalThumbnail;

namespace Display.Interfaces;

internal interface IThumbnailGeneratorService
{
    // Open url contain video
    bool OpenVideo(UrlOptions options);

    // Seek position
    bool SeekPosition(long timeStamp);

    Task DecodeAllFramesToImages(ThumbnailGenerateOptions thumbnailGenerateOptions, IProgress<LocalThumbnail> progress = null, AVHWDeviceType hwDevice = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE);
}