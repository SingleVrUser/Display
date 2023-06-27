using Display.Extensions;
using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using static System.Int32;
using System.Xml.Linq;
using LiveChartsCore.SkiaSharpView.Painting;
using Windows.Media;
using System.Dynamic;
using static Display.ContentsPage.SpiderVideoInfo.FileStatistics;
using SkiaSharp;
using static Display.Spider.Manager;

namespace Display.Data
{
    public static class DataAccess
    {
        private const string DbName = "115_uwp.db";
        public static string DbPath => NewDbPath(AppSettings.DataAccessSavePath);

        public static string ConnectionString => ConnectionString;

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
                                      $"EXISTS VideoInfo ( " +
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
                                      $"EXISTS Is_Wm ( " +
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
                                      $"EXISTS DownHistory ( " +
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
                                      $"EXISTS ActorInfo ( " +
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
                                      $"EXISTS bwh ( " +
                                      "bwh text," +
                                      "bust integer," +
                                      "waist integer," +
                                      "hips integer," +
                                      "PRIMARY KEY('bwh')" +
                                      ") ";
            createTable.ExecuteNonQuery();

            //FailList_islike_looklater
            createTable.CommandText = "CREATE TABLE IF NOT " +
                                      $"EXISTS FailList_islike_looklater ( " +
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
                                      $"EXISTS Actor_Names ( " +
                                      "id INTEGER NOT NULL," +
                                      "Name TEXT NOT NULL," +
                                      "PRIMARY KEY('id','Name')" +
                                      ") ";
            createTable.ExecuteNonQuery();

            //演员视频中间表
            createTable.CommandText = "CREATE TABLE IF NOT " +
                                      $"EXISTS Actor_Video ( " +
                                      "actor_id INTEGER NOT NULL," +
                                      "video_name TEXT NOT NULL," +
                                      "PRIMARY KEY('actor_id','video_name')" +
                                      ") ";
            createTable.ExecuteNonQuery();

            //搜刮记录
            createTable.CommandText = "CREATE TABLE IF NOT " +
                                      $"EXISTS SpiderLog ( " +
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
                                      $"EXISTS SpiderTask ( " +
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
        /// 更新版本14之前的数据
        /// </summary>
        /// <returns></returns>
        public static async Task UpdateDatabaseFrom14()
        {
            //更新演员数据
            Dictionary<string, List<string>> actorsInfoDict = new();

            //加载全部数据
            var videoInfoList = await DataAccess.LoadVideoInfo(-1);
            foreach (var videoInfo in videoInfoList)
            {
                string actorStr = videoInfo.actor;

                var actorList = actorStr.Split(",");
                foreach (var actor in actorList)
                {
                    //当前名称不存在
                    if (!actorsInfoDict.ContainsKey(actor))
                    {
                        actorsInfoDict.Add(actor, new List<string>());
                    }
                    actorsInfoDict[actor].Add(videoInfo.truename);
                }
            }

            foreach (var item in actorsInfoDict)
            {
                AddActorInfo(item.Key, item.Value);
            }
        }

        /// <summary>
        /// 添加演员信息
        /// </summary>
        /// <param name="actorName"></param>
        /// <param name="videoNameList"></param>
        public static void AddActorInfo(string actorName, List<string> videoNameList)
        {
            string singleActorName;
            var isWoman = 1;
            string[] otherNames = null;

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
            var actorId = GetActorIdByName(singleActorName);
            //数据库中不存在该名称
            if (actorId == -1)
            {
                ActorInfo actorInfo = new() { name = singleActorName, is_woman = isWoman };

                //检查演员图片是否存在
                string imagePath = Path.Combine(AppSettings.ActorInfoSavePath, singleActorName, "face.jpg");
                if (File.Exists(imagePath))
                {
                    actorInfo.prifile_path = imagePath;
                }

                actorId = InsertActorInfo(actorInfo);
            }
            //为了弥补，之前所有的is_woman都默认为1
            else if (isWoman == 0)
            {
                UpdateActorInfoIsWoman(actorId, isWoman);
            }

            //添加Actor_Names
            //主名称
            AddOrIgnoreActor_Names(actorId, singleActorName);
            //别名
            if (otherNames != null)
            {
                foreach (var name in otherNames)
                {
                    AddOrIgnoreActor_Names(actorId, name);
                }
            }

            //添加演员和作品的信息
            foreach (var videoName in videoNameList)
            {
                AddOrIgnoreActor_Video(actorId, videoName);
            }
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
                $"EXISTS ProducerInfo ( " +
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


        /// <summary>
        /// 执行命令但不查询
        /// </summary>
        /// <param name="command"></param>
        private static void ExecuteNonQuery(string command)
        {
            using var db = new SqliteConnection(ConnectionString);

            db.Open();

            var sqliteCommand = new SqliteCommand(command, db);
            sqliteCommand.ExecuteNonQuery(); 

            db.Close();
        }

        /// <summary>
        /// 执行命令（带参数）但不查询
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterCollection"></param>
        private static void ExecuteNonQueryWithParameters(string command, List<SqliteParameter> parameterCollection)
        {
            using var db = new SqliteConnection(ConnectionString); 

            db.Open();  

            var sqliteCommand = new SqliteCommand(command, db)
            {
                Parameters = { parameterCollection }
            };

            sqliteCommand.ExecuteNonQuery(); 

            db.Close();

        }


        public enum TableName
        {
            VideoInfo, FilesInfo, DownHistory, SpiderLog, SpiderTask
        }

        #endregion



        #region 删除

        /// <summary>
        /// 清空指定表
        /// </summary>
        /// <param name="tableName"></param>
        public static void DeleteTable(TableName tableName)
        {
            var command = $"delete from {tableName}";
            ExecuteNonQuery(command);
        }

        /// <summary>
        /// 删除VideoInfo表中的一条记录
        /// </summary>
        public static void DeleteDataInVideoInfoTable(string value, string key = "truename")
        {
            var command = $"DELETE FROM VideoInfo WHERE {key} == '{value}'";
            ExecuteNonQuery(command);
        }

        /// <summary>
        /// 删除特定pc的表FilesInfo和中间表FileToInfo
        /// </summary>
        /// <param name="pc"></param>
        public static void DeleteDataInFilesInfoAndFileToInfo(string pc)
        {
            using var db = new SqliteConnection(ConnectionString);

            db.Open();

            var command = new SqliteCommand()
            {
                Connection = db
            };

            // 先删除 FilesInfo
            command.CommandText = $"DELETE FROM FilesInfo WHERE pc == '{pc}' COLLATE NOCASE";
            command.ExecuteScalar();

            // 后删除中间表 FileToInfo
            command.CommandText = $"DELETE FROM FileToInfo WHERE file_pickcode == '{pc}' COLLATE NOCASE";
            command.ExecuteScalar();

            db.Close();

        }

        /// <summary>
        /// 删除FilesInfo表里文件夹下的所有文件和文件夹（最多两级）
        /// </summary>
        public static void DeleteDirectoryAndFiles_InFilesInfoTable(long cid)
        {
            var command = $"DELETE FROM FilesInfo WHERE cid == '{cid}' or pid = '{cid}'";

            ExecuteNonQuery(command);
        }

        /// <summary>
        /// 删除FilesInfo表里文件夹下的所有文件和文件夹（遍历所有）
        /// </summary>
        /// <param name="cid"></param>
        public static void DeleteAllDirectoryAndFiles_InFilesInfoTable(long cid)
        {
            var files = GetAllFilesTraverse(cid);
            List<long> cidList = new();

            foreach (var file in files)
            {
                if (!cidList.Contains(file.cid))
                {
                    cidList.Add(file.cid);
                }
            }
            foreach (var cidFolder in cidList)
            {
                DeleteDirectoryAndFiles_InFilesInfoTable(cidFolder);
            }
        }

        #endregion

        #region 添加


        /// <summary>
        /// 添加Datum信息到FilesInfo
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task AddFilesInfoAsync(Datum data)
        {

            var keyList = new List<string>();
            foreach (var item in data.GetType().GetProperties())
            {
                //fl为数组，应添加进新表中，一对多。目前暂不考虑，故跳过
                if (item.Name == "fl") continue;

                keyList.Add($"@{item.Name}");
            }
            //唯一值（pc）重复 则代替 （replace）
            var commandText = $"INSERT OR REPLACE INTO FilesInfo VALUES ({string.Join(",", keyList)});";


            var parameters = new List<SqliteParameter>();
            foreach (var item in data.GetType().GetProperties())
            {
                //fl为数组，应添加进新表中，一对多。目前暂不考虑，故跳过
                if (item.Name == "fl") continue;

                parameters.Add(new SqliteParameter("@" + item.Name, $"{item.GetValue(data)}"));
            }

            ExecuteNonQueryWithParameters(commandText,parameters);
        }


        /// <summary>
        /// 添加FileToInfo表
        /// </summary>
        /// <param name="pickCode"></param>
        /// <param name="trueName"></param>
        /// <param name="isSuccess"></param>
        /// <param name="isReplace"></param>
        public static void AddFileToInfo(string pickCode, string trueName, bool isSuccess = false, bool isReplace = false)
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
            ExecuteNonQueryWithParameters(commandText, parameters);
        }

