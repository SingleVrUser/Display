using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Display.Extensions;
using Display.Helper.FileProperties.Name;
using Display.Models.Enums.OneOneFive;
using Display.Providers.Spider;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static System.Int32;

namespace Display.Providers;

public static class DataAccessLocal
{
    public static string DbPath => NewDbPath(AppSettings.DataAccessSavePath);

    public static string NewDbPath(string newPath)
    {
        return Path.Combine(newPath, Context.DbName);
    }

    private static readonly IActorInfoDao ActorInfoDao = App.GetService<IActorInfoDao>();

    public class Add
    {
        /// <summary>
        /// 添加视频信息
        /// </summary>
        /// <param name="data"></param>
        public static async Task AddVideoInfo_ActorInfo_IsWmAsync(VideoInfo data)
        {
            Context.Instance.VideoInfos.Add(data);
            
            //添加演员信息
            AddActorInfoByActorInfo(data, [data.TrueName]);

            //添加是否步兵
            Context.Instance.IsWms.Add(new IsWm
            {
                Truename = data.TrueName,
                IsWm1 = data.IsWm
            });

            if (data.Producer != null)
            {
                //添加厂商信息
                var producerInfo = Context.Instance.ProducerInfos.FirstOrDefault(i=>i.Name == data.Producer);

                if (producerInfo == null)
                {
                    Context.Instance.ProducerInfos.Add(new ProducerInfo
                    {
                        Name = data.Producer,
                        IsWm = data.IsWm
                    });
                }
            }
            
            await Context.Instance.SaveChangesAsync();
        }

        /// <summary>
        /// 升级ActorInfo
        /// </summary>
        /// <param name="videoInfo"></param>
        /// <param name="videoNameList"></param>
        private static void AddActorInfoByActorInfo(VideoInfo videoInfo, List<string> videoNameList)
        {
            var actorStr = videoInfo.Actor;
            if (actorStr == null) return;
            
            var actorList = actorStr.Split(",");
            foreach (var actorName in actorList)
            {
                //查询Actor_ID
                var actorInfo = ActorInfoDao.GetPartInfoByActorName(JavDb.TrimGenderFromActorName(actorName));
                
                if(actorInfo != null)
                {
                    //添加信息，如果已经存在则忽略
                    Context.Instance.ActorVideos.Add(new ActorVideo
                    {
                        ActorId = actorInfo.Id,
                        VideoName = videoInfo.TrueName
                    });
                }
                // 没有该演员信息的话
                // 新添加演员信息
                else
                {
                    AddActorInfo(actorName, videoNameList);
                }
            }

        }

        /// <summary>
        /// 插入演员信息并返回actor_id
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static long AddActorInfoAndReturnId(ActorInfo info)
        {
            Context.Instance.ActorInfos.Add(info);
            Context.Instance.SaveChanges();
            
            // TODO 插入后可直接返回
            Debug.WriteLine(info.Id);

            var firstInfo = Context.Instance.ActorInfos.FirstOrDefault(i=>i.Name==info.Name);
            return firstInfo!.Id;
        }

