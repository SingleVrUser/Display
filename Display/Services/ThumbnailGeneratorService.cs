using Display.Helper.Date;
using Display.Helper.Media;
using Display.Helper.Network;
using Display.Interfaces;
using Display.Models.Media;
using FFmpeg.AutoGen.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Display.Models.Dto.Media;
using LocalThumbnail = Display.Models.Dto.Media.LocalThumbnail;

namespace Display.Services;

public class ThumbnailGeneratorService : IThumbnailGeneratorService
{
    public ThumbnailGeneratorService()
    {
        FFmpegHelper.RegisterFFmpegBinaries();
        Directory.CreateDirectory(Path.Combine(ApplicationData.Current.LocalFolder.Path, "frames"));
    }

    public bool OpenVideo(UrlOptions options)
    {
        //ConfigureHwDecoder(out var deviceType);
        //DecodeAllFramesToImages(options, deviceType);

        return true;
    }

    public bool SeekPosition(long timeStamp)
    {
        throw new NotImplementedException();
    }

    public async Task DecodeAllFramesToImages(ThumbnailGenerateOptions thumbnailGenerateOptions, IProgress<LocalThumbnail> progress = null, AVHWDeviceType hwDevice = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
    {
        using var vsd = new VideoStreamDecoder(thumbnailGenerateOptions.UrlOptions, hwDevice);

        await Task.Run(vsd.Init);
        //Debug.WriteLine($"codec name: {vsd.CodecName}");

        //var info = vsd.GetContextInfo();
        //info.ToList().ForEach(x => Debug.WriteLine($"{x.Key} = {x.Value}"));

        var sourceSize = vsd.FrameSize;
        var sourcePixelFormat = hwDevice == AVHWDeviceType.AV_HWDEVICE_TYPE_NONE
            ? vsd.PixelFormat
            : FFmpegHelper.GetHwPixelFormat(hwDevice);
        var destinationSize = sourceSize;
        var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_BGRA;
        using var vfc = new VideoFrameConverter(sourceSize, sourcePixelFormat, destinationSize, destinationPixelFormat);

        var frameCount = thumbnailGenerateOptions.FrameCount;
        var increaseTime = vsd.Duration / (frameCount + 1);
        var currentTime = increaseTime;

        // 非m3u8都要等待
        var isWait = !thumbnailGenerateOptions.UrlOptions.IsM3U8;

        for (var i = 0; i < frameCount; i++, currentTime += increaseTime)
        {
            var filePath = GetFilePath(thumbnailGenerateOptions, currentTime);
            if (filePath == null) continue;

            if (!File.Exists(filePath))
            {
                if (isWait) await NetworkHelper.RandomTimeDelay(10, 20);

                if (!vsd.TrySeekPosition(currentTime)) break;

                AVFrame frame = default;
                if (!await Task.Run(() => vsd.TryDecodeNextFrame(frame: out frame))) break;

                var convertedFrame = vfc.Convert(frame);

                FFmpegHelper.WriteFrame(convertedFrame, filePath);
            }

            var localThumbnail = new LocalThumbnail(timeStamp: currentTime);
            await localThumbnail.SetBitmap(filePath);

            progress?.Report(localThumbnail);
        }
    }

    private static string GetFilePath(ThumbnailGenerateOptions thumbnailGenerateOptions, long currentTime)
    {
        DateHelper.GetTimeFromTimeStamp(currentTime, out var hh, out var mm, out var ss);

        var filePath = Path.Combine(thumbnailGenerateOptions.SavePath,
            string.Format(thumbnailGenerateOptions.StringFormat, $"{hh:D2}_{mm:D2}_{ss:D2}") + ".jpg");
        return filePath;
    }


    private static void ConfigureHwDecoder(out AVHWDeviceType hwType)
    {
        hwType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;

        var availableHwDecoders = new Dictionary<int, AVHWDeviceType>();
        var type = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
        var number = 0;

        //var logPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "log123456.txt");

        //File.AppendAllText(logPath, $"start\r\n");
        while ((type = ffmpeg.av_hwdevice_iterate_types(type)) != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
        {
            Debug.WriteLine($"{number}. {type}");

            number++;
            //File.AppendAllText(logPath, $"{number}:{type}\r\n");
            availableHwDecoders.Add(number, type);
            //File.AppendAllText(logPath, $"done!\r\n");
        }

        if (availableHwDecoders.Count == 0)
        {
            Debug.WriteLine("Your system have no hardware decoders");
            hwType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
            return;
        }

        // 默认编码方式
        var decoderNumber = availableHwDecoders
            .SingleOrDefault(t => t.Value == AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2).Key;
        Debug.WriteLine($"Selected [{decoderNumber}]");

        // 跳过选择
        availableHwDecoders.TryGetValue(decoderNumber, out hwType);
    }



}

