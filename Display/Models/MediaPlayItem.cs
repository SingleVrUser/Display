using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Display.Data;
using static Display.Data.WebApi;
using System.Xml.Linq;

namespace Display.Models
{
    public class MediaPlayItem
    {
        public string FileName;
        public string FileNameWithoutExtension;
        public string TrueName;
        public string PickCode;
        public string Title;
        public string Description;
        public List<m3u8Info> M3U8Infos;

        private FailInfo _failInfo;

        public async Task<FailInfo> GetFailInfo()
        {
            return _failInfo ??= await DataAccess.LoadSingleFailInfo(PickCode);
        }

        private VideoInfo _videoInfo;

        public VideoInfo GetVideoInfo()
        {
            return _videoInfo ??= DataAccess.LoadOneVideoInfoByCID(TrueName);
        }

        public List<Quality> QualityInfos;

        public string OriginalUrl;
        public List<SubInfo> SubInfos;

        public bool IsRequestM3U8 = false;
        public bool IsRequestOriginal = false;

        public MediaPlayItem(string pickCode, string fileName)
        {
            PickCode = pickCode;
            FileName = fileName;
            _webApi = WebApi.GlobalWebApi;

            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            TrueName = FileMatch.MatchName(FileNameWithoutExtension)?.ToUpper();
            Title = FileNameWithoutExtension;


            if (AppSettings.IsFindSub && string.IsNullOrEmpty(pickCode))
            {
                SubInfos = new();
                var subDict = DataAccess.FindSubFile(pickCode);

                subDict.ToList().ForEach(item => SubInfos.Add(new SubInfo(item.Key, item.Value, pickCode, TrueName)));

                SubInfos = SubInfos.OrderBy(item => item.Name).ToList();

            }
        }

        private static WebApi _webApi;

        //public async Task<string> GetVideoUrl()
        //{
        //    //首先是m3u8
        //    var videoUrl = await GetM3U8Url();

        //    //无m3u8就换为原画
        //    if (string.IsNullOrEmpty(videoUrl))
        //    {
        //        videoUrl = OriginalUrl;
        //    }

        //    return videoUrl;
        //}

        private async Task<List<m3u8Info>> GetM3U8Urls()
        {
            if (IsRequestM3U8) return M3U8Infos;

            // 没获取过
            M3U8Infos = await _webApi.GetM3U8InfoByPickCode(PickCode);

            IsRequestM3U8 = true;

            return M3U8Infos;
        }

        public async Task<string> GetM3U8Url(int index = 0)
        {
            var m3U8Infos = await GetM3U8Urls();

            if (m3U8Infos is not { Count: > 0 }) return null;

            if (index > m3U8Infos.Count + 1)
            {
                index = m3U8Infos.Count-1;
            }

            return m3U8Infos[index].Url;
        }

        public string GetOriginalUrl()
        {
            if (IsRequestOriginal)
            {
                return OriginalUrl;
            }

            var downUrlList = _webApi.GetDownUrl(PickCode, GetInfoFromNetwork.BrowserUserAgent);
            IsRequestOriginal = true;

            OriginalUrl = downUrlList.FirstOrDefault().Value;
            return OriginalUrl;
        }

        public async Task<List<Quality>> GetQualities()
        {
            if (QualityInfos != null) return QualityInfos;
            
            //先原画
            QualityInfos = new List<Quality> { new("原画", isOriginal: true) };

            M3U8Infos = await _webApi.GetM3U8InfoByPickCode(PickCode);
            //有m3u8
            if (M3U8Infos is { Count: > 0 })
            {
                //后m3u8
                M3U8Infos.ForEach(item => QualityInfos.Add(new Quality(item.Name, item.Url)));

            }

            return QualityInfos;
        }

        public async Task<string> GetUrl(int index = 1)
        {
            var qualities = await GetQualities();
            int maxIndex = qualities.Count - 1;
            if (index > qualities.Count - 1)
            {
                index = qualities.Count - 1;
            }

            var quality = qualities[index];

            if (string.IsNullOrEmpty(quality.Url) && quality.IsOriginal)
            {
                quality.Url = GetOriginalUrl();
            }

            return quality.Url;
        }

        public async Task<string> GetUrl(AppSettings.PlayQuality quality)
        {
            switch (quality)
            {
                case AppSettings.PlayQuality.M3U8:
                {
                    var m3U8Infos = await GetM3U8Urls();
                    if (m3U8Infos == null || m3U8Infos.Count == 0) return GetOriginalUrl();

                    return m3U8Infos.FirstOrDefault()?.Url;
                }
                case AppSettings.PlayQuality.Origin:
                    return GetOriginalUrl();
                default:
                    return null;
            }
        }

        private string _subFilePath;
        public async Task<string> GetOneSubFilePath()
        {
            if (_subFilePath != null) return _subFilePath;

            var firstSubInfo = SubInfos?.FirstOrDefault();
            if (firstSubInfo == null) return null;

            _subFilePath = await _webApi.TryDownSubFile(firstSubInfo.Name, firstSubInfo.PickCode);

            return _subFilePath;
        }

    }

    public class Quality
    {
        public string ShowName { get; set; }

        public string Url { get; set; }

        public bool IsOriginal { get; set; }

        public Quality(string showName, string url = null, bool isOriginal = false)
        {
            ShowName = showName;

            if (url != null) Url = url;

            IsOriginal = isOriginal;
        }

    }
}