        /// <summary>
        /// 添加演员信息
        /// </summary>
        /// <param name="actorName"></param>
        /// <param name="videoNameList"></param>
        private static void AddActorInfo(string actorName, List<string> videoNameList)
        {
            string singleActorName;
            var isWoman = 1;
            string[] otherNames = null;

            // 获取演员名称
            if (actorName != null)
            {
                //针对演员的别名，添加到Actor_Names
                var matchResult = Regex.Match(actorName, "(.*)[（（](.*)[)）]");
                if (matchResult.Success)
                {
                    singleActorName = matchResult.Groups[1].Value;
                    isWoman = singleActorName.EndsWith(JavDb.ManSymbol) ? 0 : 1;
                    singleActorName = JavDb.TrimGenderFromActorName(singleActorName);

                    otherNames = matchResult.Groups[2].Value.Split("、");
                }
                else
                {
                    isWoman = actorName.EndsWith(JavDb.ManSymbol) ? 0 : 1;
                    singleActorName = JavDb.TrimGenderFromActorName(actorName);
                }
            }
            else
            {
                singleActorName = string.Empty;
            }

            //插入演员信息（不存在时才插入）
            
            var actorInfoFromDb = ActorInfoDao.GetPartInfoByActorName(singleActorName);
            long actorId;
            //数据库中不存在该名称
            if (actorInfoFromDb == null)
            {
                ActorInfo actorInfo = new() { Name = singleActorName, IsWoman = isWoman , BwhInfo = new Bwh()};

                //检查演员图片是否存在
                var imagePath = Path.Combine(AppSettings.ActorInfoSavePath, singleActorName, "face.jpg");
                if (File.Exists(imagePath))
                {
                    actorInfo.ProfilePath = imagePath;
                }

                actorId = AddActorInfoAndReturnId(actorInfo);
            }
            else
            {
                actorId = actorInfoFromDb.Id;
                
                //为了弥补，之前所有的is_woman都默认为1
                if (isWoman == 0)
                {
                    Context.Instance.ActorInfos.Update(new ActorInfo { Id = actorId, IsWoman = isWoman });
                }
            }

            //添加Actor_Names
            //主名称
            Context.Instance.ActorNames.Add(new ActorName
            {
                Id = actorId,
                Name = singleActorName
            });

            //别名
            if (otherNames != null)
            {
                foreach (var name in otherNames)
                {
                    Context.Instance.ActorNames.Add(new ActorName
                    {
                        Id = actorId,
                        Name = name
                    });
                }
            }

            //添加演员和作品的信息
            foreach (var videoName in videoNameList)
            {

                Context.Instance.ActorVideos.Add(new ActorVideo
                {
                    ActorId = actorId,
                    VideoName = videoName
                });
            }
        }
    }

    public class Get
    {
        /// <summary>
        /// 通过trueName查询文件列表
        /// </summary>
        /// <param name="trueName"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static FilesInfo[] GetFilesInfoByTrueName(string trueName)
        {
            return Context.Instance.Database.SqlQuery<FilesInfo>(
                $"SELECT * FROM FilesInfo,FileToInfo WHERE FilesInfo.pc == FileToInfo.file_pickcode AND FileToInfo.truename == '{trueName}' COLLATE NOCASE").ToArray();
        }

        /// <summary>
        /// 查找truename
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetOneTrueNameByName(string name)
        {
            if (name.Contains('_'))
            {
                return Context.Instance.Database
                    .SqlQuery<string>(
                        $"SELECT truename FROM VideoInfo WHERE truename COLLATE NOCASE in ('{name}', '{name.Replace('_', '-')}', '{name.Replace("_", "")}' ) Limit 1")
                    .FirstOrDefault();
            }

            return Context.Instance.Database
                .SqlQuery<string>(
                    $"SELECT truename FROM VideoInfo WHERE truename COLLATE NOCASE in ('{name}', '{name.Replace("-", "_")}', '{name.Replace("-", "")}') Limit 1")
                .FirstOrDefault();

        }

        public static FailListIsLikeLookLater GetSingleFailInfoByPickCode(string pc, SqliteConnection connection = null)
        {
            var commandText =
                $"SELECT info.*,fail.is_like, fail.score, fail.look_later, fail.image_path  FROM FailList_islike_looklater as fail LEFT JOIN FilesInfo as info on info.pc = fail.pc WHERE fail.pc = '{pc}' LIMIT 1";

            return Context.Instance.FailListIsLikeLookLater.FromSqlRaw(commandText).FirstOrDefault();
        }

        /// <summary>
        /// 查询失败列表（FailInfo格式）
        /// </summary>
        /// <returns></returns>
        public static FailListIsLikeLookLater[] GetFailFileInfoWithFailInfo(int offset = 0, int limit = -1, FailInfoShowType showType = FailInfoShowType.like, SqliteConnection connection = null)
        {
            var showTypeStr = showType == FailInfoShowType.like ? " WHERE is_like = 1" : " WHERE look_later != 0";

            var commandText =
                $"SELECT info.*,fail.is_like, fail.score, fail.look_later, fail.image_path  FROM FailList_islike_looklater as fail LEFT JOIN FilesInfo as info on info.pc = fail.pc{showTypeStr} LIMIT {limit} offset {offset}";

            return Context.Instance.FailListIsLikeLookLater.FromSqlRaw(commandText).ToArray();
        }

