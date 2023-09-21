using Display.Helper;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Display.Extensions;
using static Display.Spider.Manager;
using static System.Int32;

namespace Display.Data
{
    public static class DataAccess
    {
        private const string DbName = "115_uwp.db";
        public static string DbPath => NewDbPath(AppSettings.DataAccessSavePath);

        public static string ConnectionString => $"Filename={DbPath}";

        public static string NewDbPath(string newPath)
        {
            return Path.Combine(newPath, DbName);
        }

        /// <summary>
        /// 数据库表初始化
        /// </summary>
        public static async Task InitializeDatabase(string dataAccessSavePath = null)
        {
            dataAccessSavePath ??= AppSettings.DataAccessSavePath;

            TryCreateDbFile(dataAccessSavePath);

            var newDbPath = Path.Combine(dataAccessSavePath, DbName);

            //获取文件夹，没有则创建
            var folder = await StorageFolder.GetFolderFromPathAsync(AppSettings.DataAccessSavePath);
            //获取数据库文件，没有则创建
            await folder.CreateFileAsync(DbName, CreationCollisionOption.OpenIfExists);

            await using var db = new SqliteConnection($"Filename={newDbPath}");

            db.Open();

            //文件信息
            var tableCommand = "CREATE TABLE IF NOT " +
                                  "EXISTS FilesInfo ( " +
                                  "fid text," +
                                  "uid integer," +
                                  "aid integer," +
                                  "cid TEXT," +
                                  "n TEXT," +
                                  "s integer," +
                                  "sta integer," +
                                  "pt TEXT," +
                                  "pid TEXT," +
                                  "pc TEXT," +
                                  "p integer," +
                                  "m integer," +
                                  "t TEXT," +
                                  "te integer," +
                                  "tp integer," +
                                  "d integer," +
                                  "c integer," +
                                  "sh integer," +
                                  "e TEXT," +
                                  "ico TEXT," +
                                  "sha TEXT," +
                                  "fdes TEXT," +
                                  "q integer," +
                                  "hdf integer," +
                                  "fvs integer," +
                                  "u TEXT," +
                                  "iv integer," +
                                  "current_time integer," +
                                  "played_end integer," +
                                  "last_time TEXT," +
                                  "vdi integer," +
                                  "play_long real," +
                                  "PRIMARY KEY('pc')) ";

            var createTable = new SqliteCommand(tableCommand, db);
            createTable.ExecuteNonQuery();

            //番号详情
            createTable.CommandText = "CREATE TABLE IF NOT " +
                                      "EXISTS VideoInfo ( " +
                                      "truename TEXT NOT NULL," +
                                      "title TEXT," +
                                      "releasetime TEXT," +
                                      "lengthtime TEXT," +
                                      "director TEXT," +
                                      "producer TEXT," +
                                      "publisher TEXT," +
                                      "series TEXT," +
                                      "category TEXT," +
                                      "actor TEXT," +
                                      "imageurl TEXT," +
                                      "sampleImageList TEXT," +
                                      "imagepath TEXT," +
                                      "busurl TEXT," +
                                      "look_later integer," +
                                      "score integer," +
                                      "is_like integer," +
                                      "addtime integer," +
                                      "PRIMARY KEY('truename')) ";

            createTable.ExecuteNonQuery();

            //是否步兵
            createTable.CommandText = "CREATE TABLE IF NOT " +
                                      "EXISTS Is_Wm ( " +
                                      "truename text," +
                                      "is_wm integer," +
                                      "PRIMARY KEY('truename')" +
                                      ") ";
            createTable.ExecuteNonQuery();

            //厂商信息，如果没有则初始化并添加常用的
            InitializeProducerInfo(createTable);

            //番号匹配
            createTable.CommandText = "CREATE TABLE IF NOT " +
                                      "EXISTS FileToInfo ( " +
                                      "file_pickcode text," +
                                      "truename text," +
                                      "issuccess integer," +
                                      "PRIMARY KEY('file_pickcode')" +
                                      ") ";
            createTable.ExecuteNonQuery();

            //下载历史
            createTable.CommandText = "CREATE TABLE IF NOT " +
                                      "EXISTS DownHistory ( " +
                                      "file_pickcode text," +
                                      "file_name text," +
                                      "true_url text," +
                                      "ua text," +
                                      "add_time integer," +
                                      "PRIMARY KEY('file_pickcode','ua')" +
                                      ") ";
            createTable.ExecuteNonQuery();

            //演员信息
            createTable.CommandText = "CREATE TABLE IF NOT " +
                                      "EXISTS ActorInfo ( " +
                                      "id INTEGER NOT NULL," +
                                      "Name TEXT NOT NULL," +
                                      "is_woman integer," +
                                      "birthday text," +
                                      "bwh text," +
                                      "height integer," +
                                      "works_count integer," +
                                      "work_time text," +
                                      "prifile_path text," +
                                      "blog_url text," +
                                      "is_like integer," +
                                      "addtime integer," +
                                      "info_url text," +
                                      "PRIMARY KEY (id)" +
                                      ") ";

            createTable.ExecuteNonQuery();

            //为ActorInfo添加info_url
            //info_url必须在最后（插入数据的顺序）
            createTable.CommandText = "ALTER TABLE ActorInfo ADD COLUMN info_url Text";

            //可能已存在
            try
            {
                createTable.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            //bwh
            createTable.CommandText = "CREATE TABLE IF NOT " +
                                      "EXISTS bwh ( " +
                                      "bwh text," +
                                      "bust integer," +
                                      "waist integer," +
                                      "hips integer," +
                                      "PRIMARY KEY('bwh')" +
                                      ") ";
            createTable.ExecuteNonQuery();

            //FailList_islike_looklater
            createTable.CommandText = "CREATE TABLE IF NOT " +
                                      "EXISTS FailList_islike_looklater ( " +
                                      "pc text," +
                                      "is_like integer," +
                                      "score integer," +
                                      "look_later integer," +
                                      "image_path text," +
                                      "PRIMARY KEY('pc')" +
                                      ") ";
            createTable.ExecuteNonQuery();

            //演员别名
            createTable.CommandText = "CREATE TABLE IF NOT " +
                                      "EXISTS Actor_Names ( " +
                                      "id INTEGER NOT NULL," +
                                      "Name TEXT NOT NULL," +
                                      "PRIMARY KEY('id','Name')" +
                                      ") ";
            createTable.ExecuteNonQuery();

            //演员视频中间表
            createTable.CommandText = "CREATE TABLE IF NOT " +
                                      "EXISTS Actor_Video ( " +
                                      "actor_id INTEGER NOT NULL," +
                                      "video_name TEXT NOT NULL," +
                                      "PRIMARY KEY('actor_id','video_name')" +
                                      ") ";
            createTable.ExecuteNonQuery();

            //搜刮记录
            createTable.CommandText = "CREATE TABLE IF NOT " +
                                      "EXISTS SpiderLog ( " +
                                      "task_id INTEGER," +
                                      "time text," +
                                      "done text," +
                                      "PRIMARY KEY('task_id')" +
                                      ") ";
            createTable.ExecuteNonQuery();

            //删除表（添加了搜刮源，需要重新添加）
            createTable.CommandText = "DROP TABLE IF EXISTS SpiderTask";
            createTable.ExecuteNonQuery();

            //搜刮任务
            createTable.CommandText = "CREATE TABLE IF NOT " +
                                      "EXISTS SpiderTask ( " +
                                      "Name text," +
                                      "bus text," +
                                      "Jav321 text," +
                                      "Avmoo text," +
                                      "Avsox text," +
                                      "libre text," +
                                      "fc text," +
                                      "db text," +
                                      "done integer," +
                                      "tadk_id integer," +
                                      "PRIMARY KEY('Name')" +
                                      ") ";
            createTable.ExecuteNonQuery();
        }

        /// <summary>
        /// 初始化厂商信息
        /// </summary>
        /// <param name="createTable"></param>
        public static void InitializeProducerInfo(SqliteCommand createTable)
        {
            const string command = "SELECT Name FROM sqlite_master WHERE type='table' AND Name='ProducerInfo';";
            createTable.CommandText = command;

            var result = createTable.ExecuteScalar();
            //存在
            if (result != null) return;

            //不存在
            //创建
            createTable.CommandText = "CREATE TABLE IF NOT " +
                "EXISTS ProducerInfo ( " +
                    "Name text," +
                    "is_wm is_wm," +
                    "PRIMARY KEY('Name')" +
                    ") ";

            createTable.ExecuteNonQuery();

            var wmProducer = Const.Info.WmProducer;

            //插入常见的
            foreach (var name in wmProducer)
            {
                createTable.CommandText = "INSERT OR IGNORE INTO ProducerInfo VALUES (@Name,1)";
                createTable.Parameters.Clear();
                createTable.Parameters.AddWithValue("@Name", name);
                createTable.ExecuteNonQuery();
            }

        }
        
        /// <summary>
        /// 创建数据文件
        /// </summary>
        /// <param name="targetPath"></param>
        public static async void TryCreateDbFile(string targetPath)
        {
            //获取文件夹
            var folder = await StorageFolder.GetFolderFromPathAsync(targetPath);
            //获取数据库文件，没有则创建
            await folder.CreateFileAsync(DbName, CreationCollisionOption.OpenIfExists);
        }

        #region 私有函数

        public enum TableName
        {
            VideoInfo, FilesInfo, DownHistory, SpiderLog, SpiderTask
        }

        #endregion

        public class Delete
        {
            public static void DeleteVideoInfoByName(string name, SqliteConnection connection)
            {
                var commandText = $"DELETE FROM Actor_Video WHERE video_name =='{name}'";

                DataAccessHelper.ExecuteNonQuery(commandText, connection);
            }

            /// <summary>
            /// 清空指定表
            /// </summary>
            /// <param name="tableName"></param>
            public static void DeleteTable(TableName tableName)
            {
                var command = $"delete from {tableName}";
                DataAccessHelper.ExecuteNonQuery(command, null);
            }

            /// <summary>
            /// 删除VideoInfo表中的一条记录
            /// </summary>
            public static void DeleteDataInVideoInfoTable(string value, string key = "truename")
            {
                var command = $"DELETE FROM VideoInfo WHERE {key} == '{value}'";
                DataAccessHelper.ExecuteNonQuery(command, null);
            }

            /// <summary>
            /// 删除FilesInfo表里文件夹下的所有文件和文件夹（最多两级）
            /// </summary>
            public static void DeleteDirectoryAndFiles_InFilesInfoTable(long cid, SqliteConnection connection)
            {
                var command = $"DELETE FROM FilesInfo WHERE cid == '{cid}' or pid = '{cid}'";

                DataAccessHelper.ExecuteNonQuery(command, connection);
            }

            /// <summary>
            /// 删除表FilesInfo中的指定数据
            /// </summary>
            public static void DeleteSingleFilesInfo(string pc, SqliteConnection connection)
            {
                var command = $"DELETE FROM FilesInfo WHERE pc == '{pc}' COLLATE NOCASE";

                DataAccessHelper.ExecuteNonQuery(command, connection);
            }

            /// <summary>
            /// 删除中间表FileToInfo中的指定数据
            /// </summary>
            public static void DeleteSingleFileToInfo(string pc, SqliteConnection connection)
            {
                var command = $"DELETE FROM FileToInfo WHERE file_pickcode == '{pc}' COLLATE NOCASE";

                DataAccessHelper.ExecuteNonQuery(command, connection);
            }

            /// <summary>
            /// 删除特定pc的表FilesInfo和中间表FileToInfo
            /// </summary>
            /// <param name="pc"></param>
            public static void DeleteDataInFilesInfoAndFileToInfo(string pc)
            {
                using var connection = new SqliteConnection(ConnectionString);

                connection.Open();

                // 先删除 FilesInfo
                Delete.DeleteSingleFilesInfo(pc, connection);

                // 后删除中间表 FileToInfo
                Delete.DeleteSingleFileToInfo(pc, connection);

                connection.Close();
            }

            /// <summary>
            /// 删除FilesInfo表里文件夹下的所有文件和文件夹（遍历所有）
            /// </summary>
            /// <param name="cid"></param>
            /// <param name="connection"></param>
            public static void DeleteAllDirectoryAndFiles_InFilesInfoTable(long cid, SqliteConnection connection = null)
            {
                var isNeedCloseConnection = connection == null;
                if (connection == null)
                {
                    connection = new SqliteConnection(DataAccess.ConnectionString);
                    connection.Open();
                }

                var files = Get.GetAllFilesTraverse(cid);
                if (files == null) return;

                List<long> cidList = new();
                foreach (var file in files)
                {
                    if (!cidList.Contains(file.Cid))
                    {
                        cidList.Add(file.Cid);
                    }
                }

                foreach (var cidFolder in cidList)
                {
                    DeleteDirectoryAndFiles_InFilesInfoTable(cidFolder, connection);
                }


                if (!isNeedCloseConnection) return;

                connection.Close();
                connection.Dispose();
            }
        }

        public class Update
        {
            /// <summary>
            /// 更新版本14之前的数据
            /// </summary>
            /// <returns></returns>
            public static async Task UpdateDatabaseFrom14()
            {
                //更新演员数据
                Dictionary<string, List<string>> actorsInfoDict = new();

                //加载全部数据
                var videoInfoList = await Get.GetVideoInfo(-1);

                if (videoInfoList == null || videoInfoList.Length == 0) return;

                foreach (var videoInfo in videoInfoList)
                {   
                    var actorStr = videoInfo.Actor;

                    var actorList = actorStr.Split(",");
                    foreach (var actor in actorList)
                    {
                        //当前名称不存在
                        if (!actorsInfoDict.ContainsKey(actor))
                        {
                            actorsInfoDict.Add(actor, new List<string>());
                        }
                        actorsInfoDict[actor].Add(videoInfo.trueName);
                    }
                }

                await using var connection = new SqliteConnection(ConnectionString);
                foreach (var item in actorsInfoDict)
                {
                    Add.AddActorInfo(item.Key, item.Value, connection);
                }
                connection.Close();
            }

            /// <summary>
            /// 更新新的VideoInfo数据
            /// </summary>
            /// <param name="videoInfo"></param>
            /// <param name="connection"></param>
            public static void UpdateVideoInfo(VideoInfo videoInfo, SqliteConnection connection)
            {
                Dictionary<string, Tuple<string, string>> dict = new();
                foreach (var item in videoInfo.GetType().GetProperties())
                {
                    if (!item.TryGetJsonName(out var fieldName)) continue;

                    //var fieldName = item.Name;

                    var value = string.Empty + item.GetValue(videoInfo);

                    switch (fieldName)
                    {
                        //忽略是否步兵
                        //忽略自定义数据
                        case "look_later" or "score" or "is_like" or "is_wm":
                            continue;
                        case "actor":
                            value = Spider.JavDB.TrimGenderFromActorName(value);
                            break;
                    }

                    dict.Add($"@{fieldName}", new Tuple<string, string>($"{fieldName} = @{fieldName}", $"{value}"));
                }

                var commandText =
                    $"UPDATE VideoInfo SET {string.Join(",", dict.Values.Select(item => item.Item1))} WHERE truename == @truename";

                var parameters = dict.Select(item => new SqliteParameter(item.Key, item.Value.Item2)).ToArray();

                DataAccessHelper.ExecuteNonQueryWithParameters(commandText, parameters, connection);
            }

            /// <summary>
            /// 更新FailInfo表
            /// </summary>
            /// <param name="pc"></param>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <param name="connection"></param>
            public static void UpdateSingleFailInfo(string pc, string key, string value, SqliteConnection connection = null)
            {
                var command = $"UPDATE FailList_islike_looklater set {key} = '{value}' WHERE pc = '{pc}'";

                DataAccessHelper.ExecuteNonQuery(command, connection);
            }

            /// <summary>
            /// 更新FileToInfo表
            /// </summary>
            /// <param name="trueName"></param>
            /// <param name="isSuccess"></param>
            /// <param name="connection"></param>
            public static void UpdateFileToInfo(string trueName, bool isSuccess, SqliteConnection connection = null)
            {
                var command = $"UPDATE FileToInfo set issuccess = {isSuccess} WHERE truename = '{trueName}'";

                DataAccessHelper.ExecuteNonQuery(command, connection);
            }

            /// <summary>
            /// 更新ActorInfo表
            /// </summary>
            /// <param name="actorId"></param>
            /// <param name="actorInfo"></param>
            /// <param name="connection"></param>
            public static void UpdateActorInfo(long actorId, ActorInfo actorInfo, SqliteConnection connection = null)
            {
                var commandText = $"UPDATE ActorInfo set birthday = @birthday, bwh = @bwh, height = @height, works_count = @works_count, work_time = @work_time, blog_url = @blog_url, info_url = @info_url, addtime = @addtime WHERE id = '{actorId}'";

                var parameters = new List<SqliteParameter>
                {
                    new("@birthday", actorInfo.Birthday),
                    new ("@bwh", actorInfo.Bwh),
                    new ("@height", actorInfo.Height),
                    new ("@works_count", actorInfo.WorksCount),
                    new ("@work_time", actorInfo.WorkTime),
                    new ("@blog_url", actorInfo.BlogUrl),
                    new ("@info_url", actorInfo.InfoUrl),
                    new ("@addtime", actorInfo.AddTime)
                };

                DataAccessHelper.ExecuteNonQueryWithParameters(commandText, parameters, connection);
            }

            /// <summary>
            /// 更新演员性别
            /// </summary>
            /// <param name="id"></param>
            /// <param name="isWoman"></param>
            /// <param name="connection"></param>
            public static void UpdateActorInfoIsWoman(long id, int isWoman,SqliteConnection connection)
            {
                var command = $"UPDATE ActorInfo SET is_woman = '{isWoman}' WHERE id = '{id}'";

                DataAccessHelper.ExecuteNonQuery(command, connection);

            }

            /// <summary>
            /// 更新演员头像地址
            /// </summary>
            /// <param name="id"></param>
            /// <param name="profilePath"></param>
            /// <param name="connection"></param>
            public static void UpdateActorInfoProfilePath(long id, string profilePath, SqliteConnection connection = null)
            {
                var command = $"UPDATE ActorInfo SET prifile_path = '{profilePath}' WHERE id = '{id}'";

                DataAccessHelper.ExecuteNonQuery(command, connection);
            }

            /// <summary>
            /// 更新数据信息，VideoInfo的单个字段
            /// </summary>
            /// <param name="trueName"></param>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <param name="connection"></param>
            public static void UpdateSingleDataFromVideoInfo(string trueName, string key, string value, SqliteConnection connection = null)
            {
                var command = $"UPDATE VideoInfo SET {key} = '{value}' WHERE truename = '{trueName}'";

                DataAccessHelper.ExecuteNonQuery(command,connection);
            }

            /// <summary>
            /// 更新演员信息，ActorInfo的单个字段
            /// </summary>
            /// <param name="id"></param>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <param name="connection"></param>
            public static void UpdateSingleDataFromActorInfo(string id, string key, string value, SqliteConnection connection = null)
            {
                var command = $"UPDATE ActorInfo SET {key} = '{value}' WHERE id = '{id}'";

                DataAccessHelper.ExecuteNonQuery(command,connection);
            }

            /// <summary>
            /// 更新图片地址
            /// </summary>
            /// <param name="srcPath"></param>
            /// <param name="dstPath"></param>
            /// <param name="connection"></param>
            public static void UpdateAllImagePath(string srcPath, string dstPath, SqliteConnection connection = null)
            {
                var command = $"update VideoInfo set imagepath = REPLACE(imagepath,'{srcPath}','{dstPath}')";

                DataAccessHelper.ExecuteNonQuery(command,connection);
            }

            /// <summary>
            /// 更新演员头像地址
            /// </summary>
            /// <param name="srcPath"></param>
            /// <param name="dstPath"></param>
            /// <param name="connection"></param>
            public static void UpdateActorProfilePath(string srcPath, string dstPath, SqliteConnection connection=null)
            {
                var command = $"update ActorInfo set prifile_path = REPLACE(prifile_path,'{srcPath}','{dstPath}')";

                DataAccessHelper.ExecuteNonQuery(command,connection);
            }

            /// <summary>
            /// 更新SpiderTask表（源搜刮中，源搜刮完成，全部搜刮完成）
            /// </summary>
            /// <param name="name"></param>
            /// <param name="spiderSource"></param>
            /// <param name="spiderSourceStatus"></param>
            /// <param name="isAllDone"></param>
            /// <param name="connection"></param>
            public static void UpdateSpiderTask(string name, SpiderSource spiderSource, SpiderStates spiderSourceStatus, bool isAllDone = false, SqliteConnection connection = null)
            {
                var command = $"update SpiderTask SET {spiderSource.Name} = '{spiderSourceStatus}' , done = {isAllDone} WHERE Name == '{name}'";

                DataAccessHelper.ExecuteNonQuery(command, connection);
            }

            /// <summary>
            /// 更新SpiderLog中的Task_Id为已完成
            /// </summary>
            /// <param name="taskId"></param>
            /// <param name="connection"></param>
            public static void UpdateSpiderLogDone(long taskId, SqliteConnection connection)
            {
                var command = $"UPDATE SpiderLog SET done = 1 WHERE task_id == {taskId}";

                DataAccessHelper.ExecuteNonQuery(command, connection);
            }

            /// <summary>
            /// 更新VideoInfo数据（不包括个性化的内容（喜欢，稍后观看，评分））
            /// </summary>
            /// <param name="videoInfo"></param>
            public static void UpdateDataFromVideoInfo(VideoInfo videoInfo)
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                // 更新表VideoInfo
                Update.UpdateVideoInfo(videoInfo, connection);

                //更新是否步兵
                Add.AddOrReplaceIs_Wm(videoInfo.trueName, videoInfo.Producer, videoInfo.IsWm, connection);

                //更新演员信息
                //先删除Actor_Videos中所有Video_name的数据
                Delete.DeleteVideoInfoByName(videoInfo.trueName, connection);

                Add.AddActorInfoByActorInfo(videoInfo, new List<string> { videoInfo.trueName }, connection);

                connection.Close();
            }


        }

