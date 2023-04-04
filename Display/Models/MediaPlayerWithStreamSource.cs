using MediaPlayerElement_Test.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Display.Data;

namespace Display.Models
{
    public class MediaPlayerWithStreamSource
    {
        public MediaPlayer MediaPlayer = new();
        private HttpRandomAccessStream _stream;
        public FilesInfo FilesInfo;

        public string Url { get; }

        public MediaPlayerWithStreamSource(FilesInfo filesInfo, string url)
        {
            Url = url;
            FilesInfo = filesInfo;
        }

        public static async Task<MediaPlayerWithStreamSource> CreateMediaPlayer(FilesInfo filesInfo, string videoUrl)
        {
            var mediaPlayerWithStreamSource = new MediaPlayerWithStreamSource(filesInfo, videoUrl);

            var mediaPlayer = mediaPlayerWithStreamSource.MediaPlayer;
            if (videoUrl.Contains(".m3u8"))
            {
                mediaPlayer.SetUriSource(new Uri(videoUrl));
            }
            else
            {
                var videoHttpClient = WebApi.GetVideoClient();
                mediaPlayerWithStreamSource._stream = await HttpRandomAccessStream.CreateAsync(videoHttpClient, new Uri(videoUrl));
                mediaPlayer.SetStreamSource(mediaPlayerWithStreamSource._stream);
            }

            return mediaPlayerWithStreamSource;
        }

        public void Dispose()
        {
            Debug.WriteLine("Dispose MediaPlayerStreamSource");
            try
            {
                _stream?.Dispose();
                MediaPlayer?.Dispose();
                Debug.WriteLine("Dispose MediaPlayerStreamSource success");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"销毁MediaPlayerStreamSource时发生错误:{e.Message}");
            }
        }

    }
}
