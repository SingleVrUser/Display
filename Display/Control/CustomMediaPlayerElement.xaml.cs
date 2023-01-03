// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.Storage;
using Data;
using SkiaSharp;
using static System.Net.WebRequestMethods;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Control
{
    public sealed partial class CustomMediaPlayerElement : UserControl
    {
        public static readonly DependencyProperty PickCodeProperty =
            DependencyProperty.Register("PickCode", typeof(string), typeof(CustomMediaPlayerElement), null);

        public string PickCode
        {
            get { return (string)GetValue(PickCodeProperty); }
            set { SetValue(PickCodeProperty, value); }
        }

        Dictionary<string, string> subDicts;

        private WebApi webApi;

        public event EventHandler<RoutedEventArgs> FullWindow;

        public CustomMediaPlayerElement()
        {
            this.InitializeComponent();

            this.Loaded += CustomMediaPlayerElement_Loaded;
        }

        private async void CustomMediaPlayerElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (PickCode == null) return;

            //m3u8UrlList
            if (webApi == null) webApi = new();


            //先原画
            List<Quality> QualityItemsSource = new() { new("原画", pickCode: PickCode) };

            var m3u8InfoList = await webApi.Getm3u8InfoByPickCode(PickCode);

            string url = null;

            //有m3u8
            if (m3u8InfoList != null && m3u8InfoList.Count > 0)
            {
                //后m3u8
                m3u8InfoList.ForEach(item => QualityItemsSource.Add(new(item.Name, item.Url)));

                url = m3u8InfoList.FirstOrDefault()?.Url;
            }
            //没有m3u8，获取下载链接播放
            else
            {
                var downUrlList = webApi.GetDownUrl(PickCode,GetInfoFromNetwork.MediaElementUserAgent);

                if (downUrlList.Count>0)
                {
                    url = downUrlList.FirstOrDefault().Value;
                }
            }

            mediaTransportControls.SetQuality(QualityItemsSource, this.Resources["QualityDataTemplate"] as DataTemplate);

            if (AppSettings.IsFindSub)
            {
                subDicts = DataAccess.FindSubFile(PickCode);
            }

            await SetMediaPlayer(url, subDicts);
        }

        private async Task SetMediaPlayer(string url, Dictionary<string,string> subDicts = null)
        {
            if (string.IsNullOrEmpty(url)) return;

            //添加播放链接
            MediaSource ms = MediaSource.CreateFromUri(new Uri(url));

            var media = new MediaPlayer();

            //添加字幕文件
            if (subDicts != null && subDicts.Count!=0)
            {
                ms.ExternalTimedTextSources.Clear();

                foreach (var item in subDicts)
                {
                    string subPickCode = item.Key;
                    string subName = item.Value;

                    //下载字幕
                    string subPath = await webApi.TryDownSubFile(subName, subPickCode);

                    StorageFile srtfile = await StorageFile.GetFileFromPathAsync(subPath);
                    IRandomAccessStream stream = await srtfile.OpenReadAsync();

                    TimedTextSource txtsrc = TimedTextSource.CreateFromStream(stream, subName);
                    ms.ExternalTimedTextSources.Add(txtsrc);
                }


                var playbackItem = new MediaPlaybackItem(ms);

                //选择默认字幕
                media.BufferingStarted += (sender, e) =>
                {
                    playbackItem.TimedMetadataTracks.SetPresentationMode(0, TimedMetadataTrackPresentationMode.PlatformPresented);
                };

                media.Source = playbackItem;

            }
            else
            {
                media.Source = ms;
            }


            MediaControl.SetMediaPlayer(media);
        }

        private async void mediaControls_QualityChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is not Quality quality) return;

            System.Diagnostics.Debug.WriteLine(quality.Name);
            System.Diagnostics.Debug.WriteLine(quality.Url);

            string url = null;
            //m3u8播放
            if (quality.Url != null)
            {
                url = quality.Url;
            }
            //原画播放
            else if (quality.Url == null && quality.PickCode != null)
            {
                var downUrlList = webApi.GetDownUrl(PickCode, GetInfoFromNetwork.MediaElementUserAgent);

                if (downUrlList.Count == 0) return;
                url = downUrlList.FirstOrDefault().Value;

                //避免重复获取
                quality.Url = url;
            }

            //播放
            if (url != null)
            {
                //记录当前的时间
                var time = MediaControl.MediaPlayer.Position;

                await SetMediaPlayer(url, subDicts);

                //恢复之前的时间
                MediaControl.MediaPlayer.Position = time;
            }
        }


        private void mediaControls_FullWindow(object sender, RoutedEventArgs e)
        {
            FullWindow?.Invoke(sender, e);
        }
    }
}
