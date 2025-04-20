using FFmpeg.AutoGen.Abstractions;
using System;
using System.Threading.Tasks;
using Display.Models.Dto.Media;
using LocalThumbnail = Display.Models.Dto.Media.LocalThumbnail;
using System.Threading;

namespace Display.Interfaces;

internal interface IThumbnailGeneratorService
{
    // Open url contain video
    bool OpenVideo(UrlOptions options);

    // Seek position
    bool SeekPosition(long timeStamp);

    Task<bool> DecodeAllFramesToImages(ThumbnailGenerateOptions thumbnailGenerateOptions, CancellationToken cancellationToken, IProgress<LocalThumbnail> progress = null, AVHWDeviceType hwDevice = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE);
}