using Display.Models.Image;
using FFmpeg.AutoGen.Abstractions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using static System.Int64;

namespace Display.Helper.Media;

public sealed unsafe class VideoStreamDecoder : IDisposable
{
    private readonly UrlOptions _urlOptions;
    private readonly AVHWDeviceType _hwDeviceType;
    private readonly AVFormatContext* _pFormatContext;
    private readonly AVFrame* _receivedFrame;
    private readonly AVDictionary* _options = null;
    private readonly AVFrame* _pFrame;
    private readonly AVPacket* _pPacket;

    private AVCodecContext* _pCodecContext;
    private int _streamIndex;

    public VideoStreamDecoder(UrlOptions urlOptions, AVHWDeviceType hwDeviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
    {
        _urlOptions = urlOptions;
        _hwDeviceType = hwDeviceType;
        _pFormatContext = ffmpeg.avformat_alloc_context();
        _receivedFrame = ffmpeg.av_frame_alloc();



        _pPacket = ffmpeg.av_packet_alloc();
        _pFrame = ffmpeg.av_frame_alloc();
    }

    private void OpenInput()
    {
        var options = _options;
        foreach (var (key, value) in _urlOptions.Headers)
        {
            ffmpeg.av_dict_set(&options, key, value, 0);
        }
        var pFormatContext = _pFormatContext;
        ffmpeg.avformat_open_input(&pFormatContext, _urlOptions.Url, null, &options).ThrowExceptionIfError();
    }

    public void Init()
    {
        OpenInput();

        ffmpeg.avformat_find_stream_info(_pFormatContext, null).ThrowExceptionIfError();
        AVCodec* codec = null;
        _streamIndex = ffmpeg
            .av_find_best_stream(_pFormatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, &codec, 0)
            .ThrowExceptionIfError();
        _pCodecContext = ffmpeg.avcodec_alloc_context3(codec); // _pCodecContext不能在Task.Run中，后续Dispose不在同一线程

        if (_hwDeviceType != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
        {
            ffmpeg.av_hwdevice_ctx_create(&_pCodecContext->hw_device_ctx, _hwDeviceType, null, null, 0)
                .ThrowExceptionIfError();
        }

        ffmpeg.avcodec_parameters_to_context(_pCodecContext, _pFormatContext->streams[_streamIndex]->codecpar)
            .ThrowExceptionIfError();
        ffmpeg.avcodec_open2(_pCodecContext, codec, null).ThrowExceptionIfError();

        Duration = _pFormatContext->duration;
        CodecName = ffmpeg.avcodec_get_name(codec->id);
        FrameSize = new Size(_pCodecContext->width, _pCodecContext->height);
        PixelFormat = _pCodecContext->pix_fmt;
    }

    public long Duration { get; set; }
    public string CodecName { get; set; }
    public Size FrameSize { get; set; }
    public AVPixelFormat PixelFormat { get; set; }

    public void Dispose()
    {
        var pFrame = _pFrame; ;
        ffmpeg.av_frame_free(&pFrame);

        var pPacket = _pPacket;
        ffmpeg.av_packet_free(&pPacket);

        var option = _options;
        ffmpeg.av_dict_free(&option);

        ffmpeg.avcodec_close(_pCodecContext);

        if (_pFormatContext->video_codec_id != AVCodecID.AV_CODEC_ID_NONE)
        {
            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_close_input(&pFormatContext);
        }
        else
        {
            //ffmpeg.avformat_free_context(_pFormatContext);
        }
    }

    public bool TrySeekPosition(long timestamp)
    {
        ffmpeg.avcodec_flush_buffers(_pCodecContext);
        var result = ffmpeg.avformat_seek_file(_pFormatContext, -1, MinValue, timestamp, MaxValue,
            ffmpeg.AVSEEK_FLAG_BACKWARD);
        return result >= 0;
    }

    public bool TryDecodeNextFrame(out AVFrame frame)
    {
        ffmpeg.av_frame_unref(_pFrame);
        ffmpeg.av_frame_unref(_receivedFrame);
        int error;

        do
        {
            try
            {
                do
                {
                    ffmpeg.av_packet_unref(_pPacket);
                    error = ffmpeg.av_read_frame(_pFormatContext, _pPacket);

                    if (error == ffmpeg.AVERROR_EOF)
                    {
                        frame = *_pFrame;
                        return false;
                    }

                    error.ThrowExceptionIfError();
                } while (_pPacket->stream_index != _streamIndex);

                ffmpeg.avcodec_send_packet(_pCodecContext, _pPacket).ThrowExceptionIfError();
            }
            finally
            {
                ffmpeg.av_packet_unref(_pPacket);
            }

            error = ffmpeg.avcodec_receive_frame(_pCodecContext, _pFrame);

        } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

        error.ThrowExceptionIfError();

        if (_pCodecContext->hw_device_ctx != null)
        {
            ffmpeg.av_hwframe_transfer_data(_receivedFrame, _pFrame, 0).ThrowExceptionIfError();
            frame = *_receivedFrame;
        }
        else
            frame = *_pFrame;

        return true;
    }

    public IReadOnlyDictionary<string, string> GetContextInfo()
    {
        AVDictionaryEntry* tag = null;
        var result = new Dictionary<string, string>();

        while ((tag = ffmpeg.av_dict_get(_pFormatContext->metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
        {
            var key = Marshal.PtrToStringAnsi((IntPtr)tag->key);
            var value = Marshal.PtrToStringAnsi((IntPtr)tag->value);

            if (key != null) result.Add(key, value);
        }

        return result;
    }
}