        public class Add
        {
            /// <summary>
            /// 添加Datum信息到FilesInfo
            /// </summary>
            /// <param name="data"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static async Task AddFilesInfoAsync(Datum data,SqliteConnection connection = null)
            {
                var keyList = new List<string>();
                foreach (var item in data.GetType().GetProperties())
                {
                    ////fl为数组，应添加进新表中，一对多。目前暂不考虑，故跳过
                    //if (item.Name == "Fl") continue;

                    if (!item.TryGetJsonName(out var fieldName)) continue;

                    keyList.Add("@"+ fieldName);
                }

                //唯一值（pc）重复 则代替 （replace）
                var commandText = $"INSERT OR REPLACE INTO FilesInfo VALUES ({string.Join(",", keyList)});";

                var parameters = new List<SqliteParameter>();
                foreach (var item in data.GetType().GetProperties())
                {
                    ////fl为数组，应添加进新表中，一对多。目前暂不考虑，故跳过
                    //if (item.Name == "Fl") continue;

                    if(!item.TryGetJsonName(out var fieldName)) continue;
                    
                    parameters.Add(new SqliteParameter("@" + fieldName, $"{item.GetValue(data)}"));
                }

                await DataAccessHelper.ExecuteNonQueryWithParametersAsync(commandText,parameters, connection);
            }

