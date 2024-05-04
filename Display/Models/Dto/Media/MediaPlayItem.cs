using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Display.Helper.FileProperties.Name;
using Display.Helper.Network;
using Display.Models.Enums;
using Display.Models.Vo;
using Display.Models.Vo.OneOneFive;
using Display.Providers;
using SharpCompress;
using FileInfo = DataAccess.Models.Entity.FileInfo;

namespace Display.Models.Dto.Media;

public class MediaPlayItem
{
    public readonly string FileName;
    public readonly string FileNameWithoutExtension;
    public readonly string Name;
    public readonly string PickCode;
    public readonly string Title;
    public readonly long? Fid;
    public readonly long? Size;
    public readonly long Cid;
    public string Description { get; set; }
    private List<M3U8Info> _m3U8Infos;
    private readonly FileType _type;


    private readonly IVideoInfoDao _videoInfoDao = App.GetService<IVideoInfoDao>();

    private VideoInfo _videoInfo;

    public VideoInfo GetVideoInfo()
    {
        return _videoInfo ??= _videoInfoDao.GetOneByName(Name);
    }

    public List<Quality> QualityInfos;

    public string OriginalUrl;
    public List<SubInfo> SubInfos;

    public bool IsRequestM3U8;
    public bool IsRequestOriginal;

    public MediaPlayItem(FileInfo fileInfo)
        : this(fileInfo.PickCode, fileInfo.Name, fileInfo.CurrentId, fileInfo.Id, fileInfo.Size, fileInfo.FileId == null ? FileType.Folder : FileType.File)
    {
        
    }

    public MediaPlayItem(DetailFileInfo detailFileInfo)
        : this(detailFileInfo.PickCode, detailFileInfo.Name, detailFileInfo.Cid, detailFileInfo.Id, detailFileInfo.Size, detailFileInfo.Type)
    {
    }

    private MediaPlayItem(string pickCode, string fileName, long cid, long? fid, long? size, FileType type)
    {
        Fid = fid;
        PickCode = pickCode;
        FileName = fileName;
        _webApi = WebApi.GlobalWebApi;

        FileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        Name = FileMatch.MatchName(FileNameWithoutExtension)?.ToUpper();
        Title = FileNameWithoutExtension;
        _type = type;

        Size = size;
        Cid = cid;

        if (!AppSettings.IsFindSub || string.IsNullOrEmpty(pickCode) || type == FileType.Folder) return;

        // TODO 加载字幕
        
        // // 加载字幕
        // var subArray = DataAccessLocal.Get.GetSubFile(pickCode);
        //
        // SubInfos = [];
        // subArray.ForEach(i =>
        //     SubInfos.Add(new SubInfo(i.PickCode, i.Name, Name)));
        // SubInfos = SubInfos.OrderBy(item => item.Name).ToList();
    }

    private static WebApi _webApi;

    public async Task<List<M3U8Info>> GetM3U8Infos()
    {
        if (IsRequestM3U8) return _m3U8Infos;

        // 没获取过
        _m3U8Infos = await _webApi.GetM3U8InfoByPickCode(PickCode);

        IsRequestM3U8 = true;

        return _m3U8Infos;
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

        var downUrlList = await _webApi.GetDownUrl(PickCode, DbNetworkHelper.DownUserAgent);
        IsRequestOriginal = true;

        OriginalUrl = downUrlList.FirstOrDefault().Value;
        return OriginalUrl;
    }

    public async Task<List<Quality>> GetQualities()
    {
        if (QualityInfos != null) return QualityInfos;

        //先原画
        QualityInfos = [new Quality("原画", isOriginal: true)];

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


    internal static async Task<IList<MediaPlayItem>> OpenFolderThenInsertVideoFileToMediaPlayItem(IList<MediaPlayItem> oldMediaPlayItems, WebApi webApi)
    {
        List<MediaPlayItem> newMediaPlayItems = [];

        foreach (var playItem in oldMediaPlayItems)
        {
            if (playItem._type == FileType.Folder)
            {
                var fileInfos = await webApi.GetFileAsync(playItem.Cid, loadAll: true);

                newMediaPlayItems.AddRange(
                    fileInfos.Data
                        .Where(x => x.Iv == 1)
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