        /// <summary>
        /// 添加视频信息
        /// </summary>
        /// <param name="data"></param>
        public static void AddVideoInfo(VideoInfo data)
        {
            using var db = new SqliteConnection(ConnectionString);
            db.Open();
                
            var insertCommand = new SqliteCommand()
            {
                Connection = db
            };
                
            Dictionary<string, string> dictionary = new();
            foreach (var item in data.GetType().GetProperties())
            {
                if (item.Name == "is_wm")
                    continue;

                var value = string.Empty + item.GetValue(data);

                //去除演员性别标记
                if (item.Name == "actor")
                {
                    value = Spider.JavDB.RemoveGenderFromActorListString(value);
                }

                dictionary.Add($"@{item.Name}", $"{value}");
            }

            //添加信息，如果已经存在则跳过
            insertCommand.CommandText = $"INSERT OR IGNORE INTO VideoInfo VALUES ({string.Join(",", dictionary.Keys)});";

            foreach (var item in dictionary)
            {
                insertCommand.Parameters.AddWithValue(item.Key, item.Value);
            }
            insertCommand.ExecuteNonQuery();

            //添加演员信息
            AddActorInfoByActorInfo(data, new List<string> { data.truename }, db);

            //添加是否步兵
            AddOrReplaceIs_Wm(data.truename, data.producer, data.is_wm);

            db.Close();
        }

        /// <summary>
        /// 添加下载记录
        /// </summary>
        /// <param name="data"></param>
        public static void AddDownHistory(DownInfo data)
        {
            const string commandText = "INSERT OR REPLACE INTO DownHistory VALUES (@file_pickcode,@file_name,@true_url,@ua,@add_time);";

            var parameters = new List<SqliteParameter>
            {
                new("@file_pickcode", data.pickCode),
                new("@file_name", data.fileName),
                new("@true_url", data.trueUrl),
                new("@ua", data.ua),
                new("@add_time", data.addTime)
            };
            ExecuteNonQueryWithParameters(commandText, parameters);
        }

        /// <summary>
        /// 插入或替换是否Is_Wm的表数据
        /// </summary>
        /// <param name="trueName"></param>
        /// <param name="producer"></param>
        /// <param name="isWm"></param>
        public static void AddOrReplaceIs_Wm(string trueName, string producer, int isWm)
        {
            const string commandText = "INSERT OR REPLACE INTO Is_Wm VALUES (@truename,@is_wm)";

            var parameters = new List<SqliteParameter>
            {
                new("@truename", trueName),
                new("@is_wm", isWm)
            };
            ExecuteNonQueryWithParameters(commandText, parameters);
        }


        /// <summary>
        /// 插入或替换表FailList_islike_looklater的表数据
        /// </summary>
        /// <param name="failInfo"></param>
        public static void AddOrReplaceFailList_Islike_Looklater(FailInfo failInfo)
        {
            const string commandText = "INSERT OR REPLACE INTO FailList_islike_looklater VALUES (@pc,@is_like,@score,@look_later,@image_path)";

            var parameters = new List<SqliteParameter>
            {
                new("@pc", failInfo.pc),
                new("@is_like", failInfo.is_like),
                new("@score", failInfo.score),
                new("@look_later", failInfo.look_later),
                new("@image_path", failInfo.image_path)
            };
            ExecuteNonQueryWithParameters(commandText, parameters);
        }

        public static void AddOrIgnoreActor_Video(long actorId, string videoName)
        {
            const string commandText = "INSERT OR IGNORE INTO Actor_Video VALUES (@actor_id,@video_name)";

            var parameters = new List<SqliteParameter>
            {
                new("@actor_id", actorId),
                new("@video_name", videoName)
            };
            ExecuteNonQueryWithParameters(commandText, parameters);
        }

        public static void AddOrIgnoreActor_Names(long id, string name)
        {
            const string commandText = "INSERT OR IGNORE INTO Actor_Names VALUES (@id,@Name)";

            var parameters = new List<SqliteParameter>
            {
                new("@id", id),
                new("@Name", name)
            };
            ExecuteNonQueryWithParameters(commandText, parameters);
        }

        public static void AddOrIgnoreBwh(string bwh, int bust, int waist, int hips)
        {
            const string commandText = "INSERT OR IGNORE INTO bwh VALUES (@bwh,@bust,@waist,@hips)";

            var parameters = new List<SqliteParameter>
            {
                new("@bwh", bwh),
                new("@bust", bust),
                new("@waist", waist),
                new("@hips", hips)
            };
            ExecuteNonQueryWithParameters(commandText, parameters);
        }


        /// <summary>
        /// 添加或忽略步兵的生厂商
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isWm"></param>
        public static void AddOrIgnoreWm_Producer(string name, int isWm)
        {
            if (string.IsNullOrEmpty(name)) return;

            //添加信息，如果已经存在则替换
            const string commandText = "INSERT OR IGNORE INTO ProducerInfo VALUES (@Name,@is_wm)";

            var parameters = new List<SqliteParameter>
            {
                new("@Name", name),
                new("@is_wm", isWm)
            };
            ExecuteNonQueryWithParameters(commandText, parameters);
        }

        /// <summary>
        /// 插入演员信息并返回actor_id
        /// </summary>
        /// <param name="actorInfo"></param>
        /// <returns></returns>
        public static long InsertActorInfo(ActorInfo actorInfo)
        {
            using var db = new SqliteConnection(ConnectionString);
            db.Open();

            var command = new SqliteCommand()
            {
                Connection = db
            };
            
            //插入新记录
            command.CommandText = $"INSERT INTO ActorInfo VALUES (NULL,@Name,@is_woman,@birthday,@bwh,@height,@works_count,@work_time,@prifile_path,@blog_url,@is_like,@addtime,@info_url);";
            command.Parameters.AddWithValue("@Name", actorInfo.name);
            command.Parameters.AddWithValue("@is_woman", actorInfo.is_woman);
            command.Parameters.AddWithValue("@birthday", actorInfo.birthday);
            command.Parameters.AddWithValue("@bwh", actorInfo.bwh);
            command.Parameters.AddWithValue("@height", actorInfo.height);
            command.Parameters.AddWithValue("@works_count", actorInfo.works_count);
            command.Parameters.AddWithValue("@work_time", actorInfo.work_time);
            command.Parameters.AddWithValue("@prifile_path", actorInfo.prifile_path);
            command.Parameters.AddWithValue("@blog_url", actorInfo.blog_url);
            command.Parameters.AddWithValue("@is_like", actorInfo.is_like);
            command.Parameters.AddWithValue("@addtime", actorInfo.addtime);
            command.Parameters.AddWithValue("@info_url", actorInfo.info_url);
            command.ExecuteReader();

            //查询Task_ID
            command = new SqliteCommand($"SELECT id FROM ActorInfo WHERE Name == '{actorInfo.name}'", db);
            var obj = command.ExecuteScalar();
            if (obj == null) return 0;

            var actorId = (long)obj;

            db.Close();

            return actorId;
        }


        /// <summary>
        /// 插入搜刮日志并返回Task_id
        /// </summary>
        /// <param Name="data"></param>
        public static long InsertSpiderLog()
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
            if(obj == null) return 0;
            var taskId = (long)obj;
                
            db.Close();

            return taskId;
        }

        /// <summary>
        /// 添加搜刮任务
        /// </summary>
        /// <param name="name"></param>
        /// <param name="taskId"></param>
        public static void AddSpiderTask(string name, long taskId)
        {
            const string commandText = "INSERT OR REPLACE INTO SpiderTask VALUES (@Name,@bus,@Jav321,@Avmoo,@Avsox,@libre,@fc,@db,@done,@task_id);";

            //fc只有javdb，fc2club能刮
            var isFc = FileMatch.IsFC2(name);

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

            ExecuteNonQueryWithParameters(commandText, parameters);
        }

        #endregion

        #region 更新

        /// <summary>
        /// 更新FailInfo表
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void UpdateSingleFailInfo(string pc, string key, string value)
        {
            var command = $"UPDATE FailList_islike_looklater set {key} = '{value}' WHERE pc = '{pc}'";

            ExecuteNonQuery(command);
        }

