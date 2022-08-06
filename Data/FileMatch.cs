using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace Data
{
    public static class FileMatch
    {
        /// <summary>
        /// 从文件名中匹配CID名字
        /// </summary>
        /// <param name="src_text"></param>
        /// <returns></returns>
        public static string MatchName(string src_text)
        {
            string combination = null;

            //提取文件名
            string fileName = Regex.Match(src_text, @"(.*).\w{3}$").Groups[1].Value;

            //正则删除关键词
            List<string> reg_replace_list = new List<string> { "uur76", "4K60fps", @"part\d","@18P2P", "1080P", "720P", @"\[?\w+\.(com|cn|xyz|la|me|net|app|cc)\]?@?", @"S0\dE\d{1,2}", @"\D[hx]265", @"\D[hx]264", "[-_][468]k" };
            for (int i = 0; i < reg_replace_list.Count; i++)
            {
                Regex rgx = new Regex(reg_replace_list[i], RegexOptions.IgnoreCase);
                fileName = rgx.Replace(fileName, "");
            }

            //替换一些容易混淆的关键词
            fileName = fileName.Replace("gachippv", "gachi");
            fileName = fileName.Replace("caribbe", "carib");
            fileName = fileName.Replace("caribpr", "carib");

            Regex fc_rgx = new Regex("fc(2[-_ ])?[-_ ]?", RegexOptions.IgnoreCase);
            fileName = fc_rgx.Replace(fileName, "fc-");

            if (fileName == "")
            {

                //Console.WriteLine(fileName);
            }
            else if (fileName[0] != ' ')
            {
                fileName = ' ' + fileName;

                Dictionary<string, Regex> special_keywords_dict = new Dictionary<string, Regex>()
                {
                {"carbi",  new Regex(@"\d{6}[-_]\d{3}", RegexOptions.IgnoreCase)},
                {"fc", new Regex(@"\d{6,7}", RegexOptions.IgnoreCase) },
                {"heyzo", new Regex(@"\d{4}", RegexOptions.IgnoreCase) },
                {"1pon", new Regex(@"\d{6}[-_]\d{3}", RegexOptions.IgnoreCase) },
                {"heydouga", new Regex(@"\d{4}-\d{3}", RegexOptions.IgnoreCase) }
                };


                foreach (var entry in special_keywords_dict)
                {
                    if (combination != null)
                    {
                        break;
                    }

                    if (fileName.Contains(entry.Key))
                    {
                        Match match_num = entry.Value.Match(fileName);
                        if (match_num.Success)
                        {
                            string number = match_num.Value;

                            if (entry.Key == "fc")
                            {
                                combination = $"{entry.Key}2-{number}";
                            }
                            else if (entry.Key == "heyzo")
                            {
                                combination = $"{entry.Key}-{number}";
                            }
                            else if (entry.Key == "heydouga")
                            {
                                combination = $"{entry.Key}-{number}";
                            }
                            //Console.WriteLine(combination);

                        }
                    }
                }

                if (combination == null)
                {
                    string general_keywords_string = AppSettings.MatchVideoKeywordsString;

                    var general_keywords_str = general_keywords_string.Split(',');
                    var general_keywords_list = general_keywords_str.OrderByDescending(x => x.Length).ToList();

                    for (int i = 0; i < general_keywords_list.Count; i++)
                    {
                        if (combination != null)
                        {
                            break;
                        }

                        var key = general_keywords_list[i];

                        Match match_cid = Regex.Match(fileName, @"(?: h_)?[^a-zA-Z]*(" + key + @")[-_ ]?0*(\d+)", RegexOptions.IgnoreCase);
                        if (match_cid.Success)
                        {
                            string keywords = match_cid.Groups[1].Value;
                            string number = match_cid.Groups[2].Value;
                            //不满三位数，填充0
                            number = number.PadLeft(3, '0');
                            combination = $"{keywords}-{number}";
                        }
                        else
                        {
                            //纯数字系列，如加勒比111815-02
                            Match keywords_match = Regex.Match(fileName, @"\d{6}[-_]\d{2,3}", RegexOptions.IgnoreCase);
                            if (keywords_match.Success)
                            {
                                string number = keywords_match.Value;
                                combination = number;
                                Console.WriteLine($"\t{combination}");
                            }
                        }
                    }
                }

            }

            return combination;
        }


        /// <summary>
        /// List<class>转换,VideoInfo ==> VideoCoverDisplayClass
        /// </summary>
        /// <param name="VideoInfoList"></param>
        /// <returns></returns>
        public static ObservableCollection<VideoCoverDisplayClass> getFileGrid(List<VideoInfo> VideoInfoList)
        {
            ObservableCollection<VideoCoverDisplayClass> FileGrid = new();

            // VR 和 4K 类别在右上角显示标签
            for (var i = 0; i < VideoInfoList.Count; i++)
            {
                VideoCoverDisplayClass info = new(VideoInfoList[i]);
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
        public static List<VideoInfo> getVideoInfoFromType(string type, string keywords)
        {
            string trueType;
            switch (type)
            {
                case "番号" or "truename":
                    trueType = "truename";
                    //item = DataAccess.loadVideoInfoByName(keywords);
                    break;
                case "演员" or "actor":
                    trueType = "actor";
                    //item = DataAccess.loadVideoInfoByActor(keywords);
                    break;
                case "标签" or "category":
                    trueType = "category";
                    //item = DataAccess.loadVideoInfoByLabel(keywords);
                    break;
                default:
                    trueType = "truename";
                    //item = DataAccess.loadVideoInfoByName(keywords);
                    break;
            }

            List<VideoInfo> item = DataAccess.loadVideoInfoBySomeType(trueType, keywords);
            return item;
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

        public static int ConvertDateTimeToInt32(string dateStr)
        {
            DateTime dt1 = new DateTime(1970, 1, 1, 8, 0, 0);
            DateTime dt2 = Convert.ToDateTime(dateStr);
            return Convert.ToInt32((dt2 - dt1).TotalSeconds);
        }


        /// <summary>
        /// 字符串内容是否为数字
        /// </summary>
        /// <param name="_string"></param>
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


        public static string ConvertInt32ToDateStr(double Second)
        {
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


        /// <summary>
        /// 从文件中挑选出视频文件
        /// </summary>
        /// <param name="data"></param>
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
                    var VideoName = FileMatch.MatchName(FileName);

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
    }
}
