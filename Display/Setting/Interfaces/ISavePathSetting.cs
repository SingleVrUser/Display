namespace Display.Setting.Interfaces;

internal interface ISavePathSetting
{
    /// <summary>
    /// 图片保存地址
    /// </summary>
    public string ImageSavePath { get; set; }

    /// <summary>
    /// 演员头像保存地址
    /// </summary>
    public string ActorImageSavePath { get; set; }

    /// <summary>
    /// 字幕保存地址
    /// </summary>
    public string SubSavePath { get; set; }

    public string AttachmentSavePath { get; set; }

    /// <summary>
    /// 演员头像仓库文件保存地址
    /// </summary>
    public string ActorFileTreeSavePath { get; set; }

    /// <summary>
    /// 数据文件存储地址
    /// </summary>
    public string DataAccessSavePath { get; set; }

    public string DataSavePath { get; set; }
}