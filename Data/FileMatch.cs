using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.System;

namespace Data
{
    public static class FileMatch
    {
        /// <summary>
        /// //正则删除某些关键词
        /// </summary>
        /// <param Name="name"></param>
        /// <returns></returns>
        public static string DeleteSomeKeywords(string name)
        {
            List<string> reg_replace_list =
                new List<string> { "uur76", @"({\d}K)?\d{2,3}fps", @"part\d", "@18P2P", @"[^\d]\d{3,6}P", @"\[?[0-9a-z]+?\.(com|cn|xyz|la|me|net|app|cc)\]?@?",
                                @"SE\d{2}",@"EP\d{2}", @"S\d{1,2}E\d{1,2}", @"\D[hx]26[54]", "[-_][468]k", @"h_[0-9]{3,4}",@"[a-z0-9]{15,}",
                                @"\d+bit",@"\d{3,6}x\d{3,6}"};
            for (int i = 0; i < reg_replace_list.Count; i++)
            {
                Regex rgx = new Regex(reg_replace_list[i], RegexOptions.IgnoreCase);
                name = rgx.Replace(name, "");
            }

            return name;
        }

        public static bool IsFC2(string cid)
        {
            return cid.Contains("FC2");
        }

        ///// <summary>
        ///// 是否是特殊番号(用于区分AvMoo和AvSox)
        ///// </summary>
        ///// <param Name="cid"></param>
        ///// <returns></returns>
        //public static bool IsSpecialCid(string cid)
        //{
        //    string CID = cid.ToUpper();

        //    //FC2 / 无分隔符 / HEY / HEYZO / 无字母 / 字母+数字+字母 / 有分隔符但是是特殊字母
        //    return IsFC2(CID) || !CID.Contains('-') || CID.Contains("HEY") || !Regex.Match(CID, "[A-Z]").Success
        //        || Regex.Match(CID, @"[A-Z]\d[A-z]").Success
        //        || (CID.Contains('-') && Regex.Match(CID, "^SKYHUD-").Success || Regex.Match(CID, "^RED-").Success || Regex.Match(CID, "^SKY-").Success)
        //        || Regex.Match(CID, "^SE-").Success || Regex.Match(CID, "^DSAMBD-").Success;
        //}
        
        /// <summary>
        /// 从文件名中匹配CID名字
        /// </summary>
        /// <param name="src_text"></param>
        /// <returns></returns>
        public static string MatchName(string src_text,string file_cid="")
        {
            //提取文件名
            string name = Regex.Match(src_text, @"(.*)(\.\w{3,5})?$",RegexOptions.IgnoreCase).Groups[1].Value;

            //删除空格
            name = src_text.Replace(" ", "_");

            //转小写
            string name_lc = name.ToLower();

            Match match;
            string no_domain = name;
            if (name_lc.Contains("fc"))
            {
                //根据FC2 Club的影片数据，FC2编号为5-7个数字
                match = Regex.Match(name, @"fc2?[^a-z\d]{0,5}(ppv[^a-z\d]{0,5})?(\d{5,7})", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    return $"FC2-{match.Groups[2].Value}";
                }
            }
            else if (name_lc.Contains("heydouga"))
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
                no_domain = DeleteSomeKeywords(name);

                if (!string.IsNullOrEmpty(no_domain) && no_domain != name)
                {
                    return MatchName(no_domain);
                }
            }

            //匹配缩写成hey的heydouga影片。由于番号分三部分，要先于后面分两部分的进行匹配
            match = Regex.Match(no_domain, @"(?:hey)[-_]*(\d{4})[-_]+0?(\d{3,5})", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return $"heydouga-" + string.Join("-", match.Groups.Values.Skip(1));
            }
            //普通番号，优先尝试匹配带分隔符的（如ABC - 123）
            match = Regex.Match(no_domain, @"([a-z]{2,10})[-_]+0*(\d{2,5})", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string number = match.Groups[2].Value;
                //不满三位数，填充0
                number = number.PadLeft(3, '0');

                return $"{match.Groups[1].Value}-{number}";
            }