        /// <summary>
        /// 查询失败列表（FilesInfo格式）
        /// </summary>
        /// <returns></returns>
        public static FilesInfo[] GetFailFileInfoWithFilesInfo(int offset = 0, int limit = -1, string n = null, string orderBy = null, bool isDesc = false, FailType showType = FailType.All, SqliteConnection connection = null)
        {
            var orderStr = GetOrderStr(orderBy, isDesc);

            var queryStr = string.IsNullOrEmpty(n) ? string.Empty : $" And FilesInfo.n LIKE '%{n.Replace("'", "%")}%'";

            var showTypeStr = showType switch
            {
                FailType.MatchFail => " AND FileToInfo.truename == ''",
                FailType.SpiderFail => " AND FileToInfo.truename != ''",
                _ => string.Empty
            };

            var commandText =
                $"SELECT * FROM FilesInfo,FileToInfo WHERE FileToInfo.issuccess == 0 AND FilesInfo.pc == FileToInfo.file_pickcode{showTypeStr}{queryStr}{orderStr} LIMIT {limit} offset {offset} ";

            return Context.Instance.FilesInfos.FromSqlRaw(commandText).ToArray();
        }

        public static int GetCountOfFailFileInfoWithFilesInfo(int offset = 0, int limit = -1, string n = null, string orderBy = null, bool isDesc = false, FailType showType = FailType.All, SqliteConnection connection = null)
        {
            var queryStr = string.IsNullOrEmpty(n) ? string.Empty : $" And FilesInfo.n LIKE '%{n.Replace("'", "%")}%'";

            string showTypeStr;
            switch (showType)
            {
                case FailType.MatchFail:
                    showTypeStr = " AND FileToInfo.truename == ''";
                    break;
                case FailType.SpiderFail:
                    showTypeStr = " AND FileToInfo.truename != ''";
                    break;
                default:
                    showTypeStr = string.Empty;
                    break;
            }

            var commandText =
                $"SELECT pc FROM FilesInfo,FileToInfo WHERE FileToInfo.issuccess == 0 AND FilesInfo.pc == FileToInfo.file_pickcode{showTypeStr}{queryStr} LIMIT {limit} offset {offset} ";


            return Context.Instance.FilesInfos.FromSqlRaw(commandText).Count();
        }

        /// <summary>
        /// 检查演员列表数量
        /// </summary>
        /// <param name="filterList"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static int GetCountOfActorInfo(IEnumerable<string> filterList = null, SqliteConnection connection = null)
        {
            var filterStr = string.Empty;
            if (filterList != null)
            {
                filterStr = $" WHERE {string.Join(",", filterList)}";
            }

            var commandText = $"SELECT id FROM ActorInfo{filterStr}";

            return Context.Instance.ActorInfos.FromSqlRaw(commandText).Count();

        }
        

        public static int GetCountOfFailInfos(FailInfoShowType showType, SqliteConnection connection = null)
        {
            var commandStringBuilder = new StringBuilder("SELECT pc FROM FailList_islike_looklater ");

            commandStringBuilder.Append(showType == FailInfoShowType.like
                ? "WHERE is_like = 1"
                : "WHERE look_later != 0");

            return Context.Instance.FailListIsLikeLookLater.FromSqlRaw(commandStringBuilder.ToString()).Count();

        }

        public static int GetCountOfFailFilesInfoFiles(string n = "", FailType showType = FailType.All, SqliteConnection connection = null)
        {
            var commandStringBuilder = new StringBuilder("SELECT FilesInfo.* FROM FilesInfo,FileToInfo WHERE FileToInfo.issuccess == 0 AND FilesInfo.pc == FileToInfo.file_pickcode");

            switch (showType)
            {
                case FailType.MatchFail:
                    commandStringBuilder.Append(" AND FileToInfo.truename == ''");
                    break;
                case FailType.SpiderFail:
                    commandStringBuilder.Append(" AND FileToInfo.truename != ''");
                    break;
            }

            if (!string.IsNullOrEmpty(n))
            {
                n = n.Replace('\'', '%');
                commandStringBuilder.Append(" And FilesInfo.n LIKE '%").Append(n).Append("%'");
            }

            return Context.Instance.FileToInfos.FromSqlRaw(commandStringBuilder.ToString()).Count();
        }