        /// <summary>
        /// 更新FileToInfo表
        /// </summary>
        /// <param name="trueName"></param>
        /// <param name="isSuccess"></param>
        public static void UpdateFileToInfo(string trueName, bool isSuccess)
        {
            var command = $"UPDATE FileToInfo set issuccess = {isSuccess} WHERE truename = '{trueName}'";

            ExecuteNonQuery(command);
        }

        /// <summary>
        /// 更新ActorInfo表
        /// </summary>
        /// <param Name="name"></param>
        /// <param Name="actorInfo"></param>
        public static void UpdateActorInfo(long actorId, ActorInfo actorInfo)
        {
            var commandText = $"UPDATE ActorInfo set birthday = @birthday, bwh = @bwh, height = @height, works_count = @works_count, work_time = @work_time, blog_url = @blog_url, info_url = @info_url, addtime = @addtime WHERE id = '{actorId}'";

            var parameters = new List<SqliteParameter>
            {
                new("@birthday", actorInfo.birthday),
                new ("@bwh", actorInfo.bwh),
                new ("@height", actorInfo.height),
                new ("@works_count", actorInfo.works_count),
                new ("@work_time", actorInfo.work_time),
                new ("@blog_url", actorInfo.blog_url),
                new ("@info_url", actorInfo.info_url),
                new ("@addtime", actorInfo.addtime)
            };

            ExecuteNonQueryWithParameters(commandText, parameters);
        }


        /// <summary>
        /// 更新演员性别
        /// </summary>
        /// <param Name="truename"></param>
        /// <param Name="key"></param>
        /// <param Name="value"></param>
        public static void UpdateActorInfoIsWoman(long id, int is_woman)
        {
            var command = $"UPDATE ActorInfo SET is_woman = '{is_woman}' WHERE id = '{id}'";

            ExecuteNonQuery(command);

        }

        /// <summary>
        /// 更新演员头像地址
        /// </summary>
        /// <param Name="truename"></param>
        /// <param Name="key"></param>
        /// <param Name="value"></param>
        public static void UpdateActorInfoPrifilePath(long id, string prifile_path)
        {
            var command = $"UPDATE ActorInfo SET prifile_path = '{prifile_path}' WHERE id = '{id}'";

            ExecuteNonQuery(command);
        }


        /// <summary>
        /// 更新数据信息，VideoInfo的单个字段
        /// </summary>
        /// <param Name="truename"></param>
        /// <returns></returns>
        public static void UpdateSingleDataFromVideoInfo(string truename, string key, string value)
        {
            var command = $"UPDATE VideoInfo SET {key} = '{value}' WHERE truename = '{truename}'";

            ExecuteNonQuery(command);
        }


        /// <summary>
        /// 更新演员信息，ActorInfo的单个字段
        /// </summary>
        /// <param Name="truename"></param>
        /// <returns></returns>
        public static void UpdateSingleDataFromActorInfo(string id, string key, string value)
        {
            var command = $"UPDATE ActorInfo SET {key} = '{value}' WHERE id = '{id}'";

            ExecuteNonQuery(command);
        }

        /// <summary>
        /// 更新VideoInfo数据（不包括个性化的内容（喜欢，稍后观看，评分））
        /// </summary>
        /// <param Name="videoInfo"></param>
        public static void UpdateDataFromVideoInfo(VideoInfo videoInfo)
        {
            using var db = new SqliteConnection(ConnectionString);
            db.Open();

            var command = new SqliteCommand()
            {
                Connection = db
            };

            Dictionary<string, Tuple<string, string>> dict = new();
            foreach (var item in videoInfo.GetType().GetProperties())
            {
                var name = item.Name;
                var value = string.Empty + item.GetValue(videoInfo);

                //忽略是否步兵
                //忽略自定义数据
                if (name == "look_later" || name == "score" || name == "is_like" || name == "is_wm")
                    continue;

                if (name == "actor")
                {
                    value = Spider.JavDB.TrimGenderFromActorName(value);
                }

                dict.Add($"@{name}", new Tuple<string, string>($"{name} = @{name}", $"{value}"));
            }

            command.CommandText = $"UPDATE VideoInfo SET {string.Join(",", dict.Values.ToList().Select(item => item.Item1))} WHERE truename == @truename";

            foreach (var item in dict)
            {
                command.Parameters.AddWithValue(item.Key, item.Value.Item2);
            }

            command.ExecuteNonQuery();

            //更新是否步兵
            AddOrReplaceIs_Wm(videoInfo.truename, videoInfo.producer, videoInfo.is_wm);

            //更新演员信息
            //先删除Actor_Videos中所有Video_name的数据
            command = new SqliteCommand($"DELETE FROM Actor_Video WHERE video_name =='{videoInfo.truename}'", db);
            command.ExecuteNonQuery();

            AddActorInfoByActorInfo(videoInfo, new List<string> { videoInfo.truename }, db);

            db.Close();
        }

