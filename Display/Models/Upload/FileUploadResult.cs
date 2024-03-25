using Display.Models.Data;
using System;

namespace Display.Models.Upload;

public class FileUploadResult(string name)
{
    public readonly string Name = name;
    public long? Id;
    public long FileSize;
    public long Cid;
    public string PickCode;
    public string Sha1;
    public string ThumbUrl;
    public int Aid;

    public FilesInfo.FileType Type = FilesInfo.FileType.File;
    public int Time = (int)DateTimeOffset.Now.ToUnixTimeSeconds();

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
        set => _isVideo = value;
    }

    public void SetFromOssUploadResult(OssUploadResult uploadResult)
    {
        var data = uploadResult?.data;
        if (data == null) return;

        PickCode = data.pick_code;
        FileSize = data.file_size;
        Id = data.file_id;
        ThumbUrl = data.thumb_url;
        Sha1 = data.sha1;
        Aid = data.aid;
        Cid = data.cid;
        IsVideo = data.is_video == 1;
    }
}