            /// <summary>
            /// 添加FileToInfo表
            /// </summary>
            /// <param name="pickCode"></param>
            /// <param name="trueName"></param>
            /// <param name="isSuccess"></param>
            /// <param name="isReplace"></param>
            /// <param name="connection"></param>
            public static void AddFileToInfo(string pickCode, string trueName, bool isSuccess = false, bool isReplace = false, SqliteConnection connection = null)
            {
                trueName ??= string.Empty;

                //唯一值（pc）重复 则代替 （replace）
                var replaceStr = isReplace ? " OR REPLACE" : " OR IGNORE";
                var commandText = $"INSERT{replaceStr} INTO FileToInfo VALUES (@file_pickcode,@truename,@issuccess);";

                var parameters = new List<SqliteParameter>
                {
                    new("@file_pickcode", pickCode),
                    new("@truename", trueName),
                    new("@issuccess", isSuccess)
                };
                DataAccessHelper.ExecuteNonQueryWithParameters(commandText, parameters, connection);
            }

            private static void AddVideoInfo(VideoInfo data, SqliteConnection connection)
            {
                Dictionary<string, string> dictionary = new();
                foreach (var item in data.GetType().GetProperties())
                {
                    if (!item.TryGetJsonName(out var fieldName)) continue;

                    if (fieldName == "is_wm")
                        continue;

                    var value = string.Empty + item.GetValue(data);

                    //去除演员性别标记
                    if (fieldName == "actor")
                    {
                        value = Spider.JavDB.RemoveGenderFromActorListString(value);
                    }

                    dictionary.Add($"@{fieldName}", $"{value}");
                }

                //添加信息，如果已经存在则跳过
                var commandText = $"INSERT OR IGNORE INTO VideoInfo VALUES ({string.Join(",", dictionary.Keys)});";

                var parameters = new List<SqliteParameter>();

                foreach (var item in dictionary)
                {
                    parameters.Add(new SqliteParameter(item.Key, item.Value));
                }

                DataAccessHelper.ExecuteNonQueryWithParameters(commandText, parameters, connection);
            }