        private static void AddActorInfoByActorInfo(VideoInfo videoInfo, List<string> videoNameList, SqliteConnection db)
        {
            var actorStr = videoInfo.actor;
            var actorList = actorStr.Split(",");
            foreach (var actorName in actorList)
            {
                //查询Actor_ID
                SqliteCommand command = new($"SELECT id FROM ActorInfo WHERE Name == '{Spider.JavDB.TrimGenderFromActorName(actorName)}'", db);

                if (command.ExecuteScalar() is long actorId)
                {
                    //添加信息，如果已经存在则忽略
                    command.CommandText = $"INSERT OR IGNORE INTO Actor_Video VALUES (@actor_id,@video_name)";
                    command.Parameters.AddWithValue("@actor_id", actorId);
                    command.Parameters.AddWithValue("@video_name", videoInfo.truename);
                    command.ExecuteNonQuery();
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
        /// 更新图片地址
        /// </summary>
        /// <param Name="truename"></param>
        /// <returns></returns>
        public static void UpdateAllImagePath(string srcPath, string dstPath)
        {
            var command = $"update VideoInfo set imagepath = REPLACE(imagepath,'{srcPath}','{dstPath}')";

            ExecuteNonQuery(command);
        }


        /// <summary>
        /// 更新演员头像地址
        /// </summary>
        /// <param Name="truename"></param>
        /// <returns></returns>
        public static void UpdateActorProfilePath(string srcPath, string dstPath)
        {
            var command = $"update ActorInfo set prifile_path = REPLACE(prifile_path,'{srcPath}','{dstPath}')";

            ExecuteNonQuery(command);
        }

        /// <summary>
        /// 更新SpiderTask表（源搜刮中，源搜刮完成，全部搜刮完成）
        /// </summary>
        /// <param Name="truename"></param>
        /// <param Name="isSuccess"></param>
        public static void UpdateSpiderTask(string Name, Spider.Manager.SpiderSource SpiderSource, SpiderStates SpiderSourceStatus, bool IsAllDone = false)
        {
            var command = $"update SpiderTask SET {SpiderSource.Name} = '{SpiderSourceStatus}' , done = {IsAllDone} WHERE Name == '{Name}'";

            ExecuteNonQuery(command);
        }

        /// <summary>
        /// 更新SpiderLog中的Task_Id为已完成
        /// </summary>
        /// <param Name="task_id"></param>
        public static void UpdateSpiderLogDone(long task_id)
        {
            var command = $"UPDATE SpiderLog SET done = 1 WHERE task_id == {task_id}";

            ExecuteNonQuery(command);
        }

        #endregion

        #region 查询

        /// <summary>
        /// 通过trueName查询文件列表
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Datum>> FindFileInfoByTrueName(string truename)
        {
            await using var db = new SqliteConnection(ConnectionString);
            db.Open();

            //COLLATE NOCASE 忽略大小写
            var selectCommand = new SqliteCommand
                ($"SELECT * FROM FilesInfo,FileToInfo WHERE FilesInfo.pc == FileToInfo.file_pickcode AND FileToInfo.truename == '{truename}' COLLATE NOCASE", db);

            var query = await selectCommand.ExecuteReaderAsync();

            var data = new List<Datum>();
            while (query.Read())
            {
                data.Add(TryCovertQueryToDatum(query));
            }

            db.Close();

            return data;
        }

        /// <summary>
        /// 加载已存在的一条videoInfo数据
        /// </summary>
        /// <param Name="truename"></param>
        /// <returns></returns>
        public static VideoInfo LoadOneVideoInfoByCID(string truename)
        {
            if (truename == null) return null;

            VideoInfo data = null;

            using SqliteConnection db = new SqliteConnection(ConnectionString);
            db.Open();

            var selectCommand = new SqliteCommand ($"SELECT * from VideoInfo WHERE truename = '{truename}' LIMIT 1", db);

            var query = selectCommand.ExecuteReader();
            if (query.Read())
            {
                data = ConvertQueryToVideoInfo(query);
            }

            db.Close();

            return data;
        }

        /// <summary>
        /// 查找truename
        /// </summary>
        /// <param Name="truename"></param>
        /// <returns></returns>
        public static List<string> SelectTrueName(string truename)
        {
            var entries = new List<string>();

            using var db = new SqliteConnection(ConnectionString);

            db.Open();

            string commandText;
            if (truename.Contains("_"))
            {
                commandText =
                    $"SELECT truename FROM VideoInfo WHERE truename COLLATE NOCASE in ('{truename}', '{truename.Replace('_', '-')}', '{truename.Replace("_", "")}' )";
            }
            else
            {
                commandText =
                    $"SELECT truename FROM VideoInfo WHERE truename COLLATE NOCASE in ('{truename}', '{truename.Replace("-", "_")}', '{truename.Replace("-", "")}')";
            }

            var selectCommand = new SqliteCommand(commandText,db);

            var query = selectCommand.ExecuteReader();

            while (query.Read())
            {
                entries.Add(query.GetString(0));
            }

            db.Close();

            return entries;
        }

        /// <summary>
        /// 加载DBNAME数据
        /// </summary>
        /// <returns></returns>
        public static List<Datum> LoadDataAccess()
        {
            var data = new List<Datum>();

            using var db = new SqliteConnection(ConnectionString);
            db.Open();

            var selectCommand = new SqliteCommand
                ("SELECT * from FilesInfo", db);

            var query = selectCommand.ExecuteReader();

            while (query.Read())
            {
                data.Add(TryCovertQueryToDatum(query));
            }
            db.Close();

            return data;
        }

        public static async Task<FailInfo> LoadSingleFailInfo(string pc)
        {
            FailInfo failInfo = null;
            using (SqliteConnection db =
               new SqliteConnection(ConnectionString))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT info.*,fail.is_like, fail.score, fail.look_later, fail.image_path  FROM FailList_islike_looklater as fail LEFT JOIN FilesInfo as info on info.pc = fail.pc WHERE fail.pc = '{pc}' LIMIT 1", db);

                var query = await selectCommand.ExecuteReaderAsync();

                if (query.Read())
                {
                    failInfo = tryCovertQueryToFailInfo(query);
                }
                //无结果
                else
                {
                    failInfo = null;
                }

                db.Close();
            }

            return failInfo;
        }

        /// <summary>
        /// 查询失败列表（FailInfo格式）
        /// </summary>
        /// <returns></returns>
        public static async Task<List<FailInfo>> LoadFailFileInfoWithFailInfo(int offset = 0, int limit = -1, FailInfoShowType showType = FailInfoShowType.like)
        {
            List<FailInfo> data = new();

            using (SqliteConnection db =
               new SqliteConnection(ConnectionString))
            {
                db.Open();

                string showTypeStr;
                if (showType == FailInfoShowType.like)
                {
                    showTypeStr = " WHERE is_like = 1";
                }
                else
                {
                    showTypeStr = " WHERE look_later != 0";
                }

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT info.*,fail.is_like, fail.score, fail.look_later, fail.image_path  FROM FailList_islike_looklater as fail LEFT JOIN FilesInfo as info on info.pc = fail.pc{showTypeStr} LIMIT {limit} offset {offset}", db);

                SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

                while (query.Read())
                {
                    data.Add(tryCovertQueryToFailInfo(query));
                }

                db.Close();
            }

            return data;
        }

        public static string GetFileNameFromPickCode(string pc)
        {
            string fileName = null;

            using (SqliteConnection db =
                   new SqliteConnection(ConnectionString))
            {
                db.Open();

                var selectCommand = new SqliteCommand
                    ($"SELECT n from FilesInfo WHERE pc == '{pc}' LIMIT 1", db);

                var query = selectCommand.ExecuteScalar();

                if (query != null)
                {
                    fileName = query.ToString();
                }

                db.Close();
            }

            return fileName;
        }

        /// <summary>
        /// 查询失败列表（Datum格式）
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Datum>> LoadFailFileInfoWithDatum(int offset = 0, int limit = -1, string n = null, string orderBy = null, bool isDesc = false, FailType showType = FailType.All)
        {
            List<Datum> data = new List<Datum>();

            using (SqliteConnection db =
               new SqliteConnection(ConnectionString))
            {
                db.Open();

                string orderStr = GetOrderStr(orderBy, isDesc);

                string queryStr = string.IsNullOrEmpty(n) ? string.Empty : $" And FilesInfo.n LIKE '%{n.Replace("'", "%")}%'";

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

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT * FROM FilesInfo,FileToInfo WHERE FileToInfo.issuccess == 0 AND FilesInfo.pc == FileToInfo.file_pickcode{showTypeStr}{queryStr}{orderStr} LIMIT {limit} offset {offset} ", db);

                SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

                while (query.Read())
                {
                    data.Add(TryCovertQueryToDatum(query));
                }

                db.Close();
            }

            return data;
        }

        public static int GetCount_FailFileInfoWithDatum(int offset = 0, int limit = -1, string n = null, string orderBy = null, bool isDesc = false, FailType showType = FailType.All)
        {
            int count = 0;

            using (SqliteConnection db =
               new SqliteConnection(ConnectionString))
            {
                db.Open();

                string orderStr = GetOrderStr(orderBy, isDesc);

                string queryStr = string.IsNullOrEmpty(n) ? string.Empty : $" And FilesInfo.n LIKE '%{n.Replace("'", "%")}%'";

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

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT COUNT(pc) FROM FilesInfo,FileToInfo WHERE FileToInfo.issuccess == 0 AND FilesInfo.pc == FileToInfo.file_pickcode{showTypeStr}{queryStr}{orderStr} LIMIT {limit} offset {offset} ", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                query.Read();
                count = query.GetInt32(0);
                query.Close();

                db.Close();
            }

            return count;
        }

        /// <summary>
        /// 检查演员列表数量
        /// </summary>
        /// <param Name="n"></param>
        /// <returns></returns>
        public static int CheckActorInfoCount(List<string> filterList = null)
        {
            int count = 0;
            using (SqliteConnection db =
               new SqliteConnection(ConnectionString))
            {
                db.Open();

                string filterStr = string.Empty;
                if (filterList != null)
                {
                    filterStr = $" WHERE {string.Join(",", filterList)}";
                }

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT COUNT(id) FROM ActorInfo{filterStr}", db);


                SqliteDataReader query = selectCommand.ExecuteReader();

                query.Read();
                count = query.GetInt32(0);
                query.Close();

                db.Close();
            }

            return count;
        }

        public static int CheckFailInfosCount(FailInfoShowType showType)
        {
            int count = 0;
            using (SqliteConnection db =
               new SqliteConnection(ConnectionString))
            {
                db.Open();

                string showTypeStr;
                if (showType == FailInfoShowType.like)
                {
                    showTypeStr = " WHERE is_like = 1";
                }
                else
                {
                    showTypeStr = " WHERE look_later != 0";
                }

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT COUNT(pc) FROM FailList_islike_looklater {showTypeStr}", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                query.Read();
                count = query.GetInt32(0);
                query.Close();
                db.Close();
            }

            return count;
        }



        public static int CheckFailDatumFilesCount(string n = "", FailType showType = FailType.All)
        {
            if (!string.IsNullOrEmpty(n) && n.Contains("'"))
            {
                n = n.Replace("'", "%");
            }

            int count = 0;
            using (SqliteConnection db =
               new SqliteConnection(ConnectionString))
            {
                db.Open();

                string queryStr = string.IsNullOrEmpty(n) ? string.Empty : $" And FilesInfo.n LIKE '%{n.Replace("'", "%")}%'";

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

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT COUNT(n) FROM FilesInfo,FileToInfo WHERE FileToInfo.issuccess == 0 AND FilesInfo.pc == FileToInfo.file_pickcode{showTypeStr}{queryStr}", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                query.Read();
                count = query.GetInt32(0);
                query.Close();
                db.Close();
            }

            return count;
        }

        /// <summary>
        /// 检查VideoInfo表的数量
        /// </summary>
        /// <param Name="n"></param>
        /// <returns></returns>
        public static int CheckVideoInfoCount(string orderBy = null, bool isDesc = false, List<string> filterConditionList = null, string filterKeywords = null, Dictionary<string, string> rangesDicts = null)
        {
            int count = 0;
            using (SqliteConnection db =
               new SqliteConnection(ConnectionString))
            {
                db.Open();

                //排序
                string orderStr = GetOrderStr(orderBy, isDesc);

                //筛选
                string filterStr = GetVideoInfoFilterStr(filterConditionList, filterKeywords, rangesDicts);

                SqliteCommand selectCommand;

                //多表查询
                if (rangesDicts != null && rangesDicts.ContainsKey("Type"))
                {
                    selectCommand = new SqliteCommand($"SELECT COUNT(VideoInfo.truename) FROM VideoInfo LEFT JOIN Is_Wm ON VideoInfo.truename = Is_Wm.truename LEFT JOIN ProducerInfo ON VideoInfo.producer = ProducerInfo.Name{filterStr}{orderStr}", db);
                }
                //普通查询
                else
                {
                    selectCommand = new SqliteCommand
                        ($"SELECT COUNT(VideoInfo.truename) from VideoInfo{filterStr}{orderStr}", db);
                }

                SqliteDataReader query = selectCommand.ExecuteReader();

                query.Read();
                count += query.GetInt32(0);
                query.Close();

                db.Close();
            }

            return count;
        }


        /// <summary>
        /// 查询最近一次的未完成搜刮记录
        /// </summary>
        /// <returns>Tuple (task_id,add_time)</returns>
        public static Tuple<long, long> GetLatestUnfinishedSpiderLog()
        {
            Tuple<long, long> tuple = null;
            using (SqliteConnection db =
            new SqliteConnection(ConnectionString))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT task_id, time FROM SpiderLog WHERE done != '1' ORDER BY time DESC LIMIT 1", db);

                var query = selectCommand.ExecuteReader();
                if (query.Read())
                {
                    tuple = new(query.GetInt64(0), query.GetInt64(1));
                }

                db.Close();
            }

            return tuple;
        }

        /// <summary>
        /// 获取一个图片地址
        /// </summary>
        /// <returns></returns>
        public static string GetOneImagePath()
        {
            string imagePath = "";
            using (SqliteConnection db =
            new SqliteConnection(ConnectionString))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT imagepath from VideoInfo WHERE imagepath != '' limit 1", db);

                SqliteDataReader query = selectCommand.ExecuteReader();
                while (query.Read())
                {
                    imagePath = query.GetString(0);
                }
                db.Close();
            }

            return imagePath;
        }


