using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Display.Models.Data;
using Microsoft.UI.Xaml;
using SharpCompress;
using Windows.Storage.Pickers;

namespace Display.Helper.FileProperties.Name;

public static class FileMatch
{
    /// <summary>
    /// 正则删除某些关键词
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string DeleteSomeKeywords(string name)
    {
        var regReplaceList =
            new List<string> { "uur76", @"({\d}K)?\d{2,3}fps\d{0,}", @"part\d", "@18P2P", @"[^\d]\d{3,6}P", @"\[?[0-9a-z]+?[\._](com|cn|xyz|la|me|net|app|cc)\]?@?",
                                @"SE\d{2}",@"EP\d{2}", @"S\d{1,2}E\d{1,2}", @"\D[hx]26[54]", "[-_][468]k", @"h_[0-9]{3,4}",@"[a-z0-9]{15,}",
                                @"\d+bit",@"\d{3,6}x\d{3,6}", @"whole[-_\w]+$"};

        foreach (var t in regReplaceList)
        {
            Regex rgx = new Regex(t, RegexOptions.IgnoreCase);
            name = rgx.Replace(name, "");
        }

        return name;
    }

    public static bool IsFc2(string cid)
    {
        return cid.Contains("FC2");
    }

    /// <summary>
    /// 从文件名中匹配CID名字
    /// </summary>
    /// <param name="srcText"></param>
    /// <param name="fileCid">父级目录的id，通过数据库获取id对应的目录名</param>
    /// <returns></returns>
    public static string MatchName(string srcText, long? fileCid = null)
    {
        //提取文件名
        var name = Regex.Replace(srcText, @"(\.\w{3,5}?)$", "", RegexOptions.IgnoreCase);

        //删除空格
        name = name.Replace(" ", "_");

        //转小写
        var nameLc = name.ToLower();

        Match match;
        var noDomain = name;
        if (nameLc.Contains("fc"))
        {
            //根据FC2 Club的影片数据，FC2编号为5-7个数字
            match = Regex.Match(name, @"fc2?[^a-z\d]{0,5}(ppv[^a-z\d]{0,5})?(\d{5,7})", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return $"FC2-{match.Groups[2].Value}";
            }
        }
        else if (nameLc.Contains("heydouga"))
        {
            match = Regex.Match(name, @"(heydouga)[-_]*(\d{4})[-_]+0?(\d{3,5})", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return string.Join("-", match.Groups.Values.Skip(1));
            }
        }
        else
        {
            //先尝试移除可疑关键词进行匹配，如果匹配不到再使用去掉关键词的名称进行匹配
            noDomain = DeleteSomeKeywords(name);

            if (!string.IsNullOrEmpty(noDomain) && noDomain != name)
            {
                return MatchName(noDomain);
            }
        }

        //匹配缩写成hey的heydouga影片。由于番号分三部分，要先于后面分两部分的进行匹配
        match = Regex.Match(noDomain, @"(?:hey)[-_]*(\d{4})[-_]+0?(\d{3,5})", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return $"heydouga-" + string.Join("-", match.Groups.Values.Skip(1));
        }
        //普通番号，优先尝试匹配带分隔符的（如ABC - 123）
        match = Regex.Match(noDomain, @"([a-z]{2,10})[-_]+0*(\d{2,5})", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            string number = match.Groups[2].Value;
            //不满三位数，填充0
            number = number.PadLeft(3, '0');

            return $"{match.Groups[1].Value}-{number}";
        }

        //然后再将影片视作缺失了 - 分隔符来匹配
        match = Regex.Match(noDomain, @"([a-z]{2,})0*(\d{2,5})", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            string number = match.Groups[2].Value;
            //不满三位数，填充0
            number = number.PadLeft(3, '0');

            return $"{match.Groups[1].Value}-{number}";
        }

        //普通番号，运行到这里时表明无法匹配到带分隔符的番号
        //先尝试匹配东热的red, sky, ex三个不带 - 分隔符的系列
        //（这三个系列已停止更新，因此根据其作品编号将数字范围限制得小一些以降低误匹配概率）
        match = Regex.Match(noDomain, @"(red[01]\d\d|sky[0-3]\d\d|ex00[01]\d)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var matchName = match.Groups[1].Value;
            match = Regex.Match(matchName, @"([a-z]+)(\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return $"{match.Groups[1].Value}-{match.Groups[2].Value}";
            }

            return matchName;
        }

        //尝试匹配TMA制作的影片（如'T28-557'，他家的番号很乱）
        match = Regex.Match(noDomain, @"(T28[-_]+\d{3})", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        //尝试匹配东热n, k系列
        match = Regex.Match(noDomain, @"(n\d{4}|k\d{4})", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        //尝试匹配纯数字番号（无码影片）
        match = Regex.Match(noDomain, @"(\d{6}[-_]+\d{2,3})", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        //如果还是匹配不了，尝试将')('替换为'-'后再试，少部分影片的番号是由')('分隔的
        if (noDomain.Contains(")("))
        {
            string avid = MatchName(noDomain.Replace(")(", "-"));
            if (!string.IsNullOrEmpty(avid))
                return avid;
        }

        //如果最后仍然匹配不了番号，则尝试使用文件所在文件夹的名字去匹配
        if (fileCid != null)
        {
            var folderDatum = DataAccess.Get.GetUpperLevelFolderCid((long)fileCid);

            if (!string.IsNullOrEmpty(folderDatum?.Name))
            {
                return MatchName(folderDatum.Name);
            }
        }

        return null;

    }

    /// <summary>
    /// List(class)转换,VideoInfo ==> VideoCoverDisplayClass
    /// </summary>
    /// <param name="videoInfoList"></param>
    /// <param name="imgWidth"></param>
    /// <param name="imgHeight"></param>
    /// <returns></returns>
    public static List<VideoCoverDisplayClass> GetFileGrid(List<VideoInfo> videoInfoList, double imgWidth, double imgHeight)
    {
        List<VideoCoverDisplayClass> fileGrid = new();

        // VR 和 4K 类别在右上角显示标签
        // 初始化为图片大小
        foreach (var t in videoInfoList)
        {
            VideoCoverDisplayClass info = new(t, imgWidth, imgHeight);
            fileGrid.Add(info);
        }

        return fileGrid;
    }

    public static bool IsLike(int isLike)
    {
        return isLike != 0;
    }

    public static bool? IsLookLater(long lookLater)
    {
        return lookLater != 0;
    }

    //是否显示喜欢图标
    public static Visibility IsShowLikeIcon(int isLike)
    {
        return isLike == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    //根据类别搜索结果
    public static async Task<List<VideoInfo>> GetVideoInfoFromType(List<string> types, string keywords, int limit)
    {
        if (types == null) return null;

        //避免重复
        Dictionary<string, VideoInfo> dictionary = new();

        foreach (var type in types)
        {
            string trueType;
            switch (type)
            {
                case "番号" or "truename":
                    trueType = "truename";
                    break;
                case "演员" or "actor":
                    trueType = "actor";
                    break;
                case "标签" or "category":
                    trueType = "category";
                    break;
                case "标题" or "title":
                    trueType = "title";
                    break;
                case "片商" or "producer":
                    trueType = "producer";
                    break;
                case "导演" or "director":
                    trueType = "director";
                    break;
                //失败比较特殊
                //从另外的表中查找
                case "失败" or "fail":
                    var failItems = await DataAccess.Get.GetFailFileInfoWithDatum(n: keywords, limit: limit);
                    failItems?.ForEach(item => dictionary.TryAdd(item.Name, new FailVideoInfo(item)));
                    continue;
                default:
                    trueType = "truename";
                    break;
            }

            var leftCount = limit - dictionary.Count;

            // 当数量超过Limit数量时，跳过（不包括失败列表）
            if (leftCount <= 0) continue;

            var newItems = DataAccess.Get.GetVideoInfoBySomeType(trueType, keywords, leftCount);

            newItems?.ForEach(item => dictionary.TryAdd(item.trueName, item));
        }

        return dictionary.Values.ToList();
    }

    public static string GetVideoPlayUrl(string pickCode)
    {
        return $"https://v.anxia.com/?pickcode={pickCode}&share_id=0";
    }

    /// <summary>
    /// 从文件中挑选出视频文件
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static List<MatchVideoResult> GetVideoAndMatchFile(List<Datum> data)
    {
        //根据视频信息匹配视频文件
        List<MatchVideoResult> resultList = new();

        foreach (var fileInfo in data)
        {
            var fileName = fileInfo.Name;

            //挑选视频文件
            if (fileInfo.Iv == 1)
            {
                //根据视频名称匹配番号
                var videoName = MatchName(fileName, fileInfo.Cid);

                //无论匹配与否，都存入数据库
                DataAccess.Add.AddFileToInfo(fileInfo.PickCode, videoName, isReplace: true);

                //未匹配
                if (videoName == null)
                {
                    resultList.Add(new MatchVideoResult() { status = false, OriginalName = fileInfo.Name, statusCode = -1, message = "匹配失败" });
                    continue;
                }

                //匹配后，查询是否重复匹配
                var existsResult = resultList.FirstOrDefault(x => x.MatchName == videoName);

                if (existsResult == null)
                {
                    resultList.Add(new MatchVideoResult() { status = true, OriginalName = fileInfo.Name, message = "匹配成功", statusCode = 1, MatchName = videoName });
                }
                else
                {
                    resultList.Add(new MatchVideoResult() { status = true, OriginalName = fileInfo.Name, statusCode = 2, message = "已添加" });
                }


            }
            else
            {
                resultList.Add(new MatchVideoResult() { status = true, OriginalName = fileInfo.Name, statusCode = 0, message = "跳过非视频" });
            }
        }

        return resultList;
    }

    public static List<CookieFormat> ExportCookies(string cookie)
    {
        List<CookieFormat> cookieList = new();

        var cookiesList = cookie.Split(';');
        foreach (var cookies in cookiesList)
        {
            var item = cookies.Split('=');

            if (item.Length != 2)
                continue;

            var key = item[0].Trim();
            var value = item[1].Trim();
            switch (key)
            {
                case "acw_tc":
                    cookieList.Add(new CookieFormat() { name = key, value = value, domain = "115.com", hostOnly = true });
                    break;
                case "115_lang":
                    cookieList.Add(new CookieFormat() { name = key, value = value, httpOnly = false });
                    break;
                case "CID" or "SEID" or "UID" or "USERSESSIONID":
                    cookieList.Add(new CookieFormat() { name = key, value = value });
                    break;
                //mini_act……_dialog_show
                default:
                    cookieList.Add(new CookieFormat { name = key, value = value, session = true });
                    break;
            }
        }
        return cookieList;
    }

    public static async void LaunchFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        var folder = await StorageFolder.GetFolderFromPathAsync(path);
        await Launcher.LaunchFolderAsync(folder);
    }

    public static async Task<StorageFolder> OpenFolder(object target, PickerLocationId suggestedStartLocation)
    {
        FolderPicker folderPicker = new();
        folderPicker.FileTypeFilter.Add("*");
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(target);
        folderPicker.SuggestedStartLocation = suggestedStartLocation;

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        return await folderPicker.PickSingleFolderAsync();
    }

    public static async Task<StorageFile> SelectFileAsync(Window window, IList<string> fileTypeFilters = null)
    {
        FileOpenPicker fileOpenPicker = new();
        fileTypeFilters?.ForEach(fileOpenPicker.FileTypeFilter.Add);
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        fileOpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

        WinRT.Interop.InitializeWithWindow.Initialize(fileOpenPicker, hwnd);

        return await fileOpenPicker.PickSingleFileAsync();
    }

    public static void CreateDirectoryIfNotExists(string savePath)
    {
        if (string.IsNullOrEmpty(savePath)) return;

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
    }

    //临时方法
    public static Visibility ShowIfImageNotNull(string imagePath)
    {
        return imagePath == Constant.FileType.NoPicturePath ? Visibility.Collapsed : Visibility.Visible;

    }

    //临时方法
    public static Visibility ShowIfImageNull(string imagePath)
    {
        return imagePath == Constant.FileType.NoPicturePath ? Visibility.Visible : Visibility.Collapsed;
    }

    public static IEnumerable<T> OrderByNatural<T>(this IEnumerable<T> items, Func<T, string> selector, StringComparer stringComparer = null)
    {
        var regex = new Regex(@"\d+", RegexOptions.Compiled);

        var enumerable = items.ToList();
        var maxDigits = enumerable
                      .SelectMany(i => regex.Matches(selector(i)).Cast<Match>().Select(digitChunk => (int?)digitChunk.Value.Length))
                      .Max() ?? 0;

        return enumerable.OrderBy(i => regex.Replace(selector(i), match => match.Value.PadLeft(maxDigits, '0')), stringComparer ?? StringComparer.CurrentCulture);
    }

    public static Tuple<string, string> SplitLeftAndRightFromCid(string cid)
    {
        var splitList = cid.Split('-', '_');
        var leftName = splitList[0];

        var rightNumber = string.Empty;
        if (splitList.Length != 1)
        {
            rightNumber = splitList[1];
        }
        else
        {
            //SE221
            var result = Regex.Match(leftName, "^([a-z]+)([0-9]+)$", RegexOptions.IgnoreCase);
            if (result.Success)
            {
                leftName = result.Groups[1].Value;
                rightNumber = result.Groups[2].Value;
            }
        }

        return Tuple.Create(leftName, rightNumber);
    }
}
