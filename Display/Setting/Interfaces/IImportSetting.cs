namespace Display.Setting.Interfaces;

internal interface IImportSetting
{
    /// <summary>
    /// 115导入数据库 进程界面 的 任务完成后通知
    /// </summary>
    public bool IsToastAfterImportDataAccess { get; set; }

    /// <summary>
    /// 115导入数据库 进程界面 的 任务完成后 开始搜刮任务
    /// </summary>
    /// 
    public bool IsSpiderAfterImportDataAccess { get; set; }
}