        /// <summary>
        /// 检查VideoInfo表的数量
        /// </summary>
        /// <param name="filterConditionList"></param>
        /// <param name="filterKeywords"></param>
        /// <param name="rangesDicts"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static int GetCountOfVideoInfo(List<string> filterConditionList = null, string filterKeywords = null, Dictionary<string, string> rangesDicts = null, SqliteConnection connection = null)
        {
            //筛选
            var filterStr = GetVideoInfoFilterStr(filterConditionList, filterKeywords, rangesDicts);

            string commandText;

            //多表查询
            if (rangesDicts != null && rangesDicts.ContainsKey("Type"))
            {
                commandText = $"SELECT * FROM VideoInfo LEFT JOIN Is_Wm ON VideoInfo.truename = Is_Wm.truename LEFT JOIN ProducerInfo ON VideoInfo.producer = ProducerInfo.Name{filterStr}";
            }
            //通过名字对演员进行精确查询
            else if (rangesDicts == null && filterConditionList?.Count == 1 &&
                     filterConditionList.FirstOrDefault() == "actor")
            {
                commandText = $"SELECT VideoInfo.* FROM VideoInfo LEFT JOIN Actor_Video ON VideoInfo.truename = Actor_Video.video_name LEFT JOIN Actor_Names ON Actor_Video.actor_id = Actor_Names.id WHERE Actor_Names.name == '{filterKeywords}'";
            }
            //普通查询
            else
            {
                commandText = $"SELECT * from VideoInfo{filterStr}";
            }

            return Context.Instance.VideoInfos.FromSqlRaw(commandText).Count();
        }

        private static string GetVideoInfoFilterStr(List<string> filterConditionList = null, string filterKeywords = null, Dictionary<string, string> rangesDicts = null)
        {
            var filterStr = string.Empty;
            if (filterConditionList == null && rangesDicts == null) return filterStr;

            var filterStrTmp = string.Empty;
            if (filterConditionList != null)
            {
                List<string> filterList = [];
                foreach (var item in filterConditionList)
                {
                    switch (item)
                    {
                        case "look_later":
                            filterList.Add($"VideoInfo.{item} != 0");
                            break;
                        case "fail":
                            //不操作
                            break;
                        default:
                            filterList.Add(filterKeywords == ""
                                ? $"(VideoInfo.{item} = '{filterKeywords}')"
                                : $"(VideoInfo.{item} LIKE '%{filterKeywords}%')");
                            break;
                    }
                }

                filterStrTmp = string.Join(" OR ", filterList);
            }

            var filterStrTmp2 = string.Empty;
            if (rangesDicts != null)
            {
                List<string> ranges = [];
                foreach (var range in rangesDicts)
                {
                    switch (range.Key)
                    {
                        case "Year":
                            ranges.Add($"(VideoInfo.releasetime LIKE '{range.Value}-%' OR (releasetime LIKE '{range.Value}/%'))");
                            break;
                        case "Score":
                            ranges.Add($"(VideoInfo.score == {range.Value})");
                            break;
                        case "Type":
                            switch (range.Value)
                            {
                                case "步兵":
                                    ranges.Add("(is_wm.is_wm == 1 OR (is_wm.is_wm IS NULL AND ProducerInfo.is_wm == 1))");
                                    break;
                                case "骑兵":
                                    ranges.Add("(is_wm.is_wm == 0 OR (is_wm.is_wm IS NULL AND ProducerInfo.is_wm IS NOT 1 ))");
                                    break;
                            }

                            break;
                    }
                }

                filterStrTmp2 = string.Join(" AND ", ranges);
            }

            if (!string.IsNullOrEmpty(filterStrTmp) && !string.IsNullOrEmpty(filterStrTmp2))
                filterStr = $" WHERE ({filterStrTmp}) AND ({filterStrTmp2})";
            else if (!string.IsNullOrEmpty(filterStrTmp2))
                filterStr = $" WHERE {filterStrTmp2}";
            else if (!string.IsNullOrEmpty(filterStrTmp))
                filterStr = $" WHERE {filterStrTmp}";

            return filterStr;
        }

