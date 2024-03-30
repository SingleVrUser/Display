using Display.Models.Api.OneOneFive.Upload;

namespace Display.Models.Vo.OneOneFive;

public class FileUploadResult(string name)
{
    public readonly string Name = name;
    public long? Id;
    public long FileSize;
    public long Cid;
    public string PickCode;
    public string Sha1;
    public string ThumbUrl;

    public readonly FilesInfo.FileType Type = FilesInfo.FileType.File;

    public bool Success;

    private bool? _isVideo;

    public bool IsVideo
    {
        get
        {
            if (_isVideo != null) return (bool)_isVideo;

            _isVideo = FilesInfo.GetTypeFromIcon(Name.Split('.')[^1]) == "video";
            return (bool)_isVideo;
        }
        private set => _isVideo = value;
    }

    public void SetFromOssUploadResult(OssUploadResult uploadResult)
    {
        var data = uploadResult?.Data;
        if (data == null) return;

        PickCode = data.PickCode;
        FileSize = data.FileSize;
        Id = data.FileId;
        ThumbUrl = data.ThumbUrl;
        Sha1 = data.Sha1;
        Cid = data.Cid;
        IsVideo = data.IsVideo == 1;
    }
}