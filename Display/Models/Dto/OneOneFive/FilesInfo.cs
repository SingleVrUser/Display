using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
using Display.Models.Entities._115;
using Display.Models.Upload;

namespace Display.Models.Dto.OneOneFive;

/// <summary>
/// 文件详情展示
/// </summary>
public class FilesInfo : INotifyPropertyChanged
{
    public readonly Datum Datum;
    public readonly long? Id;
    public readonly bool NoId;
    public readonly long Cid;
    public readonly long Size;
    public readonly string Sha1;
    public readonly string PickCode;
    public readonly FileType Type;
    public readonly int Time;
    public readonly bool IsVideo;
    public readonly bool IsImage;

    public readonly string ThumbnailUrl;

    private string _ico;
    public string Ico
    {
        get
        {
            if (!string.IsNullOrEmpty(_ico))
            {
                return _ico;
            }

            var nameArray = Name.Split('.');
            _ico = nameArray.Length == 1 ? string.Empty : nameArray[^1];
            return _ico;
        }
    }

    public FilesInfo(SearchDatum data)
    {
        Name = data.Name;
        Time = data.TimeEdit;

        // 文件
        if (data.Fid != null)
        {
            Type = FileType.File;
            Id = data.Fid;
            Cid = data.Cid;
            PickCode = data.Pc;
            Size = data.Size;
            Sha1 = data.Sha;

            //视频文件
            if (data.Iv == 1)
            {
                IsVideo = true;
                var videoQuality = GetVideoQualityFromVdi(data.Vdi);

                IconPath = videoQuality != null ? "ms-appx:///Assets/115/file_type/video_quality/" + videoQuality + ".svg"
                    : Constants.FileType.VideoSvgPath;
            }
            else if (!string.IsNullOrEmpty(data.Ico))
            {
                // 图片文件
                var tmpIcoPath = GetPathFromIcon(data.Ico);
                if (tmpIcoPath != null)
                {
                    IconPath = tmpIcoPath;
                }

                IsImage = GetTypeFromIcon(data.Ico) == "image";
                ThumbnailUrl = data.U;
            }

            IconPath ??= Constants.FileType.UnknownSvgPath;
        }
        //文件夹
        else if (data.Fid == null && data.Pid != null)
        {
            Type = FileType.Folder;
            Id = data.Cid;
            Cid = (long)data.Pid;
            PickCode = data.Pc;
            IconPath = Constants.FileType.FolderSvgPath;
        }

    }

    public FilesInfo(FileUploadResult result)
    {
        Name = result.Name;
        Id = result.Id;
        Cid = result.Cid;
        PickCode = result.PickCode;
        Size = result.FileSize;
        Sha1 = result.Sha1;
        Type = result.Type;
        NoId = Id == null;
        Time = (int)DateTimeOffset.Now.ToUnixTimeSeconds();

        IsVideo = result.IsVideo;

        // 不是视频文件，判断是不是图片文件
        if (!IsVideo)
        {
            IsImage = GetTypeFromIcon(Ico) == "image";

            ThumbnailUrl = result.ThumbUrl;
        }

        IconPath = GetPathFromIcon(Ico);

    }
    public FilesInfo(Datum data)
    {
        Datum = data;
        Name = data.Name;
        Time = data.TimeEdit;

        //文件夹
        if (data.Fid == null && data.Pid != null)
        {
            Type = FileType.Folder;
            Id = data.Cid;
            Cid = (long)data.Pid;
            PickCode = data.PickCode;
            IconPath = Constants.FileType.FolderSvgPath;
        }
        else if (data.Fid != null)
        {
            Type = FileType.File;
            Id = data.Fid;
            Cid = data.Cid;
            PickCode = data.PickCode;
            Size = data.Size;
            Sha1 = data.Sha;

            //视频文件
            if (data.Iv == 1)
            {
                IsVideo = true;
                var videoQuality = GetVideoQualityFromVdi(data.Vdi);

                IconPath = videoQuality != null ? "ms-appx:///Assets/115/file_type/video_quality/" + videoQuality + ".svg"
                    : Constants.FileType.VideoSvgPath;

            }
            else if (!string.IsNullOrEmpty(data.Ico))
            {
                // 图片文件
                var tmpIcoPath = GetPathFromIcon(data.Ico);
                if (tmpIcoPath != null)
                {
                    IconPath = tmpIcoPath;
                }

                IsImage = GetTypeFromIcon(data.Ico) == "image";
                ThumbnailUrl = data.U;
            }

            IconPath ??= Constants.FileType.UnknownSvgPath;

        }

        NoId = Id == null;
    }