        /// <summary>
        /// 加载已存在的videoInfo数据
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="orderBy"></param>
        /// <param name="isDesc"></param>
        /// <param name="filterConditionList"></param>
        /// <param name="filterKeywords"></param>
        /// <param name="rangesDicts"></param>
        /// <param name="isFuzzyQueryActor"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static VideoInfo[] GetVideoInfo(int limit = 1, int offset = 0, string orderBy = null, bool isDesc = false, List<string> filterConditionList = null, string filterKeywords = null, Dictionary<string, string> rangesDicts = null, bool isFuzzyQueryActor = true, SqliteConnection connection = null)
        {
            string commandText;

            var filterStr = GetVideoInfoFilterStr(filterConditionList, filterKeywords, rangesDicts);

            //多表查询
            if (rangesDicts != null && rangesDicts.ContainsKey("Type"))
            {
                commandText =
                    "SELECT VideoInfo.* FROM VideoInfo LEFT JOIN Is_Wm ON VideoInfo.truename = Is_Wm.truename LEFT JOIN ProducerInfo ON VideoInfo.producer = ProducerInfo.Name";

            }
            //通过名字对演员进行精确查询
            else if (rangesDicts == null && filterConditionList?.Count == 1 &&
                      filterConditionList.FirstOrDefault() == "actor" && !isFuzzyQueryActor)
            {
                commandText = $"SELECT VideoInfo.* FROM VideoInfo LEFT JOIN Actor_Video ON VideoInfo.truename = Actor_Video.video_name LEFT JOIN Actor_Names ON Actor_Video.actor_id = Actor_Names.id WHERE Actor_Names.name == '{filterKeywords}'";
                filterStr = string.Empty;
            }
            //普通查询
            else
            {
                commandText = "SELECT * from VideoInfo";
            }

            if (!string.IsNullOrEmpty(filterStr))
            {
                commandText += filterStr;
            }
            if (orderBy != null)
            {
                commandText += GetOrderStr(orderBy, isDesc);
            }

            return Context.Instance.VideoInfos.FromSqlRaw(commandText).Skip(offset).Take(limit).ToArray();
        }

        public static ActorInfo[] GetActorInfo(int limit = 1, int offset = 0, Dictionary<string, bool> orderByList = null, List<string> filterList = null, SqliteConnection connection = null)
        {
            var orderStr = string.Empty;
            if (orderByList != null)
            {
                List<string> orderStrList = [];
                foreach (var item in orderByList)
                {
                    var dscStr = item.Value ? " DESC" : string.Empty;

                    orderStrList.Add($"{item.Key}{dscStr}");
                }

                orderStr = $" ORDER BY {string.Join(",", orderStrList)}";
            }

            var filterStr = string.Empty;
            if (filterList != null)
            {
                filterStr = $" WHERE {string.Join(",", filterList)}";
            }

            var commandText =
                $"SELECT ActorInfo.*,bwh.bust,bwh.waist,bwh.hips,COUNT(id) as video_count FROM ActorInfo LEFT JOIN Actor_Video ON Actor_Video.actor_id = ActorInfo.id LEFT JOIN bwh ON ActorInfo.bwh = bwh.bwh{filterStr} GROUP BY id{orderStr} LIMIT {limit} offset {offset}";

            return Context.Instance.ActorInfos.FromSqlRaw(commandText).ToArray();
        }

        /// <summary>
        /// 获取演员出演的视频信息（By label）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="label"></param>
        /// <param name="limit"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static VideoInfo[] GetVideoInfoBySomeType(string type, string label, int limit, SqliteConnection connection = null)
        {
            var commandText = string.IsNullOrEmpty(label) ? $"SELECT * from VideoInfo WHERE {type} == '' LIMIT {limit}"
                : $"SELECT * from VideoInfo WHERE {type} LIKE '%{label}%' LIMIT {limit}";

            
            return Context.Instance.VideoInfos.FromSqlRaw(commandText).ToArray();
        }

