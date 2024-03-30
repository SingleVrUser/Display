using Display.Models.Api.OneOneFive.File;

namespace Display.Models.Records;

/// <summary>
/// 存储Cid下面的文件列表
/// </summary>
public record StoreDatum(long Cid, Datum[] DatumList)
{
}