            /// <summary>
            /// 添加下载记录
            /// </summary>
            /// <param name="data"></param>
            /// <param name="connection"></param>
            public static void AddDownHistory(DownInfo data, SqliteConnection connection = null)
            {
                const string commandText = "INSERT OR REPLACE INTO DownHistory VALUES (@file_pickcode,@file_name,@true_url,@ua,@add_time);";

                var parameters = new List<SqliteParameter>
                {
                    new("@file_pickcode", data.PickCode),
                    new("@file_name", data.FileName),
                    new("@true_url", data.TrueUrl),
                    new("@ua", data.Ua),
                    new("@add_time", data.AddTime)
                };
                DataAccessHelper.ExecuteNonQueryWithParameters(commandText, parameters, connection);
            }

            /// <summary>
            /// 插入或替换是否Is_Wm的表数据
            /// </summary>
            /// <param name="trueName"></param>
            /// <param name="producer"></param>
            /// <param name="isWm"></param>
            /// <param name="connection"></param>
            public static void AddOrReplaceIs_Wm(string trueName, string producer, int isWm, SqliteConnection connection)
            {
                const string commandText = "INSERT OR REPLACE INTO Is_Wm VALUES (@truename,@is_wm)";

                var parameters = new List<SqliteParameter>
                {
                    new("@truename", trueName),
                    new("@is_wm", isWm)
                };
                DataAccessHelper.ExecuteNonQueryWithParameters(commandText, parameters, connection);

                //添加厂商信息
                AddOrIgnoreWm_Producer(producer, isWm, connection);
            }

            /// <summary>
            /// 插入或替换表FailList_islike_looklater的表数据
            /// </summary>
            /// <param name="failInfo"></param>
            /// <param name="connection"></param>
            public static void AddOrReplaceFailList_IsLike_LookLater(FailInfo failInfo, SqliteConnection connection = null)
            {
                const string commandText = "INSERT OR REPLACE INTO FailList_islike_looklater VALUES (@pc,@is_like,@score,@look_later,@image_path)";

                var parameters = new List<SqliteParameter>
                {
                    new("@pc", failInfo.PickCode),
                    new("@is_like", failInfo.IsLike),
                    new("@score", failInfo.Score),
                    new("@look_later", failInfo.LookLater),
                    new("@image_path", failInfo.ImagePath)
                };
                DataAccessHelper.ExecuteNonQueryWithParameters(commandText, parameters, connection);
            }

            public static void AddOrIgnoreActor_Video(long actorId, string videoName,SqliteConnection connection)
            {
                const string commandText = "INSERT OR IGNORE INTO Actor_Video VALUES (@actor_id,@video_name)";

                var parameters = new List<SqliteParameter>
                {
                    new("@actor_id", actorId),
                    new("@video_name", videoName)
                };
                DataAccessHelper.ExecuteNonQueryWithParameters(commandText, parameters, connection);
            }

            public static void AddOrIgnoreActor_Names(long id, string name, SqliteConnection connection = null)
            {
                const string commandText = "INSERT OR IGNORE INTO Actor_Names VALUES (@id,@Name)";

                var parameters = new List<SqliteParameter>
                {
                    new("@id", id),
                    new("@Name", name)
                };
                DataAccessHelper.ExecuteNonQueryWithParameters(commandText, parameters, connection);
            }

            public static void AddOrIgnoreBwh(string bwh, int bust, int waist, int hips, SqliteConnection connection = null)
            {
                const string commandText = "INSERT OR IGNORE INTO bwh VALUES (@bwh,@bust,@waist,@hips)";

                var parameters = new List<SqliteParameter>
                {
                    new("@bwh", bwh),
                    new("@bust", bust),
                    new("@waist", waist),
                    new("@hips", hips)
                };
                DataAccessHelper.ExecuteNonQueryWithParameters(commandText, parameters, connection);
            }

            /// <summary>
            /// 添加或忽略步兵的生厂商
            /// </summary>
            /// <param name="name"></param>
            /// <param name="isWm"></param>
            /// <param name="connection"></param>
            public static void AddOrIgnoreWm_Producer(string name, int isWm, SqliteConnection connection)
            {
                if (string.IsNullOrEmpty(name)) return;

                //添加信息，如果已经存在则替换
                const string commandText = "INSERT OR IGNORE INTO ProducerInfo VALUES (@Name,@is_wm)";

                var parameters = new List<SqliteParameter>
                {
                    new("@Name", name),
                    new("@is_wm", isWm)
                };
                DataAccessHelper.ExecuteNonQueryWithParameters(commandText, parameters, connection);
            }

            /// <summary>
            /// 添加搜刮任务
            /// </summary>
            /// <param name="name"></param>
            /// <param name="taskId"></param>
            /// <param name="connection"></param>
            public static void AddSpiderTask(string name, long taskId,SqliteConnection connection = null)
            {
                const string commandText = "INSERT OR REPLACE INTO SpiderTask VALUES (@Name,@bus,@Jav321,@Avmoo,@Avsox,@libre,@fc,@db,@done,@task_id);";

                //fc只有javdb，fc2club能刮
                var isFc = FileMatch.IsFc2(name);

                var parameters = new List<SqliteParameter>
                {
                    new("@Name", name),
                    new ("@bus", !isFc && AppSettings.IsUseJavBus ? "ready" : "done"),
                    new ("@Jav321", !isFc && AppSettings.IsUseJav321 ? "ready" : "done"),
                    new ("@Avmoo", !isFc && AppSettings.IsUseAvMoo ? "ready" : "done"),
                    new ("@Avsox", AppSettings.IsUseAvSox ? "ready" : "done"),
                    new ("@libre", !isFc && AppSettings.IsUseLibreDmm ? "ready" : "done"),
                    new ("@fc", isFc && AppSettings.IsUseFc2Hub ? "ready" : "done"),
                    new ("@db", AppSettings.IsUseJavDb ? "ready" : "done"),
                    new ("@done", false),
                    new ("@task_id", taskId)
                };

                DataAccessHelper.ExecuteNonQueryWithParameters(commandText, parameters, connection);
            }

            /// <summary>
            /// 插入ActorInfo
            /// </summary>
            /// <param name="info"></param>
            /// <param name="connection"></param>
            public static void AddActorInfo(ActorInfo info, SqliteConnection connection)
            {
                //插入新记录
                var commandText = "INSERT INTO ActorInfo VALUES (NULL,@Name,@is_woman,@birthday,@bwh,@height,@works_count,@work_time,@prifile_path,@blog_url,@is_like,@addtime,@info_url);";

                var parameters = new List<SqliteParameter>
                {
                    new("@Name", info.Name),
                    new("@is_woman", info.IsWoman),
                    new("@birthday", info.Birthday),
                    new("@bwh", info.Bwh),
                    new("@height", info.Height),
                    new("@works_count", info.WorksCount),
                    new("@work_time", info.WorkTime),
                    new("@prifile_path", info.ProfilePath),
                    new("@blog_url", info.BlogUrl),
                    new("@is_like", info.IsLike),
                    new("@addtime", info.AddTime),
                    new("@info_url", info.InfoUrl),
                };

                DataAccessHelper.ExecuteNonQueryWithParameters(commandText, parameters, connection);
            }

            /// <summary>
            /// 插入搜刮日志并返回Task_id
            /// </summary>
            /// <param Name="data"></param>
            public static long AddSpiderLog()
            {
                using var db = new SqliteConnection(ConnectionString);
                db.Open();

                var command = new SqliteCommand()
                {
                    Connection = db
                };

                var addTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                //插入新记录
                command.CommandText = $"INSERT INTO SpiderLog VALUES (NULL,@time,0);";
                command.Parameters.AddWithValue("@time", addTime);
                command.ExecuteReader();

                //查询Task_ID
                command = new SqliteCommand($"SELECT task_id FROM SpiderLog WHERE time == '{addTime}'", db);

                var obj = command.ExecuteScalar();
                if (obj == null) return 0;
                var taskId = (long)obj;

                db.Close();

                return taskId;
            }