        /// <summary>
        /// 获取FileInfo（By TrueName）
        /// </summary>
        /// <param name="trueName"></param>
        /// <returns></returns>
        public static List<FilesInfo> GetSingleFileInfoByTrueName(string trueName)
        {
            var tuple = FileMatch.SplitLeftAndRightFromCid(trueName);

            var leftName = tuple.Item1.Replace("FC2", "FC");
            var rightNumber = tuple.Item2;

            if (rightNumber.StartsWith("0"))
            {
                var matchResult = Regex.Match(rightNumber, @"0*(\d+)");

                if (matchResult.Success)
                    rightNumber = matchResult.Groups[1].Value;
            }
            //vdi = 0，即视频转码未成功，无法在线观看

            var commText = $"SELECT * from FilesInfo WHERE uid != 0 AND iv = 1 AND n LIKE '%{leftName}%{rightNumber}%'";
            var tmpList = Context.Instance.FilesInfos.FromSqlRaw(commText).ToArray();

            var data = new List<FilesInfo>();

            //进一步筛选，通过右侧数字
            // '%xxx%57%' 会选出 057、157、257之类的
            foreach (var datum in tmpList)
            {
                if (leftName == "FC")
                {
                    var matchResult = Regex.Match(datum.Name, @"(FC2?)[-_PV]*[-_]?0*(\d+)", RegexOptions.IgnoreCase);
                    if (matchResult.Success && matchResult.Groups[2].Value == rightNumber)
                        data.Add(datum);
                }
                else
                {
                    var matchResult = Regex.Match(datum.Name, @$"({leftName})[-_]?0*(\d+)", RegexOptions.IgnoreCase);
                    if (matchResult.Success && matchResult.Groups[2].Value == rightNumber)
                        data.Add(datum);
                }
            }

            //进一步筛选
            data = data.Where(x => Regex.Match(x.Name, @$"[^a-z]{leftName}[^a-z]|^{leftName}[^a-z]", RegexOptions.IgnoreCase).Success).ToList();

            return data;
        }

        private static string GetOrderStr(string orderBy, bool isDesc)
        {
            if (string.IsNullOrEmpty(orderBy)) return string.Empty;

            string orderStr;
            if (orderBy == "random")
            {
                orderStr = " ORDER BY RANDOM() ";
            }
            else
            {
                orderStr = isDesc ? $" ORDER BY {orderBy} DESC" : $" ORDER BY {orderBy}";
            }

            return orderStr;
        }


        /// <summary>
        /// 查询PickCode文件对应的字幕文件（首先匹配文件名，其次匹配上一级文件夹的名称）
        /// </summary>
        /// <returns></returns>
        public static FilesInfo[] GetSubFile(string filePickCode)
        {
            //查询字幕文件的信息
            var fileInfo = Context.Instance.FilesInfos.FirstOrDefault(i=>i.PickCode == filePickCode);

            //查询无果，几乎不可能发生
            if (fileInfo == null) return [];

            var fileName = fileInfo.Name;
            var folderCid = fileInfo.CurrentId;

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            var nameIsNumber = TryParse(fileNameWithoutExtension, out _);

            string[] srtSuffix = ["srt", "ass", "ssa"];

            //1.首先查询同名字幕文件(纯数字则跳过)
            if (!nameIsNumber)
            {
                var sameNameSubArray = Context.Instance.FilesInfos.Where(i => srtSuffix.Contains(i.Ico) && EF.Functions.Like( i.Name, "%" + fileNameWithoutExtension + "%")).ToArray();

                if (sameNameSubArray.Length != 0) return sameNameSubArray;
                
                //2.没有同名字幕文件，根据番号特点匹配（字母+数字），这里的方法正常的视频不通用
                var matchName = FileMatch.MatchName(fileName);
                if (!string.IsNullOrEmpty(matchName))
                {
                    var (leftName, rightNumber) = FileMatch.SplitLeftAndRightFromCid(matchName);
                    leftName = leftName.Replace("FC2", "FC");

                    //通过trueName查询字幕文件
                    var subArray = Context.Instance.FilesInfos.Where(i=> srtSuffix.Contains(i.Ico ) && EF.Functions.Like(i.Name, $"%{leftName}%{rightNumber}%")).ToArray();

                    if (subArray.Length != 0) return subArray;
                }
            }

            // 到这一步，直接返回 文件cid下的字幕
            var subListInFolder = Context.Instance.FilesInfos.Where(i => srtSuffix.Contains(i.Ico) && i.CurrentId == folderCid).ToArray();

            if (!nameIsNumber) return subListInFolder;

            return (from item in subListInFolder
                let match = Regex.Match(item.Name, $"(^|[^0-9]){fileNameWithoutExtension}($|[^0-9])")
                where match.Success
                select item).ToArray();
        }

    }
}