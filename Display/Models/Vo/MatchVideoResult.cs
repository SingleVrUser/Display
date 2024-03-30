namespace Display.Models.Dto.OneOneFive;

public class MatchVideoResult
{
    public bool Status;

    /**
     * 1. 匹配成功、2. 匹配名称已存在（多个视频对应一个匹配名称）
     * 3. 正在搜刮、4. 搜刮完成
     */
    public int StatusCode;
    public string Message;
    public string OriginalName;
    public string MatchName;
}