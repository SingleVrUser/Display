using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen.Abstractions;
using FFmpeg.AutoGen.Bindings.DynamicallyLoaded;
using SkiaSharp;

namespace Display.Helper.Media;

internal static class FFmpegHelper
{
    public static int ThrowExceptionIfError(this int error)
    {
        if (error >= 0) return error;

        var message = av_strError(error);
        Debug.WriteLine(message);
        throw new ApplicationException(message);

    }

    private static unsafe string av_strError(int error)
    {
        var bufferSize = 1024;
        var buffer = stackalloc byte[bufferSize];
        ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
        var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
        return message;
    }

    internal static void RegisterFFmpegBinaries()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        var current = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        var probe = Path.Combine("FFmpeg", "bin", Environment.Is64BitProcess ? "x64" : "x86");

        while (current != null)
        {
            var ffmpegBinaryPath = Path.Combine(current, probe);

            if (Directory.Exists(ffmpegBinaryPath))
            {
                Debug.WriteLine($"FFmpeg binaries found in: {ffmpegBinaryPath}");
                //ffmpeg.RootPath = ffmpegBinaryPath;
                DynamicallyLoadedBindings.LibrariesPath = ffmpegBinaryPath;
                DynamicallyLoadedBindings.Initialize();
                return;
            }

            current = Directory.GetParent(current)?.FullName;
        }
    }


    public static unsafe void WriteFrame(AVFrame frame, string saveFilePath)
    {
        var imageInfo = new SKImageInfo(frame.width, frame.height, SKColorType.Bgra8888,
            SKAlphaType.Opaque);
        using var bitmap = new SKBitmap();
        bitmap.InstallPixels(imageInfo, (IntPtr)frame.data[0]);
        using var stream = File.Create(saveFilePath);
        bitmap.Encode(stream, SKEncodedImageFormat.Jpeg, 90);
    }

    public static AVPixelFormat GetHwPixelFormat(AVHWDeviceType hWDevice)
    {
        return hWDevice switch
        {
            AVHWDeviceType.AV_HWDEVICE_TYPE_NONE => AVPixelFormat.AV_PIX_FMT_NONE,
            AVHWDeviceType.AV_HWDEVICE_TYPE_VDPAU => AVPixelFormat.AV_PIX_FMT_VDPAU,
            AVHWDeviceType.AV_HWDEVICE_TYPE_CUDA => AVPixelFormat.AV_PIX_FMT_CUDA,
            AVHWDeviceType.AV_HWDEVICE_TYPE_VAAPI => AVPixelFormat.AV_PIX_FMT_VAAPI,
            AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2 => AVPixelFormat.AV_PIX_FMT_NV12,
            AVHWDeviceType.AV_HWDEVICE_TYPE_QSV => AVPixelFormat.AV_PIX_FMT_QSV,
            AVHWDeviceType.AV_HWDEVICE_TYPE_VIDEOTOOLBOX => AVPixelFormat.AV_PIX_FMT_VIDEOTOOLBOX,
            AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA => AVPixelFormat.AV_PIX_FMT_NV12,
            AVHWDeviceType.AV_HWDEVICE_TYPE_DRM => AVPixelFormat.AV_PIX_FMT_DRM_PRIME,
            AVHWDeviceType.AV_HWDEVICE_TYPE_OPENCL => AVPixelFormat.AV_PIX_FMT_OPENCL,
            AVHWDeviceType.AV_HWDEVICE_TYPE_MEDIACODEC => AVPixelFormat.AV_PIX_FMT_MEDIACODEC,
            _ => AVPixelFormat.AV_PIX_FMT_NONE
        };
    }
}