    public void UpdateName(string name, bool isUpdateIco = false)
    {
        Name = name;
        // 清空_nameWithoutExtension，再次获取时会重新计算
        _nameWithoutExtension = null;

        if (isUpdateIco && Type == FileType.File) IconPath = GetPathFromIcon(Ico);
    }

    public static string GetVideoQualityFromVdi(int vdi)
    {
        string videoQuality = null;
        switch (vdi)
        {
            case 1:
                videoQuality = "sd";
                break;
            case 2:
                videoQuality = "hd";
                break;
            case 3:
                videoQuality = "fhd";
                break;
            case 4:
                videoQuality = "1080p";
                break;
            case 5:
                videoQuality = "4k";
                break;
            case 100:
                videoQuality = "origin";
                break;
        }
        return videoQuality;
    }

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    private string _nameWithoutExtension;
    public string NameWithoutExtension
    {
        get
        {
            if (_nameWithoutExtension != null) return _nameWithoutExtension;

            var ico = Ico;
            if (Type == FileType.File && !string.IsNullOrEmpty(ico))
            {
                _nameWithoutExtension = Regex.Replace(Name, $".{ico}$", "", RegexOptions.IgnoreCase);
            }
            else
            {
                _nameWithoutExtension = Name;
            }

            return _nameWithoutExtension;
        }
    }

    private string _iconPath;
    public string IconPath
    {
        get => _iconPath;
        set
        {
            _iconPath = value;
            OnPropertyChanged();
        }
    }

    public static string GetPathFromIcon(string ico)
    {
        string iconPath = null;

        var isMatch = false;
        foreach (var (key, values) in Constants.FileType.FileTypeDictionary)
        {
            if (isMatch)
                break;

            foreach (var (key2, values2) in values)
            {
                if (isMatch)
                    break;

                foreach (var key3 in values2)
                {
                    if (key3 != ico) continue;

                    var tmpStringBuilder = new StringBuilder(Constants.FileType.FileTypeBasePath).Append(key).Append('/').Append(key2).Append(".svg");
                    if (File.Exists(Path.Combine(Package.Current.InstalledLocation.Path, tmpStringBuilder.ToString())))
                    {
                        iconPath = tmpStringBuilder.Insert(0, Constants.FileType.MsUri).ToString();
                    }
                    else
                    {
                        var newStringBuilder = new StringBuilder(Constants.FileType.FileTypeFullBasePath).Append(key).Append('/').Append(key).Append(".svg");
                        iconPath = newStringBuilder.ToString();
                    }
                    isMatch = true;
                    break;
                }
            }
        }

        return string.IsNullOrEmpty(iconPath) ? Constants.FileType.UnknownSvgPath : iconPath;
    }

    public static string GetTypeFromIcon(string ico)
    {
        foreach (var (key, values) in Constants.FileType.FileTypeDictionary)
        {
            foreach (var (_, values2) in values)
            {
                foreach (var value3 in values2)
                {
                    if (value3 != ico) continue;

                    return key;
                }
            }
        }

        return "unknown";
    }

    public static string GetFileIconFromType(FileType fileType)
    {
        var iconUrl = Constants.FileType.UnknownSvgPath;

        switch (fileType)
        {
            case FileType.Folder:
                iconUrl = Constants.FileType.FolderSvgPath;
                break;
            case FileType.File:
                iconUrl = Constants.FileType.UnknownSvgPath;
                break;
        }

        return iconUrl;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public enum FileType { Folder, File };
}