            /// <summary>
            /// 添加视频信息
            /// </summary>
            /// <param name="data"></param>
            public static async Task AddVideoInfo_ActorInfo_IsWmAsync(VideoInfo data)
            {
                await using var connection = new SqliteConnection(ConnectionString);

                connection.Open();

                AddVideoInfo(data, connection);

                //添加演员信息
                AddActorInfoByActorInfo(data, new List<string> { data.trueName }, connection);

                //添加是否步兵
                AddOrReplaceIs_Wm(data.trueName, data.Producer, data.IsWm, connection);

                connection.Close();
            }

            /// <summary>
            /// 升级ActorInfo
            /// </summary>
            /// <param name="videoInfo"></param>
            /// <param name="videoNameList"></param>
            /// <param name="connection"></param>
            public static void AddActorInfoByActorInfo(VideoInfo videoInfo, List<string> videoNameList, SqliteConnection connection)
            {
                var actorStr = videoInfo.Actor;
                var actorList = actorStr.Split(",");
                foreach (var actorName in actorList)
                {
                    //查询Actor_ID
                    SqliteCommand command = new($"SELECT id FROM ActorInfo WHERE Name == '{Spider.JavDB.TrimGenderFromActorName(actorName)}'", connection);

                    if (command.ExecuteScalar() is long actorId)
                    {
                        //添加信息，如果已经存在则忽略
                        command.CommandText = $"INSERT OR IGNORE INTO Actor_Video VALUES (@actor_id,@video_name)";
                        command.Parameters.AddWithValue("@actor_id", actorId);
                        command.Parameters.AddWithValue("@video_name", videoInfo.trueName);
                        command.ExecuteNonQuery();
                    }
                    // 没有该演员信息的话
                    // 新添加演员信息
                    else
                    {
                        AddActorInfo(actorName, videoNameList, connection);
                    }
                }
            }

            /// <summary>
            /// 插入演员信息并返回actor_id
            /// </summary>
            /// <param name="info"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            private static long AddActorInfoAndReturnId(ActorInfo info, SqliteConnection connection)
            {
                Add.AddActorInfo(info, connection);

                return Get.GetActorInfoIdByName(info.Name, connection);
            }

            /// <summary>
            /// 添加演员信息
            /// </summary>
            /// <param name="actorName"></param>
            /// <param name="videoNameList"></param>
            /// <param name="connection"></param>
            public static void AddActorInfo(string actorName, List<string> videoNameList, SqliteConnection connection)
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
                        isWoman = singleActorName.EndsWith(Spider.JavDB.manSymbol) ? 0 : 1;
                        singleActorName = Spider.JavDB.TrimGenderFromActorName(singleActorName);

                        otherNames = matchResult.Groups[2].Value.Split("、");
                    }
                    else
                    {
                        isWoman = actorName.EndsWith(Spider.JavDB.manSymbol) ? 0 : 1;
                        singleActorName = Spider.JavDB.TrimGenderFromActorName(actorName);
                    }
                }
                else
                {
                    singleActorName = string.Empty;
                }

                //插入演员信息（不存在时才插入）
                var actorId = Get.GetIdInActor_Names(singleActorName, connection);
                //数据库中不存在该名称
                if (actorId == -1)
                {
                    ActorInfo actorInfo = new() { Name = singleActorName, IsWoman = isWoman };

                    //检查演员图片是否存在
                    var imagePath = Path.Combine(AppSettings.ActorInfoSavePath, singleActorName, "face.jpg");
                    if (File.Exists(imagePath))
                    {
                        actorInfo.ProfilePath = imagePath;
                    }

                    actorId = AddActorInfoAndReturnId(actorInfo, connection);
                }
                //为了弥补，之前所有的is_woman都默认为1
                else if (isWoman == 0)
                {
                    Update.UpdateActorInfoIsWoman(actorId, isWoman, connection);
                }

                //添加Actor_Names
                //主名称
                AddOrIgnoreActor_Names(actorId, singleActorName, connection);
                //别名
                if (otherNames != null)
                {
                    foreach (var name in otherNames)
                    {
                        Add.AddOrIgnoreActor_Names(actorId, name, connection);
                    }
                }

                //添加演员和作品的信息
                foreach (var videoName in videoNameList)
                {
                    AddOrIgnoreActor_Video(actorId, videoName, connection);
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
            public static async Task<Datum[]> GetDatumByTrueName(string trueName, SqliteConnection connection)
            {
                var commandText =
                    $"SELECT * FROM FilesInfo,FileToInfo WHERE FilesInfo.pc == FileToInfo.file_pickcode AND FileToInfo.truename == '{trueName}' COLLATE NOCASE";
                
                return await DataAccessHelper.ExecuteReaderGetArrayAsync<Datum>(commandText,connection);
            }

            /// <summary>
            /// 加载已存在的一条videoInfo数据
            /// </summary>
            /// <param name="trueName"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static VideoInfo GetSingleVideoInfoByTrueName(string trueName, SqliteConnection connection = null)
            {
                var commandText = $"SELECT * from VideoInfo WHERE truename = '{trueName}' LIMIT 1";

                return DataAccessHelper.ExecuteReaderGetSingle<VideoInfo>(commandText,connection);
            }

            /// <summary>
            /// 查找truename
            /// </summary>
            /// <param name="name"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static string GetOneTrueNameByName(string name, SqliteConnection connection = null)
            {
                string commandText;
                if (name.Contains("_"))
                {
                    commandText =
                        $"SELECT truename FROM VideoInfo WHERE truename COLLATE NOCASE in ('{name}', '{name.Replace('_', '-')}', '{name.Replace("_", "")}' ) Limit 1";
                }
                else
                {
                    commandText =
                        $"SELECT truename FROM VideoInfo WHERE truename COLLATE NOCASE in ('{name}', '{name.Replace("-", "_")}', '{name.Replace("-", "")}') Limit 1";
                }

                return DataAccessHelper.ExecuteScalar<string>(commandText,connection);
            }

            public static FailInfo GetSingleFailInfoByPickCode(string pc, SqliteConnection connection = null)
            {   
                var commandText =
                    $"SELECT info.*,fail.is_like, fail.score, fail.look_later, fail.image_path  FROM FailList_islike_looklater as fail LEFT JOIN FilesInfo as info on info.pc = fail.pc WHERE fail.pc = '{pc}' LIMIT 1";

                return DataAccessHelper.ExecuteReaderGetSingle<FailInfo>(commandText,connection);
            }

            /// <summary>
            /// 查询失败列表（FailInfo格式）
            /// </summary>
            /// <returns></returns>
            public static async Task<FailInfo[]> GetFailFileInfoWithFailInfo(int offset = 0, int limit = -1, FailInfoShowType showType = FailInfoShowType.like, SqliteConnection connection=null)
            {
                var showTypeStr = showType == FailInfoShowType.like ? " WHERE is_like = 1" : " WHERE look_later != 0";

                var commandText =
                    $"SELECT info.*,fail.is_like, fail.score, fail.look_later, fail.image_path  FROM FailList_islike_looklater as fail LEFT JOIN FilesInfo as info on info.pc = fail.pc{showTypeStr} LIMIT {limit} offset {offset}";

                return await DataAccessHelper.ExecuteReaderGetArrayAsync<FailInfo>(commandText, connection);
            }

            /// <summary>
            /// 查询失败列表（Datum格式）
            /// </summary>
            /// <returns></returns>
            public static async Task<Datum[]> GetFailFileInfoWithDatum(int offset = 0, int limit = -1, string n = null, string orderBy = null, bool isDesc = false, FailType showType = FailType.All, SqliteConnection connection=null)
            {
                var orderStr = GetOrderStr(orderBy, isDesc);

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
                    $"SELECT * FROM FilesInfo,FileToInfo WHERE FileToInfo.issuccess == 0 AND FilesInfo.pc == FileToInfo.file_pickcode{showTypeStr}{queryStr}{orderStr} LIMIT {limit} offset {offset} ";

                return await DataAccessHelper.ExecuteReaderGetArrayAsync<Datum>(commandText,connection);
            }

            public static int GetCountOfFailFileInfoWithDatum(int offset = 0, int limit = -1, string n = null, string orderBy = null, bool isDesc = false, FailType showType = FailType.All, SqliteConnection connection = null)
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
                    $"SELECT COUNT(pc) FROM FilesInfo,FileToInfo WHERE FileToInfo.issuccess == 0 AND FilesInfo.pc == FileToInfo.file_pickcode{showTypeStr}{queryStr} LIMIT {limit} offset {offset} ";

                return DataAccessHelper.ExecuteScalar<int>(commandText,connection);
            }

