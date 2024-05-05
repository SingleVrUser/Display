using System.Collections.Concurrent;
using System.Collections.Generic;
using DataAccess.Models.Dto;
using DataAccess.Models.Entity;
using Display.Helper.Data;
using Display.Models.Spider;

namespace Display.Models.Vo.Spider;

public record SpiderItem(string Name)
{
    public ConcurrentQueue<SpiderSourceName> DoneSpiderNameArray { get; } = [];

    public VideoInfoDto Info { get; private set; }
    
    public required List<long> FileIdList { get; set; }

    public bool IsCompleted { get; private set; }

    private static readonly string[] AlternativeList = [
        nameof(Info.DirectorName),
        nameof(Info.SeriesName),
        nameof(Info.PublisherName)
    ];

    public void AddInfo(VideoInfoDto info)
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


    private void UpdateInfo(VideoInfoDto info)
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