            //然后再将影片视作缺失了 - 分隔符来匹配
            match = Regex.Match(no_domain, @"([a-z]{2,})0*(\d{2,5})", RegexOptions.IgnoreCase);
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
            match = Regex.Match(no_domain, @"(red[01]\d\d|sky[0-3]\d\d|ex00[01]\d)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string matchName = match.Groups[1].Value;
                match = Regex.Match(matchName, @"([a-z]+)(\d+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return $"{match.Groups[1].Value}-{match.Groups[2].Value}";
                }

                return matchName;
            }

            //尝试匹配TMA制作的影片（如'T28-557'，他家的番号很乱）
            match = Regex.Match(no_domain, @"(T28[-_]+\d{3})", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            //尝试匹配东热n, k系列
            match = Regex.Match(no_domain, @"(n\d{4}|k\d{4})", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            //尝试匹配纯数字番号（无码影片）
            match = Regex.Match(no_domain, @"(\d{6}[-_]+\d{2,3})", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            //如果还是匹配不了，尝试将')('替换为'-'后再试，少部分影片的番号是由')('分隔的
            if (no_domain.Contains(")("))
            {
                string avid = MatchName(no_domain.Replace(")(", "-"));
                if (!string.IsNullOrEmpty(avid))
                    return avid;
            }

            //如果最后仍然匹配不了番号，则尝试使用文件所在文件夹的名字去匹配
            if (!string.IsNullOrEmpty(file_cid))
            {
                var FolderDatum = DataAccess.getUpperLevelFolderCid(file_cid);

                if (!string.IsNullOrEmpty(FolderDatum.n))
                {
                    return MatchName(FolderDatum.n);
                }
            }

            return null;

        }

        /// <summary>
        /// List<class>转换,VideoInfo ==> VideoCoverDisplayClass
        /// </summary>
        /// <param Name="VideoInfoList"></param>
        /// <returns></returns>
        public static List<VideoCoverDisplayClass> getFileGrid(List<VideoInfo> VideoInfoList, double imgwidth, double imgheight)
        {
            List<VideoCoverDisplayClass> FileGrid = new();

            // VR 和 4K 类别在右上角显示标签
            // 初始化为图片大小
            for (var i = 0; i < VideoInfoList.Count; i++)
            {
                VideoCoverDisplayClass info = new(VideoInfoList[i], imgwidth,imgheight);
                FileGrid.Add(info);
            }

            return FileGrid;
        }

        public static bool isLike(int is_like)
        {
            bool result = is_like == 0 ? false : true;
            return result;
        }

        public static bool? isLookLater(long look_later)
        {
            bool result = look_later == 0 ? false : true;
            return result;
        }


        //是否显示喜欢图标
        public static Visibility isShowLikeIcon(int is_like)
        {
            return is_like == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        //根据类别搜索结果
        public static async Task<List<VideoInfo>> getVideoInfoFromType(List<string> types, string keywords, int limit)
        {
            if (types == null) return null;

            string trueType;
            //避免重复
            Dictionary<string,VideoInfo> dicts = new();

            foreach (var type in types)
            {
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
                        var failItems = await DataAccess.LoadFailFileInfoWithDatum(n: keywords, limit:limit);
                        failItems.ForEach(item => dicts.TryAdd(item.n,new(item)));
                        continue;
                    default:
                        trueType = "truename";
                        break;
                }

                int leftCount = limit - dicts.Count;

                // 当数量超过Limit数量时，跳过（不包括失败列表）
                if (leftCount <= 0) continue;

                var newItems = DataAccess.loadVideoInfoBySomeType(trueType, keywords, leftCount);

                newItems.ForEach(item => dicts.TryAdd(item.truename, item));
            }

            return dicts.Values.ToList();
        }

        public static string getVideoPlayUrl(string pickCode)
        {
            return $"https://v.anxia.com/?pickcode={pickCode}&share_id=0";
        }

        public static string ConvertInt32ToDateTime(int dateInt)
        {
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0);
            startTime = startTime.AddSeconds(dateInt).ToLocalTime();

            var result = string.Format("{0:yyyy/MM/dd H:mm}", startTime);

            return result;
        }

        public static TimeSpan CalculatTimeStrDiff(string dt1Str,string dt2Str)
        {
            if (string.IsNullOrEmpty(dt1Str) || string.IsNullOrEmpty(dt2Str)) return TimeSpan.Zero;

            DateTime dt1 = Convert.ToDateTime(dt1Str);

            DateTime dt2 = Convert.ToDateTime(dt2Str);

            if (dt2 > dt1)
            {
                return dt2 - dt1;
            }
            else
            {
                return dt1 - dt2;
            }
        }

        public static int ConvertDateTimeToInt32(string dateStr)
        {
            DateTime dt1 = new DateTime(1970, 1, 1, 8, 0, 0);
            DateTime dt2 = Convert.ToDateTime(dateStr);
            return Convert.ToInt32((dt2 - dt1).TotalSeconds);
        }

        /// <summary>
        /// 字符串内容是否为数字
        /// </summary>
        /// <param Name="_string"></param>
        /// <returns></returns>
        public static bool isNumberic1(this string _string)
        {
            if (string.IsNullOrEmpty(_string))
                return false;
            foreach (char c in _string)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        public static string ConvertInt32ToDateStr(int Second)
        {
            return ConvertDoubleToDateStr(Convert.ToDouble(Second));
        }

        public static string ConvertDoubleToDateStr(double Second)
        {
            if (Second is double.NaN)
                return Second.ToString();

            string formatStr;

            if (Second < 60)
            {
                formatStr = "ss'秒'";
            }
            else if (Second < 3600)
            {   
                formatStr = "mm'分'ss'秒'";
            }
            else if (Second < 86400)
            {
                formatStr = "hh'小时'mm'分'ss'秒'";
            }
            else
            {
                formatStr = "dd'天'hh'小时'mm'分'ss'秒'";
            }

            TimeSpan ts = TimeSpan.FromSeconds(Second);

            return ts.ToString(formatStr);
        }

        public static string ConvertPtTimeToTotalMinute(string PtTimeStr)
        {
            int totalMinute = 0;

            var match = Regex.Match(PtTimeStr, @"PT((\d+)H)?(\d+)M(\d+)S", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                int result;
                if (int.TryParse(match.Groups[2].Value, out result))
                {
                    totalMinute += result * 60;
                }

                if (int.TryParse(match.Groups[3].Value, out result))
                {
                    totalMinute += result;
                }

                return $"{totalMinute}分钟";
            }

            return PtTimeStr;
        }

        /// <summary>
        /// 从文件中挑选出视频文件
        /// </summary>
        /// <param Name="data"></param>
        /// <returns></returns>
        public static List<MatchVideoResult> GetVideoAndMatchFile(List<Datum> data)
        {
            //根据视频信息匹配视频文件
            List<MatchVideoResult> resultList = new();

            foreach (var file_info in data)
            {
                string FileName = file_info.n;

                //挑选视频文件
                if (file_info.iv == 1)
                {
                    //根据视频名称匹配番号
                    var VideoName = FileMatch.MatchName(FileName,file_info.cid);

                    //无论匹配与否，都存入数据库
                    DataAccess.AddFileToInfo(file_info.pc, VideoName);

                    //未匹配
                    if (VideoName == null)
                    {
                        resultList.Add(new MatchVideoResult() { status = false, OriginalName = file_info.n, statusCode = -1, message = "匹配失败"});
                        continue;
                    }

                    //匹配后，查询是否重复匹配
                    var existsResult = resultList.Where(x => x.MatchName == VideoName).FirstOrDefault();

                    if (existsResult == null)
                    {
                        resultList.Add(new MatchVideoResult() {status = true, OriginalName=file_info.n, message="匹配成功", statusCode = 1, MatchName = VideoName});
                    }
                    else
                    {
                        resultList.Add(new MatchVideoResult() { status = true, OriginalName = file_info.n, statusCode = 2, message = "已添加" });
                    }


                }
                else
                {
                    resultList.Add(new MatchVideoResult() { status = true, OriginalName = file_info.n, statusCode = 0, message = "跳过非视频" });
                }
            }

            return resultList;
        }

        public static List<CookieFormat> ExportCookies(string Cookies)
        {
            List<CookieFormat> cookieList = new();

            var cookiesList = Cookies.Split(';');
            foreach (var cookies in cookiesList)
            {
                var item = cookies.Split('=');

                if (item.Length != 2)
                    continue;

                string key = item[0].Trim();
                string value = item[1].Trim();
                switch (key)
                {
                    case "acw_tc":
                        cookieList.Add(new CookieFormat() {name=key,value = value, domain = "115.com", hostOnly = true}) ;
                        break;
                    case "115_lang":
                        cookieList.Add(new CookieFormat() {name = key, value = value, httpOnly = false });
                        break;
                    case "CID" or "SEID" or "UID" or "USERSESSIONID":
                        cookieList.Add(new CookieFormat() { name = key, value = value});
                        break;
                    //mini_act……_dialog_show
                    default:
                        cookieList.Add(new CookieFormat() {name = key, value = value ,session = true});
                        break;
                }
            }
            return cookieList;
        }

        public async static void LaunchFolder(string path)
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
            await Launcher.LaunchFolderAsync(folder);
        }

        public static void tryToast(string Title, string content1, string content2 = "")
        {
            new ToastContentBuilder()
                    .AddArgument("action", "viewConversation")
                    .AddArgument("conversationId", 384928)

                    .AddText(Title)

                    .AddText(content1)

                    .AddText(content2)

                    .Show();
        }

        public async static void PlayByPotPlayer(string playUrl)
        {
            string url = $"PotPlayer://{playUrl}";
            var uriDown = new Uri(url);

            // Set the option to show a warning
            var options = new Windows.System.LauncherOptions();
            options.DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseLess;

            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uriDown, options);
        }