            /// <summary>
            /// 检查演员列表数量
            /// </summary>
            /// <param name="filterList"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static int GetCountOfActorInfo(IEnumerable<string> filterList = null,SqliteConnection connection =null)
            {
                var filterStr = string.Empty;
                if (filterList != null)
                {
                    filterStr = $" WHERE {string.Join(",", filterList)}";
                }

                var commandText = $"SELECT COUNT(id) FROM ActorInfo{filterStr}";

                return DataAccessHelper.ExecuteScalar<int>(commandText, connection);
            }

            public static int GetCountOfFailInfos(FailInfoShowType showType, SqliteConnection connection = null)
            {
                var commandStringBuilder = new StringBuilder("SELECT COUNT(pc) FROM FailList_islike_looklater ");

                commandStringBuilder.Append(showType == FailInfoShowType.like
                    ? "WHERE is_like = 1"
                    : "WHERE look_later != 0");

                return DataAccessHelper.ExecuteScalar<int>(commandStringBuilder.ToString(), connection);
            }

            public static int GetCountOfFailDatumFiles(string n = "", FailType showType = FailType.All, SqliteConnection connection = null)
            {
                var commandStringBuilder = new StringBuilder("SELECT COUNT(n) FROM FilesInfo,FileToInfo WHERE FileToInfo.issuccess == 0 AND FilesInfo.pc == FileToInfo.file_pickcode");

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

                return DataAccessHelper.ExecuteScalar<int>(commandStringBuilder.ToString(), connection);
            }

            /// <summary>
            /// 检查VideoInfo表的数量
            /// </summary>
            /// <param name="filterConditionList"></param>
            /// <param name="filterKeywords"></param>
            /// <param name="rangesDicts"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static int GetCountOfVideoInfo(List<string> filterConditionList = null, string filterKeywords = null, Dictionary<string, string> rangesDicts = null,SqliteConnection connection =null)
            {
                //筛选
                var filterStr = GetVideoInfoFilterStr(filterConditionList, filterKeywords, rangesDicts);

                string commandText;

                //多表查询
                if (rangesDicts != null && rangesDicts.ContainsKey("Type"))
                {
                    commandText = $"SELECT COUNT(VideoInfo.truename) FROM VideoInfo LEFT JOIN Is_Wm ON VideoInfo.truename = Is_Wm.truename LEFT JOIN ProducerInfo ON VideoInfo.producer = ProducerInfo.Name{filterStr}";
                }
                //普通查询
                else
                {
                    commandText = $"SELECT COUNT(VideoInfo.truename) from VideoInfo{filterStr}";
                }

                return DataAccessHelper.ExecuteScalar<int>(commandText, connection);
            }

            /// <summary>
            /// 获取一个图片地址
            /// </summary>
            /// <returns></returns>
            public static string GetOneImagePath(SqliteConnection connection = null)
            {
                const string commandText = "SELECT imagepath from VideoInfo WHERE imagepath != '' limit 1";

                return DataAccessHelper.ExecuteScalar<string>(commandText, connection);
            }

            /// <summary>
            /// 获取一张演员头像的地址
            /// </summary>
            /// <returns></returns>
            public static string GetOneActorProfilePath(SqliteConnection connection=null)
            {
                var commandText = "SELECT prifile_path from ActorInfo WHERE prifile_path != '' limit 1";
                return DataAccessHelper.ExecuteScalar<string>(commandText, connection);

            }

            /// <summary>
            /// 查询表Actor_Names中对应Names的id
            /// </summary>
            /// <param name="name"></param>
            /// <param name="connection"></param>
            /// <returns>对应Name的id，不存在则返回 -1</returns>
            public static long GetIdInActor_Names(string name, SqliteConnection connection = null)
            {
                var commandText = $"SELECT id FROM Actor_Names WHERE Name == '{name}'";

                return DataAccessHelper.ExecuteScalar<long?>(commandText, connection) ?? -1;
            }

            /// <summary>   
            /// 查询表ActorInfo中对应Names的id
            /// </summary>  
            /// <param name="name"></param>
            /// <param name="connection"></param>
            /// <returns>对应Name的id，不存在则返回 -1</returns>
            public static long GetActorInfoIdByName(string name, SqliteConnection connection)
            {
                var commandText = $"SELECT id FROM ActorInfo WHERE Name == '{name}'";

                return DataAccessHelper.ExecuteScalar<long?>(commandText, connection) ?? -1;
            }

            /// <summary>
            /// 获取可用搜刮任务的一个name
            /// </summary>
            /// <param name="spiderSource"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static string GetOneSpiderTask(SpiderSource spiderSource, SqliteConnection connection)
            {
                var commandText = $"SELECT Name from SpiderTask WHERE Done == 0 AND " +
                                  $"(libre != 'doing' AND bus != 'doing' AND Jav321 != 'doing' AND Avmoo != 'doing' AND Avsox != 'doing' AND fc != 'doing' AND db != 'doing' AND fc != 'doing') AND " +
                                  $"{spiderSource.Name} == 'ready' LIMIT 1";

                return DataAccessHelper.ExecuteScalar<string>(commandText,connection);
            }

            /// <summary>
            /// 获取该番号搜刮未完成且该搜刮源的状态为ready的数量
            /// </summary>
            /// <param name="spiderSource"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static int GetWaitSpiderTaskCount(SpiderSource spiderSource, SqliteConnection connection = null)
            {
                //TODO 分类讨论，考虑FC2的搜刮源
                var commandText = $"SELECT count(Name) from SpiderTask WHERE Done == 0 AND {spiderSource.Name} == 'ready'";

                return DataAccessHelper.ExecuteScalar<int>(commandText, connection);

            }

