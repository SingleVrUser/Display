using Display.Helper.Encode;
using Display.Providers;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Display.Models.Dto.OneOneFive;

namespace Display.Services;

public class MediaPlayerWithStreamSource
{
    private HttpRandomAccessStream _stream;
    private MediaSource _ms;
    public MediaPlayer MediaPlayer = new();
    public FilesInfo FilesInfo;

    public string Url { get; }

    public MediaPlayerWithStreamSource(string url, FilesInfo filesInfo)
    {
        Url = url;
        FilesInfo = filesInfo;
    }

    public static async Task<MediaPlayerWithStreamSource> CreateMediaPlayer(string videoUrl, SubInfo subInfo = null, FilesInfo filesInfo = null)
    {
        var mediaPlayerWithStreamSource = new MediaPlayerWithStreamSource(videoUrl, filesInfo);

        var mediaPlayer = mediaPlayerWithStreamSource.MediaPlayer;

        MediaSource ms = null;

        var videoHttpClient = WebApi.SingleVideoWindowWebHttpClient;
        if (videoUrl.Contains(".m3u8"))
        {
            var result = await AdaptiveMediaSource.CreateFromUriAsync(new Uri(videoUrl), videoHttpClient);

            if (result.Status == AdaptiveMediaSourceCreationStatus.Success)
            {
                ms = MediaSource.CreateFromAdaptiveMediaSource(result.MediaSource);
            }
        }

        if (ms == null)
        {
            mediaPlayerWithStreamSource._stream = await HttpRandomAccessStream.CreateAsync(videoHttpClient, new Uri(videoUrl));

            if (!mediaPlayerWithStreamSource._stream.CanRead)
            {
                return mediaPlayerWithStreamSource;
            }

            ms = MediaSource.CreateFromStream(mediaPlayerWithStreamSource._stream, "video/mp4");
        }

        mediaPlayerWithStreamSource._ms = ms;

        //添加字幕文件
        if (subInfo != null)
        {
            //下载字幕
            var subPath = await WebApi.GlobalWebApi.TryDownSubFile(subInfo.Name, subInfo.PickCode);

            if (!string.IsNullOrEmpty(subPath))
            {
                //var timedTextSource = TimedTextSource.CreateFromUri(new Uri(subPath), SubInfo.name);
                //ms.ExternalTimedTextSources.Add(timedTextSource);

                var textEncoding = FileEncodingHelper.GetEncoding(subPath);

                //如果文本格式不是UTF-8，就将文本转换为该格式
                if (!Equals(textEncoding, Encoding.UTF8))
                {
                    Debug.WriteLine("字幕不是UTF-8，转为该格式并覆盖");
                    await File.WriteAllTextAsync(subPath,
                        await File.ReadAllTextAsync(subPath, textEncoding),
                        Encoding.UTF8);
                }

                ms.ExternalTimedTextSources.Add(TimedTextSource.CreateFromUri(new Uri(subPath)));

            }

            var playbackItem = new MediaPlaybackItem(ms);


            if (ms.ExternalTimedTextSources.Count > 0)
            {
                Debug.WriteLine("发现字幕文件");

                playbackItem.TimedMetadataTracksChanged += (_, _) =>
                {
                    playbackItem.TimedMetadataTracks.SetPresentationMode(0, TimedMetadataTrackPresentationMode.PlatformPresented);

                    Debug.WriteLine("默认选中第一个字幕");

                };
            }
            else
            {
                Debug.WriteLine("没有字幕文件");
            }

            mediaPlayer.Source = playbackItem;

        }
        else
        {
            mediaPlayer.Source = ms;
        }

        return mediaPlayerWithStreamSource;
    }

    public void Dispose()
    {
        if (MediaPlayer.CanPause) MediaPlayer.Pause();

        MediaPlayer?.Dispose();
        MediaPlayer = null;

        _ms?.Dispose();
        _ms = null;
    }

}