        public static void PlayByVlc(string playUrl, string FileName, bool showWindow = true)
        {
            var process = new Process();

            process.StartInfo.FileName = FileName;
            //process.StartInfo.Arguments = @$" ""{playUrl}"" :http-referrer=""{referrerUrl}"" :http-user-agent=""{user_agnet}""";
            process.StartInfo.Arguments = @$" ""{playUrl}""";
            process.StartInfo.UseShellExecute = false;
            if (!showWindow)
            {
                process.StartInfo.CreateNoWindow = true;
            }

            process.Start();
        }

        public static void CreateDirectoryIfNotExists(string savePath)
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
        }

        //临时方法
        public static Visibility showIfImageENotNull(string imagepath)
        {
            if (imagepath == Data.Const.NoPictruePath)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        //临时方法
        public static Visibility showIfImageNull(string imagepath)
        {
            if (imagepath == Data.Const.NoPictruePath)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public static IEnumerable<T> OrderByNatural<T>(this IEnumerable<T> items, Func<T, string> selector, StringComparer stringComparer = null)
        {
            var regex = new Regex(@"\d+", RegexOptions.Compiled);

            int maxDigits = items
                          .SelectMany(i => regex.Matches(selector(i)).Cast<Match>().Select(digitChunk => (int?)digitChunk.Value.Length))
                          .Max() ?? 0;

            return items.OrderBy(i => regex.Replace(selector(i), match => match.Value.PadLeft(maxDigits, '0')), stringComparer ?? StringComparer.CurrentCulture);
        }


        public static Tuple<string,string> SpliteLeftAndRightFromCid(string cid)
        {
            string[] splitList = cid.Split(new char[] { '-', '_' });
            string leftName = splitList[0];

            string rightNumber = "";
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
}