            private static string GetVideoInfoFilterStr(List<string> filterConditionList = null, string filterKeywords = null, Dictionary<string, string> rangesDicts = null)
            {
                var filterStr = string.Empty;
                if (filterConditionList == null && rangesDicts == null) return filterStr;

                var filterStrTmp = string.Empty;
                if (filterConditionList != null)
                {
                    List<string> filterList = new();
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
                                    ? $"(VideoInfo.{item} == '{filterKeywords}')"
                                    : $"(VideoInfo.{item} LIKE '%{filterKeywords}%')");
                                break;
                        }
                    }

                    filterStrTmp = string.Join(" OR ", filterList);
                }

                var filterStrTmp2 = string.Empty;
                if (rangesDicts != null)
                {
                    List<string> ranges = new();
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
            public static async Task<VideoInfo[]> GetVideoInfo(int limit = 1, int offset = 0, string orderBy = null, bool isDesc = false, List<string> filterConditionList = null, string filterKeywords = null, Dictionary<string, string> rangesDicts = null, bool isFuzzyQueryActor = true, SqliteConnection connection= null)
            {
                var orderStr = GetOrderStr(orderBy, isDesc);
                //筛选
                var filterStr = GetVideoInfoFilterStr(filterConditionList, filterKeywords, rangesDicts);
                string commandText;

                //多表查询
                if (rangesDicts != null && rangesDicts.ContainsKey("Type"))
                {
                    commandText = $"SELECT VideoInfo.* FROM VideoInfo LEFT JOIN Is_Wm ON VideoInfo.truename = Is_Wm.truename LEFT JOIN ProducerInfo ON VideoInfo.producer = ProducerInfo.Name{filterStr}{orderStr} LIMIT {limit} offset {offset}";
                }
                else if (rangesDicts == null && filterConditionList?.Count == 1 && filterConditionList.FirstOrDefault() == "actor" && !isFuzzyQueryActor)
                {
                    commandText =
                        $"SELECT VideoInfo.* FROM VideoInfo LEFT JOIN Actor_Video ON VideoInfo.truename = Actor_Video.video_name LEFT JOIN Actor_Names ON Actor_Video.actor_id = Actor_Names.id WHERE Actor_Names.name == '{filterKeywords}'{orderStr} LIMIT {limit} offset {offset}";
                }
                //普通查询
                else
                {
                    commandText = $"SELECT * from VideoInfo{filterStr}{orderStr} LIMIT {limit} offset {offset}";
                }

                return await DataAccessHelper.ExecuteReaderGetArrayAsync<VideoInfo>(commandText, connection);
            }

            public static async Task<ActorInfo[]> GetActorInfo(int limit = 1, int offset = 0, Dictionary<string, bool> orderByList = null, List<string> filterList = null,SqliteConnection connection=null)
            {
                var orderStr = string.Empty;
                if (orderByList != null)
                {
                    List<string> orderStrList = new();
                    foreach (var item in orderByList)
                    {
                        string dscStr = item.Value ? " DESC" : string.Empty;

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




                return await DataAccessHelper.ExecuteReaderGetArrayAsync<ActorInfo>(commandText,connection);
            }

            public static async Task<ActorInfo[]> GetActorInfoByVideoName(string videoName, SqliteConnection connection = null)
            {
                var commandText =
                    $"SELECT actors.*,bwh.bust,bwh.waist,bwh.hips, COUNT(id) as video_count FROM ( SELECT ActorInfo.* FROM ActorInfo, VideoInfo, Actor_Video WHERE VideoInfo.truename == '{videoName}' AND VideoInfo.truename == Actor_Video.video_name AND Actor_Video.actor_id == ActorInfo.id ) AS actors LEFT JOIN Actor_Video ON Actor_Video.actor_id = actors.id LEFT JOIN bwh ON actors.bwh = bwh.bwh GROUP BY id";

                return await DataAccessHelper.ExecuteReaderGetArrayAsync<ActorInfo>(commandText,connection);
            }

            /// <summary>
            /// 通过PickCode以及TimeEdit判断文件夹是否存在且未更新。
            /// 针对的是文件夹
            /// 如果条件为真，则返回pid（如果是文件夹，则为上一级目录的cid；如果是文件则为空）
            /// </summary>
            /// <param name="pickCode"></param>
            /// <param name="timeEdit"></param>
            /// <returns></returns>
            public static long GetLatestFolderPid(string pickCode, int timeEdit = 0)
            {
                long pid = -1;

                using SqliteConnection db =
                    new SqliteConnection(ConnectionString);

                db.Open();

                var selectCommand = new SqliteCommand
                    ($"SELECT pid FROM FilesInfo WHERE pc == '{pickCode}' and te == '{timeEdit}'", db);

                if (timeEdit == 0)
                {
                    selectCommand = new SqliteCommand
                        ($"SELECT pid FROM FilesInfo WHERE pc == '{pickCode}'", db);
                }

                var query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    if (query.FieldCount != 0)
                    {
                        pid = Convert.ToInt64(query["pid"]);
                        break;
                    }
                }

                db.Close();

                return pid;
            }

            /// <summary>
            /// 获取演员出演的视频信息（By ActorName）
            /// </summary>
            /// <param name="actorName"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static VideoInfo[] GetSingleVideoInfoByActor(string actorName, SqliteConnection connection = null)
            {
                string commandText;

                if (actorName == "")
                {
                    commandText = "SELECT * from VideoInfo WHERE actor == ''";
                }
                else
                {
                    commandText = $"SELECT * from VideoInfo WHERE actor LIKE '%{actorName}%'";
                }

                return DataAccessHelper.ExecuteReaderGetArray<VideoInfo>(commandText,connection);
            }

            /// <summary>
            /// 获取VideoInfo（By Name）
            /// </summary>
            /// <param name="name"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static VideoInfo[] GetSingleVideoInfoByName(string name, SqliteConnection connection = null)
            {
                var splitList = name.Split(new char[] { '-', '_' });
                var leftName = splitList[0];

                var rightNumber = string.Empty;
                if (splitList.Length != 1)
                {
                    rightNumber = splitList[1];
                }

                var commandText = $"SELECT * from VideoInfo WHERE truename LIKE '%{leftName}%{rightNumber}%'";


                return DataAccessHelper.ExecuteReaderGetArray<VideoInfo>(commandText,connection);
            }

            /// <summary>
            /// 获取演员出演的视频信息（By label）
            /// </summary>
            /// <param name="type"></param>
            /// <param name="label"></param>
            /// <param name="limit"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static VideoInfo[] GetVideoInfoBySomeType(string type, string label, int limit,SqliteConnection connection = null)
            {
                var commandText = string.IsNullOrEmpty(label) ? $"SELECT * from VideoInfo WHERE {type} == '' LIMIT {limit}"
                                                            : $"SELECT * from VideoInfo WHERE {type} LIKE '%{label}%' LIMIT {limit}";

                return DataAccessHelper.ExecuteReaderGetArray<VideoInfo>(commandText,connection);
            }

            /// <summary>
            /// 获取FileInfo（By TrueName）
            /// </summary>
            /// <param name="trueName"></param>
            /// <returns></returns>
            public static List<Datum> GetSingleFileInfoByTrueName(string trueName)
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
                var commandText = $"SELECT * from FilesInfo WHERE uid != 0 AND iv = 1 AND n LIKE '%{leftName}%{rightNumber}%'";

                var tmpList = DataAccessHelper.ExecuteReaderGetArray<Datum>(commandText, null);

                if (tmpList == null) return null;

                var data = new List<Datum>();
            
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

            /// <summary>
            /// 通过pid获取文件夹列表，PID的下一级目录
            /// </summary>
            /// <param name="pid"></param>
            /// <param name="limit"></param>
            /// <returns></returns>
            public static Datum[] GetFolderListByPid(long pid, int limit = -1)
            {
                var command = $"SELECT * FROM FilesInfo WHERE pid == {pid} LIMIT {limit}";
            
                return DataAccessHelper.ExecuteReaderGetArray<Datum>(command, null);
            }

            /// <summary>
            /// 通过cid获取文件和文件夹列表，CID的下一级目录（只有一级）
            /// </summary>
            /// <param name="cid"></param>
            /// <param name="limit"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static Datum[] GetListByCid(long? cid, int limit = -1, SqliteConnection connection = null)
            {
                var command = $"SELECT * FROM FilesInfo WHERE cid == {cid} and uid != 0 or pid == {cid} LIMIT {limit}";

                return DataAccessHelper.ExecuteReaderGetArray<Datum>(command, connection);
            }

            /// <summary>
            /// 通过pickCode和ua获取下载记录
            /// </summary>
            /// <param name="pickCode"></param>
            /// <param name="ua"></param>
            /// <returns></returns>
            public static DownInfo GetDownHistoryByPcAndUa(string pickCode, string ua)
            {
                var command = $"SELECT * FROM DownHistory WHERE file_pickcode == '{pickCode}' and ua == '{ua}' LIMIT 1";

                return DataAccessHelper.ExecuteReaderGetSingle<DownInfo>(command, null);
            }

            public static async Task<List<Datum>> GetAllFilesInFolderListAsync(List<Datum> dataList)
            {
                return await Task.Run(() => GetAllFilesInFolderList(dataList, null));
            }

            /// <summary>
            ///  遍历cid下所有的文件（文件和文件夹）
            /// </summary>
            /// <param name="cid"></param>
            /// <param name="allDatumList"></param>
            /// <returns></returns>
            public static List<Datum> GetAllFilesTraverse(long? cid, List<Datum> allDatumList = null)
            {
                allDatumList ??= new List<Datum>();

                var datumList = GetListByCid(cid);
                if (datumList == null) return null;

                allDatumList.AddRange(datumList);

                foreach (var datum in datumList)
                {
                    //文件夹
                    if (datum.Fid == default)
                    {
                        allDatumList = GetAllFilesTraverse(datum.Cid, allDatumList);
                    }
                }

                return allDatumList;
            }

            /// <summary>
            /// 获取标记为“稍后观看”的部分信息（*）
            /// </summary>
            /// <param name="count"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static async Task<VideoInfo[]> GetNameAndImageFromLookLater(int count = 10, SqliteConnection connection = null)
            {
                var commandText =
                    $"SELECT * FROM VideoInfo WHERE look_later != 0 ORDER BY look_later DESC LIMIT {count}";

                return await DataAccessHelper.ExecuteReaderGetArrayAsync<VideoInfo>(commandText,connection);
            }

            /// <summary>
            /// 获取标记为“稍后观看”的部分信息（*）
            /// </summary>
            /// <param name="count"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static async Task<VideoInfo[]> GetNameAndImageFromLike(int count = 10, SqliteConnection connection = null)
            {
                var commandText =
                    $"SELECT * FROM VideoInfo WHERE is_like != 0 LIMIT {count}";

                return await DataAccessHelper.ExecuteReaderGetArrayAsync<VideoInfo>(commandText, connection);
            }

            /// <summary>
            /// 随机获取信息（*）RANDOM
            /// </summary>
            /// <param name="limit"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static async Task<VideoInfo[]> GetNameAndImageRandom(int limit = 10, SqliteConnection connection = null)
            {
                var commandText =
                    $"SELECT * FROM VideoInfo ORDER BY RANDOM() LIMIT {limit}";

                return await DataAccessHelper.ExecuteReaderGetArrayAsync<VideoInfo>(commandText, connection);

            }

            /// <summary>
            /// 获取最近视频的部分信息（*）
            /// </summary>
            /// <param name="count"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static async Task<VideoInfo[]> GetNameAndImageRecent(int count = 10, SqliteConnection connection = null)
            {
                var commandText =
                    $"SELECT * FROM VideoInfo ORDER BY addtime DESC LIMIT {count}";

                return await DataAccessHelper.ExecuteReaderGetArrayAsync<VideoInfo>(commandText, connection);
            }

            /// <summary>
            /// 获取该cid文件夹上一层目录的信息
            /// </summary>
            /// <param name="cid"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static Datum GetUpperLevelFolderCid(long cid, SqliteConnection connection = null)
            {
                var commandText =
                    $"SELECT * FROM FilesInfo WHERE cid = {cid} AND fid == '' LIMIT 1";

                return DataAccessHelper.ExecuteReaderGetSingle<Datum>(commandText, connection);

            }

            /// <summary>
            /// 通过文件夹的Cid回溯根目录，不包括当前文件夹
            /// </summary>
            /// <param name="folderCid"></param>
            /// <returns></returns>
            public static List<Datum> GetRootByCid(long folderCid)
            {
                const int maxDepth = 30;

                List<Datum> folderToRootList = new();

                Datum upperLevelFolderInfo = new() { Pid = folderCid };
                if (upperLevelFolderInfo.Pid == null) return null;

                var pid = upperLevelFolderInfo.Pid;
                for (var i = 0; i < maxDepth; i++)
                {
                    if (pid == null) break;

                    upperLevelFolderInfo = GetUpperLevelFolderCid((long)pid);

                    if (upperLevelFolderInfo == null) break;

                    folderToRootList.Add(upperLevelFolderInfo);

                    pid = upperLevelFolderInfo.Pid;

                    if (pid == 0)
                    {
                        folderToRootList.Add(new Datum { Cid =0, Name = "根目录" });
                        break;
                    }
                }

                folderToRootList.Reverse();

                return folderToRootList;
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
            public static Dictionary<string, string> GetSubFile(string filePickCode)
            {
                Dictionary<string, string> subDicts = new();
                using (SqliteConnection db =
                    new SqliteConnection(ConnectionString))
                {
                    db.Open();


                    //查询文件名称
                    SqliteCommand selectCommand = new SqliteCommand
                        ($"SELECT cid,n FROM FilesInfo WHERE pc == '{filePickCode}'", db);

                    SqliteDataReader query = selectCommand.ExecuteReader();
                    query.Read();

                    //查询无果，几乎不可能发生
                    if (!query.HasRows)
                    {
                        query.Close();
                        db.Close();
                        return subDicts;
                    }

                    var folderCid = query["cid"] as string;
                    var fileName = query["n"] as string;
                    query.Close();

                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                    var nameIsNumber = TryParse(fileNameWithoutExtension, out _);

                    //1.首先查询同名字幕文件(纯数字则跳过，长度小于10也跳过)
                    if (fileNameWithoutExtension != null && !nameIsNumber && fileNameWithoutExtension.Length > 10)
                    {
                        selectCommand = new SqliteCommand
                            ($"SELECT pc, n FROM FilesInfo WHERE (ico == 'srt' OR ico == 'ass' OR ico == 'ssa') AND n LIKE '%{fileNameWithoutExtension}%'", db);

                        query = selectCommand.ExecuteReader();
                        while (query.Read())
                        {
                            subDicts.Add(query["pc"] as string ?? string.Empty, query["n"] as string);
                        }
                    }

                    //2.没有同名字幕文件，根据番号特点匹配（字母+数字），这里的方法正常的视频不通用
                    if (subDicts.Count == 0 && !nameIsNumber)
                    {
                        var matchName = FileMatch.MatchName(fileName);
                        if (string.IsNullOrEmpty(folderCid) || string.IsNullOrEmpty(matchName))
                        {
                            db.Close();
                            return subDicts;
                        }

                        var tuple = FileMatch.SplitLeftAndRightFromCid(matchName);
                        var leftName = tuple.Item1.Replace("FC2", "FC");
                        var rightNumber = tuple.Item2;

                        //通过truename查询字幕文件
                        selectCommand = new SqliteCommand
                            ($"SELECT pc,n FROM FilesInfo WHERE (ico == 'srt' OR ico == 'ass' OR ico == 'ssa') AND n LIKE '%{leftName}%{rightNumber}%'", db);

                        query = selectCommand.ExecuteReader();
                        while (query.Read())
                        {
                            subDicts.Add(query["pc"] as string ?? string.Empty, query["n"] as string);
                        }

                    }

                    //3.查询失败，从上一级文件夹的名称入手
                    if (subDicts.Count == 0)
                    {
                        //查询文件cid下的字幕
                        selectCommand = new SqliteCommand
                            ($"SELECT pc,n FROM FilesInfo WHERE (ico == 'srt' OR ico == 'ass' OR ico == 'ssa') AND cid == '{folderCid}'", db);

                        query = selectCommand.ExecuteReader();

                        Dictionary<string, string> tmpDict = new();
                        while (query.Read())
                        {
                            tmpDict.Add(query["pc"] as string ?? string.Empty, query["n"] as string);
                        }

                        if (!nameIsNumber)
                        {
                            subDicts = tmpDict;
                        }
                        else
                        {
                            foreach (var item in tmpDict)
                            {
                                var match = Regex.Match(item.Value, $"(^|[^0-9]){fileNameWithoutExtension}($|[^0-9])");

                                if (match.Success)
                                {
                                    subDicts.Add(item.Key, item.Value);
                                }
                            }
                        }
                    }

                    db.Close();
                }

                return subDicts;
            }

            /// <summary>
            /// 获取当前文件列表下所有的文件
            /// </summary>
            /// <param name="dataList"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static List<Datum> GetAllFilesInFolderList(List<Datum> dataList, SqliteConnection connection)
            {
                var isNeedCloseConnection = connection == null;
                if (connection == null)
                {
                    connection = new SqliteConnection(ConnectionString);
                    connection.Open();
                }

                List<Datum> newData = new();

                foreach (var currentFile in dataList)
                {
                    //文件夹
                    if (currentFile.Fid == null)
                    {
                        // 获取当前文件夹下所有的文件夹和文件
                        var newDataList = GetListByCid(currentFile.Cid, connection:connection);

                        if(newDataList == null) continue;

                        var fileInFolderList = GetAllFilesInFolderList(newDataList.ToList(), connection);

                        newData.AddRange(fileInFolderList);
                    }
                }

                newData.AddRange(dataList);

                if (!isNeedCloseConnection) return newData;

                connection.Close();
                connection.Dispose();

                return newData;
            }

            /// <summary>
            /// 是否所有的搜刮源都尝试搜刮了name
            /// </summary>
            /// <param name="queryName"></param>
            /// <param name="connection"></param>
            /// <returns></returns>
            public static bool GetIsAllSpiderSourceAttempt(string queryName, SqliteConnection connection = null)
            {
                var commandText = $"SELECT Name FROM SpiderTask WHERE Name == '{queryName}' AND " +
                                  $"libre == 'done' AND bus == 'done' AND Jav321 == 'done' AND Avmoo == 'done' AND Avsox == 'done' AND fc == 'done' AND db == 'done' " +
                                  $"AND done == 0";

                var name = DataAccessHelper.ExecuteScalar<string>(commandText, connection);

                return !string.IsNullOrEmpty(name);
            }
        }
    }

}
