using System.Collections.Generic;
using DataAccess.Models.Entity;

namespace Display.Models.Records;

/// <summary>
/// 存储Cid下面的文件列表
/// </summary>
public record StoreDatum(long Cid, List<FileInfo> DatumList);