        /// <summary>
        /// 获取一张演员头像的地址
        /// </summary>
        /// <returns></returns>
        public static string GetOneActorProfilePath()
        {
            string imagePath = "";
            using (SqliteConnection db =
                   new SqliteConnection(ConnectionString))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT prifile_path from ActorInfo WHERE prifile_path != '' limit 1", db);

                SqliteDataReader query = selectCommand.ExecuteReader();
                while (query.Read())
                {
                    imagePath = query.GetString(0);
                }
                db.Close();
            }

            return imagePath;
        }

        /// <summary>
        /// 查询当前是否存入该演员
        /// </summary>
        /// <param Name="spiderSource"></param>
        /// <returns></returns>
        public static long GetActorIdByName(string actor_name)
        {
            long actor_id = -1;

            using (SqliteConnection db =
            new SqliteConnection(ConnectionString))
            {
                db.Open();

                //查询Actor_ID
                SqliteCommand Command = new($"SELECT id FROM Actor_Names WHERE Name == '{actor_name}'", db);
                var tmp = Command.ExecuteScalar();

                if (tmp != null)
                    actor_id = (long)tmp;

                db.Close();
            }

            return actor_id;
        }

        /// <summary>
        /// 获取可用搜刮任务的一个name
        /// </summary>
        /// <param Name="spiderSource"></param>
        /// <returns></returns>
        public static string GetOneSpiderTask(Spider.Manager.SpiderSource spiderSource)
        {
            string name = string.Empty;

            using (SqliteConnection db =
            new SqliteConnection(ConnectionString))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT Name from SpiderTask WHERE Done == 0 AND " +
                        $"(libre != 'doing' AND bus != 'doing' AND Jav321 != 'doing' AND Avmoo != 'doing' AND Avsox != 'doing' AND fc != 'doing' AND db != 'doing' AND fc != 'doing') AND " +
                        $"{spiderSource.Name} == 'ready' LIMIT 1", db);

                SqliteDataReader query = selectCommand.ExecuteReader();
                while (query.Read())
                {
                    name = query.GetString(0);
                }
                db.Close();
            }

            return name;
        }

        /// <summary>
        /// 获取该番号搜刮未完成且该搜刮源的状态为ready的数量
        /// </summary>
        /// <param Name="spiderSource"></param>
        /// <returns></returns>
        public static int GetWaitSpiderTaskCount(Spider.Manager.SpiderSource spiderSource)
        {
            int count = 0;

            using (SqliteConnection db =
            new SqliteConnection(ConnectionString))
            {
                db.Open();


                //TODO 分类讨论，考虑FC2的搜刮源
                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT count(Name) from SpiderTask WHERE Done == 0 AND {spiderSource.Name} == 'ready'", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                query.Read();
                count = query.GetInt32(0);
                query.Close();
                db.Close();
            }

            return count;
        }

        /// <summary>
        /// 是否所有的搜刮源都尝试搜刮了name
        /// </summary>
        /// <param Name="spiderSource"></param>
        /// <returns></returns>
        public static bool IsAllSpiderSourceAttempt(string QueryName)
        {
            bool isDone = false;

            using (SqliteConnection db =
            new SqliteConnection(ConnectionString))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT Name FROM SpiderTask WHERE Name == '{QueryName}' AND " +
                    $"libre == 'done' AND bus == 'done' AND Jav321 == 'done' AND Avmoo == 'done' AND Avsox == 'done' AND fc == 'done' AND db == 'done' " +
                    $"AND done == 0", db);

                string name = (string)selectCommand.ExecuteScalar();

                if (!string.IsNullOrEmpty(name))
                    isDone = true;

                db.Close();
            }

            return isDone;
        }

        private static string GetOrderStr(string orderBy, bool isDesc)
        {
            string orderStr = string.Empty;
            if (!string.IsNullOrEmpty(orderBy))
            {
                if (orderBy == "random")
                {
                    orderStr = " ORDER BY RANDOM() ";
                }
                else
                {
                    if (isDesc)
                    {
                        orderStr = $" ORDER BY {orderBy} DESC";
                    }
                    else
                    {
                        orderStr = $" ORDER BY {orderBy}";
                    }
                }
            }

            return orderStr;
        }

        private static string GetVideoInfoFilterStr(List<string> filterConditionList = null, string filterKeywords = null, Dictionary<string, string> rangesDicts = null)
        {
            string filterStr = string.Empty;
            if (filterConditionList != null || rangesDicts != null)
            {
                string filterStr_tmp = string.Empty;
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
                                if (filterKeywords == "")
                                {
                                    filterList.Add($"(VideoInfo.{item} == '{filterKeywords}')");
                                }
                                else
                                {
                                    filterList.Add($"(VideoInfo.{item} LIKE '%{filterKeywords}%')");
                                }
                                break;
                        }
                    }

                    filterStr_tmp = string.Join(" OR ", filterList);
                }

                string filterStr_tmp2 = string.Empty;
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

                    filterStr_tmp2 = string.Join(" AND ", ranges);
                }

                if (!string.IsNullOrEmpty(filterStr_tmp) && !string.IsNullOrEmpty(filterStr_tmp2))
                    filterStr = $" WHERE ({filterStr_tmp}) AND ({filterStr_tmp2})";
                else if (!string.IsNullOrEmpty(filterStr_tmp2))
                    filterStr = $" WHERE {filterStr_tmp2}";
                else if (!string.IsNullOrEmpty(filterStr_tmp))
                    filterStr = $" WHERE {filterStr_tmp}";
            }

            return filterStr;
        }

        /// <summary>
        /// 加载已存在的videoInfo数据
        /// </summary>
        /// <param Name="limit"></param>
        /// <returns></returns>
        public static async Task<List<VideoInfo>> LoadVideoInfo(int limit = 1, int offset = 0, string orderBy = null, bool isDesc = false, List<string> filterConditionList = null, string filterKeywords = null, Dictionary<string, string> rangesDicts = null, bool isFuzzyQueryActor = true)
        {
            List<VideoInfo> data = new List<VideoInfo>();

            using (SqliteConnection db =
               new SqliteConnection(ConnectionString))
            {
                db.Open();

                //排序
                string orderStr = GetOrderStr(orderBy, isDesc);

                //筛选
                string filterStr = GetVideoInfoFilterStr(filterConditionList, filterKeywords, rangesDicts);
                SqliteCommand selectCommand;

                //多表查询
                if (rangesDicts != null && rangesDicts.ContainsKey("Type"))
                {
                    selectCommand = new SqliteCommand($"SELECT VideoInfo.* FROM VideoInfo LEFT JOIN Is_Wm ON VideoInfo.truename = Is_Wm.truename LEFT JOIN ProducerInfo ON VideoInfo.producer = ProducerInfo.Name{filterStr}{orderStr} LIMIT {limit} offset {offset}", db);
                }
                else if (rangesDicts == null && filterConditionList?.Count == 1 && filterConditionList.FirstOrDefault() == "actor" && !isFuzzyQueryActor)
                {
                    selectCommand = new SqliteCommand
                        ($"SELECT VideoInfo.* FROM VideoInfo LEFT JOIN Actor_Video ON VideoInfo.truename = Actor_Video.video_name LEFT JOIN Actor_Names ON Actor_Video.actor_id = Actor_Names.id WHERE Actor_Names.name == '{filterKeywords}'{orderStr} LIMIT {limit} offset {offset}", db);
                }
                //普通查询
                else
                {
                    selectCommand = new SqliteCommand
                        ($"SELECT * from VideoInfo{filterStr}{orderStr} LIMIT {limit} offset {offset}", db);
                }

                SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

                while (query.Read())
                {
                    data.Add(ConvertQueryToVideoInfo(query));
                }

                db.Close();
            }

            return data;
        }

        public static async Task<List<ActorInfo>> LoadActorInfo(int limit = 1, int offset = 0, Dictionary<string, bool> orderByList = null, List<string> filterList = null)
        {
            List<ActorInfo> data = new();

            using (SqliteConnection db =
               new SqliteConnection(ConnectionString))
            {
                db.Open();

                string orderStr = string.Empty;
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

                string filterStr = string.Empty;
                if (filterList != null)
                {
                    filterStr = $" WHERE {string.Join(",", filterList)}";
                }

                SqliteCommand Command = new SqliteCommand
                    ($"SELECT ActorInfo.*,bwh.bust,bwh.waist,bwh.hips,COUNT(id) FROM ActorInfo LEFT JOIN Actor_Video ON Actor_Video.actor_id = ActorInfo.id LEFT JOIN bwh ON ActorInfo.bwh = bwh.bwh{filterStr} GROUP BY id{orderStr} LIMIT {limit} offset {offset}", db);

                SqliteDataReader query = await Command.ExecuteReaderAsync();

                while (query.Read())
                {
                    data.Add(ConvertQueryToActorInfo(query));
                }

                db.Close();
            }

            return data;
        }

        public static async Task<List<ActorInfo>> LoadActorInfoByVideoName(string videoName)
        {
            List<ActorInfo> data = new();

            using (SqliteConnection db =
               new SqliteConnection(ConnectionString))
            {
                db.Open();

                SqliteCommand Command = new SqliteCommand
                    ($"SELECT actors.*,bwh.bust,bwh.waist,bwh.hips, COUNT(id)  FROM ( SELECT ActorInfo.* FROM ActorInfo, VideoInfo, Actor_Video WHERE VideoInfo.truename == '{videoName}' AND VideoInfo.truename == Actor_Video.video_name AND Actor_Video.actor_id == ActorInfo.id ) AS actors LEFT JOIN Actor_Video ON Actor_Video.actor_id = actors.id LEFT JOIN bwh ON actors.bwh = bwh.bwh GROUP BY id", db);

                SqliteDataReader query = await Command.ExecuteReaderAsync();

                while (query.Read())
                {
                    data.Add(ConvertQueryToActorInfo(query));
                }

                db.Close();
            }

            return data;
        }



        /// <summary>
        /// 通过PickCode以及TimeEdit判断文件夹是否存在且未更新。
        /// 针对的是文件夹
        /// 如果条件为真，则返回pid（如果是文件夹，则为上一级目录的cid；如果是文件则为空）
        /// </summary>
        /// <param Name="limit"></param>
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
        /// 获取演员出演的视频信息（By label）
        /// </summary>
        /// <param Name="actorName"></param>
        /// <returns></returns>
        public static List<VideoInfo> loadVideoInfoByLabel(string label)
        {
            List<VideoInfo> data = new List<VideoInfo>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
                new SqliteConnection(ConnectionString))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand();

                if (label == "")
                {
                    selectCommand = new SqliteCommand
                        ($"SELECT * from VideoInfo WHERE category == ''", db);
                }
                else
                {
                    selectCommand = new SqliteCommand
                        ($"SELECT * from VideoInfo WHERE category LIKE '%{label}%'", db);
                }


                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    data.Add(ConvertQueryToVideoInfo(query));
                }

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 获取演员出演的视频信息（By ActorName）
        /// </summary>
        /// <param Name="actorName"></param>
        /// <returns></returns>
        public static List<VideoInfo> loadVideoInfoByActor(string actorName)
        {
            List<VideoInfo> data = new List<VideoInfo>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
                new SqliteConnection(ConnectionString))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand();

                if (actorName == "")
                {
                    selectCommand = new SqliteCommand
                        ($"SELECT * from VideoInfo WHERE actor == ''", db);
                }
                else
                {
                    selectCommand = new SqliteCommand
                        ($"SELECT * from VideoInfo WHERE actor LIKE '%{actorName}%'", db);
                }


                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    data.Add(ConvertQueryToVideoInfo(query));
                }

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 获取VideoInfo（By Name）
        /// </summary>
        /// <param Name="actorName"></param>
        /// <returns></returns>
        public static List<VideoInfo> loadVideoInfoByName(string name)
        {
            string[] splitList = name.Split(new char[] { '-', '_' });
            string leftName = splitList[0];

            string rightNumber = "";
            if (splitList.Length != 1)
            {
                rightNumber = splitList[1];
            }

            List<VideoInfo> data = new List<VideoInfo>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
                new SqliteConnection(ConnectionString))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand();

                selectCommand = new SqliteCommand
                    ($"SELECT * from VideoInfo WHERE truename LIKE '%{leftName}%{rightNumber}%'", db);


                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    data.Add(ConvertQueryToVideoInfo(query));
                }

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 获取演员出演的视频信息（By label）
        /// </summary>
        /// <param Name="actorName"></param>
        /// <returns></returns>
        public static List<VideoInfo> loadVideoInfoBySomeType(string type, string label, int limit)
        {
            List<VideoInfo> data = new List<VideoInfo>();

            using (SqliteConnection db =
                new SqliteConnection(ConnectionString))
            {
                db.Open();

                SqliteCommand selectCommand;
                if (label == "")
                {
                    selectCommand = new SqliteCommand($"SELECT * from VideoInfo WHERE {type} == '' LIMIT {limit}", db);
                }
                else
                {
                    selectCommand = new SqliteCommand($"SELECT * from VideoInfo WHERE {type} LIKE '%{label}%' LIMIT {limit}", db);
                }

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    data.Add(ConvertQueryToVideoInfo(query));
                }

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 查询PickCode文件对应的字幕文件（首先匹配文件名，其次匹配上一级文件夹的名称）
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> FindSubFile(string file_pickCode)
        {
            Dictionary<string, string> subDicts = new();
            using (SqliteConnection db =
                new SqliteConnection(ConnectionString))
            {
                db.Open();


                //查询文件名称
                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT cid,n FROM FilesInfo WHERE pc == '{file_pickCode}'", db);

                SqliteDataReader query = selectCommand.ExecuteReader();
                query.Read();

                //查询无果，几乎不可能发生
                if (!query.HasRows)
                {
                    query.Close();
                    db.Close();
                    return subDicts;
                }

                string folderCid = query["cid"] as string;
                string fileName = query["n"] as string;
                query.Close();

                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                var nameIsNumber = TryParse(fileNameWithoutExtension, out _);

                //1.首先查询同名字幕文件(纯数字则跳过，长度小于10也跳过)
                if (!nameIsNumber && fileNameWithoutExtension.Length > 10)
                {
                    selectCommand = new SqliteCommand
                        ($"SELECT pc, n FROM FilesInfo WHERE (ico == 'srt' OR ico == 'ass' OR ico == 'ssa') AND n LIKE '%{fileNameWithoutExtension}%'", db);

                    query = selectCommand.ExecuteReader();
                    while (query.Read())
                    {
                        subDicts.Add(query["pc"] as string, query["n"] as string);
                    }
                }

                //2.没有同名字幕文件，根据番号特点匹配（字母+数字），这里的方法正常的视频不通用
                if (subDicts.Count == 0 && !nameIsNumber)
                {
                    string truename = FileMatch.MatchName(fileName);
                    if (string.IsNullOrEmpty(folderCid) || string.IsNullOrEmpty(truename))
                    {
                        db.Close();
                        return subDicts;
                    }

                    var tuple = FileMatch.SplitLeftAndRightFromCid(truename);
                    string leftName = tuple.Item1.Replace("FC2", "FC");
                    string rightNumber = tuple.Item2;

                    //通过truename查询字幕文件
                    selectCommand = new SqliteCommand
                        ($"SELECT pc,n FROM FilesInfo WHERE (ico == 'srt' OR ico == 'ass' OR ico == 'ssa') AND n LIKE '%{leftName}%{rightNumber}%'", db);

                    query = selectCommand.ExecuteReader();
                    while (query.Read())
                    {
                        subDicts.Add(query["pc"] as string, query["n"] as string);
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
                        tmpDict.Add(query["pc"] as string, query["n"] as string);
                    }

                    if (!nameIsNumber)
                    {
                        subDicts = tmpDict;
                    }
                    else
                    {
                        foreach (var item in tmpDict)
                        {
                            Match match = Regex.Match(item.Value, $"(^|[^0-9]){fileNameWithoutExtension}($|[^0-9])");

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
        /// 获取FileInfo（By TrueName）
        /// </summary>
        /// <param Name="actorName"></param>
        /// <returns></returns>
        public static List<Datum> loadFileInfoByTruename(string truename)
        {
            var tuple = FileMatch.SplitLeftAndRightFromCid(truename);

            string leftName = tuple.Item1.Replace("FC2", "FC");
            string rightNumber = tuple.Item2;

            if (rightNumber.StartsWith("0"))
            {
                var match_result = Regex.Match(rightNumber, @"0*(\d+)");

                if (match_result.Success)
                    rightNumber = match_result.Groups[1].Value;
            }

            List<Datum> data = new List<Datum>();
            
            using (SqliteConnection db =
                new SqliteConnection(ConnectionString))
            { 
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand();

                //vdi = 0，即视频转码未成功，无法在线观看
                //selectCommand = new SqliteCommand ($"SELECT * from FilesInfo WHERE uid != 0 AND vdi != 0 AND n LIKE '%{leftName}%{rightNumber}%'", db);
                selectCommand = new SqliteCommand($"SELECT * from FilesInfo WHERE uid != 0 AND iv = 1 AND n LIKE '%{leftName}%{rightNumber}%'", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                List<Datum> tmpList = new();
                while (query.Read())
                {
                    tmpList.Add(TryCovertQueryToDatum(query));
                }

                //进一步筛选，通过右侧数字
                // '%xxx%57%' 会选出 057、157、257之类的
                foreach (var datum in tmpList)
                {
                    if (leftName == "FC")
                    {
                        var match_result = Regex.Match(datum.n, @"(FC2?)[-_PV]*[-_]?0*(\d+)", RegexOptions.IgnoreCase);
                        if (match_result.Success && match_result.Groups[2].Value == rightNumber)
                            data.Add(datum);
                    }
                    else
                    {
                        var match_result = Regex.Match(datum.n, @$"({leftName})[-_]?0*(\d+)", RegexOptions.IgnoreCase);
                        if (match_result.Success && match_result.Groups[2].Value == rightNumber)
                            data.Add(datum);
                    }
                }

                db.Close();
            }

            //进一步筛选
            data = data.Where(x => Regex.Match(x.n, @$"[^a-z]{leftName}[^a-z]|^{leftName}[^a-z]", RegexOptions.IgnoreCase).Success).ToList();

            return data;
        }

        /// <summary>
        /// 通过pid获取文件夹列表，PID的下一级目录
        /// </summary>
        /// <param Name="pid"></param>
        /// <returns></returns>
        public static List<Datum> GetFolderListByPid(long pid, int limit = -1)
        {
            var data = new List<Datum>();

            using var db =
                new SqliteConnection(ConnectionString);

            db.Open();

            var selectCommand = new SqliteCommand
                ($"SELECT * FROM FilesInfo WHERE pid == {pid} LIMIT {limit}", db);

            var query = selectCommand.ExecuteReader();

            while (query.Read())
            {
                data.Add(TryCovertQueryToDatum(query));
            }

            db.Close();

            return data;
        }

        /// <summary>
        /// 通过cid获取文件和文件夹列表，CID的下一级目录
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static List<Datum> GetListByCid(long? cid, int limit = -1)
        {
            var data = new List<Datum>();
            
            using var db =
                new SqliteConnection(ConnectionString);

            db.Open();

            var selectCommand = new SqliteCommand
                ($"SELECT * FROM FilesInfo WHERE cid == {cid} and uid != 0 or pid == {cid} LIMIT {limit}", db);

            var query = selectCommand.ExecuteReader();

            while (query.Read())
            {
                data.Add(TryCovertQueryToDatum(query));
            }

            db.Close();

            return data;
        }

        /// <summary>
        /// 通过pickCode和ua获取下载记录
        /// </summary>
        /// <param Name="cid"></param>
        /// <returns></returns>
        public static DownInfo GetDownHistoryByPcAndUa(string picoCode, string ua)
        {
            DownInfo data = null;

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
               new SqliteConnection(ConnectionString))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    //($"SELECT n,cid,fid,vdi,pid FROM {TABLENAME} WHERE cid == {cid} or pid == {cid}", db);
                    ($"SELECT * FROM DownHistory WHERE file_pickcode == '{picoCode}' and ua == '{ua}' LIMIT 1", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    data = tryCovertQueryToDownHistory(query);
                }

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 获取当前文件列表下所有的文件
        /// </summary>
        /// <param Name="data"></param>
        /// <returns></returns>
        public static List<Datum> GetAllFilesInFolderList(List<Datum> dataList)
        {
            List<Datum> newData = new();

            foreach (var currentFile in dataList)
            {
                //文件夹
                if (currentFile.fid == default)
                {
                    // 获取当前文件夹下所有的文件夹和文件
                    var newDataList = GetListByCid(currentFile.cid);

                    var fileInFolderList = GetAllFilesInFolderList(newDataList);

                    newData.AddRange(fileInFolderList);
                }
            }

            newData.AddRange(dataList);

            return newData;
        }

        public static async Task<List<Datum>> GetAllFilesInFolderListAsync(List<Datum> dataList)
        {
            return await Task.Run(() => GetAllFilesInFolderList(dataList));
        }

        /// <summary>
        ///  遍历cid下所有的文件（文件和文件夹）
        /// </summary>
        /// <param Name="cid"></param>
        /// <param Name="AllDatumList"></param>
        /// <returns></returns>
        public static List<Datum> GetAllFilesTraverse(long? cid, List<Datum> AllDatumList = null)
        {
            if (AllDatumList == null)
            {
                AllDatumList = new List<Datum>();
            }

            List<Datum> datumList = GetListByCid(cid);
            AllDatumList.AddRange(datumList);

            foreach (Datum datum in datumList)
            {
                //文件夹
                if (datum.fid == default)
                {
                    AllDatumList = GetAllFilesTraverse(datum.cid, AllDatumList);
                }
            }

            return AllDatumList;
        }

        /// <summary>
        /// 获取标记为“稍后观看”的部分信息（*）
        /// </summary>
        /// <param Name="actorName"></param>
        /// <returns></returns>
        public static async Task<List<VideoInfo>> GetNameAndImageFromLookLater(int count = 10)
        {
            List<VideoInfo> data = new List<VideoInfo>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            await using SqliteConnection db =
                new SqliteConnection(ConnectionString);

            db.Open();

            SqliteCommand selectCommand = new SqliteCommand();

            selectCommand = new SqliteCommand
                ($"SELECT * FROM VideoInfo WHERE look_later != 0 ORDER BY look_later DESC LIMIT {count}", db);


            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

            while (query.Read())
            {
                data.Add(ConvertQueryToVideoInfo(query));
            }

            db.Close();

            return data;
        }

        /// <summary>
        /// 获取标记为“稍后观看”的部分信息（*）
        /// </summary>
        /// <param Name="actorName"></param>
        /// <returns></returns>
        public static async Task<List<VideoInfo>> GetNameAndImageFromLike(int count = 10)
        {
            List<VideoInfo> data = new List<VideoInfo>();

            using SqliteConnection db =
                new SqliteConnection(ConnectionString);

            db.Open();

            SqliteCommand selectCommand = new SqliteCommand();

            selectCommand = new SqliteCommand
                ($"SELECT * FROM VideoInfo WHERE is_like != 0 LIMIT {count}", db);

            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

            while (query.Read())
            {
                data.Add(ConvertQueryToVideoInfo(query));
            }

            db.Close();

            return data;
        }

        /// <summary>
        /// 随机获取信息（*）RANDOM
        /// </summary>
        /// <param Name="actorName"></param>
        /// <returns></returns>
        public static async Task<List<VideoInfo>> GetNameAndImageRandom(int limit = 10)
        {
            List<VideoInfo> data = new List<VideoInfo>();

            await using SqliteConnection db =
                new SqliteConnection(ConnectionString);

            db.Open();

            SqliteCommand selectCommand = new SqliteCommand();

            selectCommand = new SqliteCommand
                ($"SELECT * FROM VideoInfo ORDER BY RANDOM() LIMIT {limit}", db);

            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

            while (query.Read())
            {
                data.Add(ConvertQueryToVideoInfo(query));
            }

            db.Close();

            return data;
        }

        /// <summary>
        /// 获取最近视频的部分信息（*）
        /// </summary>
        /// <param Name="actorName"></param>
        /// <returns></returns>
        public static async Task<List<VideoInfo>> GetNameAndImageRecent(int count = 10)
        {
            List<VideoInfo> data = new List<VideoInfo>();

            await using SqliteConnection db =
                new SqliteConnection(ConnectionString);

            db.Open();

            SqliteCommand selectCommand = new SqliteCommand();

            selectCommand = new SqliteCommand
                ($"SELECT * FROM VideoInfo ORDER BY addtime DESC LIMIT {count}", db);


            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

            while (query.Read())
            {
                data.Add(ConvertQueryToVideoInfo(query));
            }
            db.Close();

            return data;
        }

        /// <summary>
        /// 获取该cid文件夹上一层目录的信息
        /// </summary>
        /// <param Name="cid"></param>
        /// <returns></returns>
        public static Datum GetUpperLevelFolderCid(long cid)
        {
            var datum = new Datum();

            using var db =
                new SqliteConnection(ConnectionString);

            db.Open();

            var selectCommand = new SqliteCommand
                ($"SELECT * FROM FilesInfo WHERE cid = {cid} AND fid == '' LIMIT 1", db);

            var query = selectCommand.ExecuteReader();

            while (query.Read())
            {
                datum = TryCovertQueryToDatum(query);
            }
            db.Close();

            return datum;
        }

        /// <summary>
        /// 通过文件夹的Cid回溯根目录，不包括当前文件夹
        /// </summary>
        /// <param name="folderCid"></param>
        /// <returns></returns>
        public static List<Datum> GetRootByCid(long folderCid)
        {
            List<Datum> folderToRootList = new();

            Datum upperLevelFolderInfo = new() { pid = folderCid };

            if (upperLevelFolderInfo.pid == null) return null;

            const int maxDepth = 30;

            ////获取当前目录信息，pid即为上一级目录信息
            //Datum currentFolderInfo = getUpperLevelFolderCid(folderCid);

            for (int i = 0; i < maxDepth; i++)
            {
                upperLevelFolderInfo = GetUpperLevelFolderCid((long)upperLevelFolderInfo.pid);

                if (upperLevelFolderInfo == null) break;

                folderToRootList.Add(upperLevelFolderInfo);

                if (upperLevelFolderInfo.pid == 0)
                {
                    folderToRootList.Add(new Datum() { cid =0, n = "根目录" });
                    break;
                }
            }

            folderToRootList.Reverse();

            return folderToRootList;
        }

        #endregion

        #region 格式转换

        /// <summary>
        /// 转换数据库查询结果到VideoInfo格式
        /// </summary>
        /// <param Name="query"></param>
        /// <returns></returns>
        private static VideoInfo ConvertQueryToVideoInfo(SqliteDataReader query)
        {
            VideoInfo dataInfo = new VideoInfo();
            dataInfo.truename = query["truename"] as string;
            dataInfo.title = query["title"] as string;
            dataInfo.releasetime = query["releasetime"] as string;
            dataInfo.lengthtime = query["lengthtime"] as string;
            dataInfo.director = query["director"] as string;
            dataInfo.producer = query["producer"] as string;
            dataInfo.publisher = query["publisher"] as string;
            dataInfo.series = query["series"] as string;
            dataInfo.category = query["category"] as string;
            dataInfo.actor = query["actor"] as string;
            dataInfo.imageurl = query["imageurl"] as string;
            dataInfo.sampleImageList = query["sampleImageList"] as string;
            dataInfo.imagepath = query["imagepath"] as string;
            dataInfo.busurl = query["busurl"] as string;
            dataInfo.look_later = Convert.ToInt32(query["look_later"]);
            dataInfo.score = Convert.ToSingle(query["score"]);
            dataInfo.is_like = Convert.ToInt32(query["is_like"]);
            dataInfo.addtime = Convert.ToInt64(query["addtime"]);

            return dataInfo;
        }

        /// <summary>
        /// 转换数据库查询结果到ActorInfo格式
        /// </summary>
        /// <param Name="query"></param>
        /// <returns></returns>
        private static ActorInfo ConvertQueryToActorInfo(SqliteDataReader reader)
        {
            ActorInfo dataInfo = new()
            {
                id = Convert.ToInt32(reader["id"]),
                name = reader["Name"] as string,
                is_woman = Convert.ToInt32(reader["is_woman"]),
                birthday = reader["birthday"] as string,
                bwh = reader["bwh"] as string,
                height = reader["height"] is not long ? 0 : Convert.ToInt32(reader["height"]),
                works_count = Convert.ToInt32(reader["works_count"]),
                work_time = reader["work_time"] as string,
                prifile_path = reader["prifile_path"] as string,
                blog_url = reader["blog_url"] as string,
                info_url = reader["info_url"] as string,
                is_like = reader["is_like"] is not long ? 0 : Convert.ToInt32(reader["is_like"]),
                addtime = Convert.ToInt64(reader["addtime"]),
                bust = reader["bust"] is not long ? 0 : Convert.ToInt32(reader["bust"]),
                waist = reader["waist"] is not long ? 0 : Convert.ToInt32(reader["waist"]),
                hips = reader["hips"] is not long ? 0 : Convert.ToInt32(reader["hips"]),
                video_count = Convert.ToInt32(reader["COUNT(id)"])
            };

            return dataInfo;
        }

        /// <summary>
        /// 转换数据库查询结果到Datum格式
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Datum TryCovertQueryToDatum(SqliteDataReader reader)
        {
            var dataInfo = new Datum();

            if (reader.FieldCount == 0) return dataInfo;

            dataInfo.fid = reader.GetNullableValue<long?>("fid");
            dataInfo.pid = (reader["pid"] as string).ToNullableLong();
            dataInfo.cid = Convert.ToInt64(reader["cid"]);
            dataInfo.uid = Convert.ToInt64(reader["uid"]);

            dataInfo.aid = Convert.ToInt32(reader["aid"]);
            dataInfo.n = reader["n"] as string;
            dataInfo.s = Convert.ToInt64(reader["s"]);
            dataInfo.sta = Convert.ToInt32(reader["sta"]);
            dataInfo.pt = reader["pt"] as string;
            dataInfo.pc = reader["pc"] as string;
            dataInfo.p = Convert.ToInt32(reader["p"]);
            dataInfo.m = Convert.ToInt32(reader["m"]);
            dataInfo.t = reader["t"] as string;
            dataInfo.te = Convert.ToInt32(reader["te"]);
            dataInfo.tp = Convert.ToInt32(reader["tp"]);
            dataInfo.d = Convert.ToInt32(reader["d"]);
            dataInfo.c = Convert.ToInt32(reader["c"]);
            dataInfo.sh = Convert.ToInt32(reader["sh"]);
            dataInfo.e = reader["e"] as string;
            dataInfo.ico = reader["ico"] as string;
            dataInfo.sha = reader["sha"] as string;
            dataInfo.fdes = reader["fdes"] as string;
            dataInfo.q = Convert.ToInt32(reader["q"]);
            dataInfo.hdf = Convert.ToInt32(reader["hdf"]);
            dataInfo.u = reader["u"] as string;
            dataInfo.iv = Convert.ToInt32(reader["iv"]);
            dataInfo.current_time = Convert.ToInt32(reader["current_time"]);
            dataInfo.played_end = Convert.ToInt32(reader["played_end"]);
            dataInfo.last_time = reader["last_time"] as string;
            dataInfo.vdi = Convert.ToInt32(reader["vdi"]);
            dataInfo.play_long = Convert.ToSingle(reader["play_long"]);

            return dataInfo;

        }

        /// <summary>
        /// 添加数据库查询结果到DownInfo格式
        /// </summary>
        /// <param Name="query"></param>
        /// <returns></returns>
        private static DownInfo tryCovertQueryToDownHistory(SqliteDataReader query)
        {
            DownInfo downInfo = new();

            if (query.FieldCount == 0) return downInfo;

            downInfo.pickCode = query["file_pickcode"] as string;
            downInfo.fileName = query["file_name"] as string;
            downInfo.trueUrl = query["true_url"] as string;
            downInfo.ua = query["ua"] as string;
            downInfo.addTime = Convert.ToInt64(query["add_time"]);

            return downInfo;
        }

        /// <summary>
        /// 添加数据库查询结果到FailInfo格式
        /// </summary>
        /// <param Name="query"></param>
        /// <returns></returns>
        private static FailInfo tryCovertQueryToFailInfo(SqliteDataReader query)
        {
            FailInfo info = new();

            if (query.FieldCount == 0) return info;

            info.datum = TryCovertQueryToDatum(query);

            info.pc = info.datum.pc;
            info.is_like = Convert.ToInt32(query["is_like"]);
            info.score = Convert.ToSingle(query["score"]);
            info.look_later = Convert.ToInt64(query["look_later"]);
            info.image_path = query["image_path"] as string;


            return info;
        }

        #endregion
    }

}
