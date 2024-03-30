using System.Collections.Concurrent;
using Display.Helper.Data;
using Display.Models.Entities.OneOneFive;
using Display.Models.Spider;

namespace Display.Models.Vo.Spider;

public record SpiderItem(string Name)
{
    public ConcurrentQueue<SpiderSourceName> DoneSpiderNameArray { get; } = [];

    public VideoInfo Info { get; private set; }

    public bool IsCompleted { get; private set; }

    private static readonly string[] AlternativeList = [nameof(Info.Director), nameof(Info.Series), nameof(Info.Publisher)];

    public void AddInfo(VideoInfo info)
    {
        if (Info == null)
        {
            Info = info;
            IsCompleted = ClassHelper.InfoIsCompleted(info, AlternativeList);
        }
        else
        {
            UpdateInfo(info);
        }
    }


    private void UpdateInfo(VideoInfo info)
    {
        // 新数据为完整的，直接替换
        if (ClassHelper.InfoIsCompleted(Info, AlternativeList))
        {
            Info = info;
            IsCompleted = true;
        }
        else
        {
            var oldInfo = Info;
            IsCompleted = ClassHelper.UpdateOldInfoByNew(ref oldInfo, info);
            Info = oldInfo;
        }

    }
}