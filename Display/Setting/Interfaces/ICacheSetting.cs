namespace Display.Setting.Interfaces;

internal interface ICacheSetting
{
    /// <summary>
    /// 记录获取演员信息的进度
    /// </summary>
    public int GetActorInfoLastIndex { get; set; }
}