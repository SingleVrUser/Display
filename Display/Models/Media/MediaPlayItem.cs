using Display.Helper.FileProperties.Name;
using Display.Models.Data;
using Display.Models.Data.Enums;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace Display.Models.Media
{
    public class MediaPlayItem
    {
        public readonly string FileName;
        public readonly string FileNameWithoutExtension;
        public readonly string TrueName;
        public readonly string PickCode;
        public readonly string Title;
        public readonly long? Fid;
        public readonly long? Size;
        public readonly long Cid;
        public string Description;
        public List<m3u8Info> M3U8Infos;

        public FilesInfo.FileType Type;

        private FailInfo _failInfo;

        public FailInfo GetFailInfo()
        {
            return _failInfo ??= DataAccess.Get.GetSingleFailInfoByPickCode(PickCode);
        }

        private VideoInfo _videoInfo;

        public VideoInfo GetVideoInfo()
        {
            return _videoInfo ??= DataAccess.Get.GetSingleVideoInfoByTrueName(TrueName);
        }

        public List<Quality> QualityInfos;

        public string OriginalUrl;
        public List<SubInfo> SubInfos;

        public bool IsRequestM3U8;
        public bool IsRequestOriginal;

        public MediaPlayItem(FilesInfo filesInfo)
            : this(filesInfo.PickCode, filesInfo.Name, filesInfo.Cid, filesInfo.Id, filesInfo.Size, filesInfo.Type)
        {
        }

        public MediaPlayItem(Datum datum)
            : this(datum.PickCode, datum.Name, datum.Cid, datum.Fid, datum.Size, datum.Fid is null ? FilesInfo.FileType.Folder : FilesInfo.FileType.File)
        {
        }

        public MediaPlayItem(FailVideoInfo failInfo)
            : this(failInfo.PickCode, failInfo.FileName, failInfo.Cid, failInfo.Fid, failInfo.Size, FilesInfo.FileType.File)
        {

        }

        public MediaPlayItem(string pickCode, string fileName, long cid, long? fid, long? size, FilesInfo.FileType type)
        {
            Fid = fid;
            PickCode = pickCode;
            FileName = fileName;
            _webApi = WebApi.GlobalWebApi;

            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            TrueName = FileMatch.MatchName(FileNameWithoutExtension)?.ToUpper();
            Title = FileNameWithoutExtension;
            Type = type;

            Size = size;
            Cid = cid;

            if (!AppSettings.IsFindSub || string.IsNullOrEmpty(pickCode) || type == FilesInfo.FileType.Folder) return;

            // 加载字幕
            SubInfos = new List<SubInfo>();
            var subDict = DataAccess.Get.GetSubFile(pickCode);
            subDict.ToList().ForEach(item => SubInfos.Add(new SubInfo(item.Key, item.Value, pickCode, TrueName)));
            SubInfos = SubInfos.OrderBy(item => item.Name).ToList();
        }

        private static WebApi _webApi;

        public async Task<List<m3u8Info>> GetM3U8Infos()
        {
            if (IsRequestM3U8) return M3U8Infos;

            // 没获取过
            M3U8Infos = await _webApi.GetM3U8InfoByPickCode(PickCode);

            IsRequestM3U8 = true;

            return M3U8Infos;
        }

        public MediaPlaybackItem MediaPlaybackItem { get; set; }

        public async Task<string> GetM3U8Url(int index = 0)
        {
            var m3U8Infos = await GetM3U8Infos();

            if (m3U8Infos is not { Count: > 0 }) return null;

            if (index > m3U8Infos.Count + 1)
            {
                index = m3U8Infos.Count - 1;
            }

            return m3U8Infos[index].Url;
        }

        public async Task<string> GetOriginalUrl()
        {
            if (IsRequestOriginal)
            {
                return OriginalUrl;
            }

            var downUrlList = await _webApi.GetDownUrl(PickCode, GetInfoFromNetwork.DownUserAgent);
            IsRequestOriginal = true;

            OriginalUrl = downUrlList.FirstOrDefault().Value;
            return OriginalUrl;
        }

        public async Task<List<Quality>> GetQualities()
        {
            if (QualityInfos != null) return QualityInfos;

            //先原画
            QualityInfos = new List<Quality> { new("原画", isOriginal: true) };

            var m3U8Infos = await GetM3U8Infos();
            //有m3u8
            if (m3U8Infos is { Count: > 0 })
            {
                //后m3u8
                m3U8Infos.ForEach(item => QualityInfos.Add(new Quality(item.Name, item.Url)));

            }

            return QualityInfos;
        }

        public async Task<string> GetUrl(int index = 1)
        {
            var qualities = await GetQualities();
            int maxIndex = qualities.Count - 1;
            if (index > maxIndex)
            {
                index = maxIndex;
            }

            var quality = qualities[index];

            if (string.IsNullOrEmpty(quality.Url) && quality.IsOriginal)
            {
                quality.Url = await GetOriginalUrl();
            }

            return quality.Url;
        }

        public async Task<string> GetUrl(PlayQuality quality)
        {
            switch (quality)
            {
                case PlayQuality.M3U8:
                    {
                        var m3U8Infos = await GetM3U8Infos();
                        if (m3U8Infos == null || m3U8Infos.Count == 0) return await GetOriginalUrl();

                        return m3U8Infos.FirstOrDefault()?.Url;
                    }
                case PlayQuality.Origin:
                    return await GetOriginalUrl();
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


        public static async Task<IList<MediaPlayItem>> OpenFolderThenInsertVideoFileToMediaPlayItem(IList<MediaPlayItem> oldMediaPlayItems, WebApi webApi)
        {
            List<MediaPlayItem> newMediaPlayItems = new();

            foreach (var playItem in oldMediaPlayItems)
            {
                if (playItem.Type == FilesInfo.FileType.Folder)
                {
                    var fileInfos = await webApi.GetFileAsync(playItem.Cid, loadAll: true);

                    newMediaPlayItems.AddRange(
                        fileInfos.data
                            .Where(x => x.Fid != null && x.Iv == 1)
                            .Select(x => new MediaPlayItem(x)));
                }
                else
                {
                    newMediaPlayItems.Add(playItem);
                }


            }


            return newMediaPlayItems;
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
