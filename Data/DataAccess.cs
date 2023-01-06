using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Media;
using Windows.Storage;

namespace Data
{
    public static class DataAccess
    {
        private const string DBNAME = "115_uwp.db";
        public static string dbpath
        {
            get { return NewDBPath(AppSettings.DataAccess_SavePath); }
        }

        /// <summary>
        /// 数据库表初始化
        /// </summary>
        public async static Task InitializeDatabase(string DataAccess_SavePath = null)
        {
            if (DataAccess_SavePath == null)
            {
                DataAccess_SavePath = AppSettings.DataAccess_SavePath;
            }

            tryCreateDBFile(DataAccess_SavePath);

            string newDBPath = Path.Combine(DataAccess_SavePath, DBNAME);

            //获取文件夹，没有则创建
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(AppSettings.DataAccess_SavePath);
            //获取数据库文件，没有则创建
            await folder.CreateFileAsync(DBNAME, CreationCollisionOption.OpenIfExists);

            using (SqliteConnection db =
               new SqliteConnection($"Filename={newDBPath}"))
            {
                db.Open();

                //文件信息
                string tableCommand = "CREATE TABLE IF NOT " +
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

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);
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
                InittializeProducerInfo(createTable);

                //番号匹配
                createTable.CommandText = "CREATE TABLE IF NOT " +
                    $"EXISTS FileToInfo ( " +
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
                      "PRIMARY KEY('file_pickcode')" +
                      ") ";
                createTable.ExecuteNonQuery();

                //演员信息
                createTable.CommandText = "CREATE TABLE IF NOT " +
                    $"EXISTS ActorInfo ( " +
                        "id INTEGER NOT NULL,"+
                        "name TEXT NOT NULL,"+
                        "is_woman integer,"+
                        "birthday text,"+
                        "bwh text,"+
                        "height integer,"+
                        "works_count integer,"+
                        "work_time text," +
                        "prifile_path text," +
                        "blog_url text," +
                        "is_like integer," +
                        "addtime integer," +
                        "PRIMARY KEY (id)" +
                      ") ";
                createTable.ExecuteNonQuery();

                //演员别名
                createTable.CommandText = "CREATE TABLE IF NOT " +
                    $"EXISTS Actor_Names ( " +
                    "id INTEGER NOT NULL," +
                    "name TEXT NOT NULL," +
                    "PRIMARY KEY('id','name')" +
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
                      "name text," +
                      "bus text," +
                      "jav321 text," +
                      "avmoo text," +
                      "avsox text," +
                      "libre text," +
                      "fc text," +
                      "db text," +
                      "done integer," +
                      "tadk_id integer," +
                      "PRIMARY KEY('name')" +
                      ") ";
                createTable.ExecuteNonQuery();
            }
        }

        public static async Task UpdateDatabaseFrom14()
        {
            //更新演员数据
            Dictionary<string, List<string>> ActorsInfoDict = new();

            //加载全部数据
            List<VideoInfo> VideoInfoList = await DataAccess.LoadVideoInfo(-1);
            foreach (var VideoInfo in VideoInfoList)
            {
                string actor_str = VideoInfo.actor;

                var actor_list = actor_str.Split(",");
                foreach (var actor in actor_list)
                {
                    //当前名称不存在
                    if (!ActorsInfoDict.ContainsKey(actor))
                    {
                        ActorsInfoDict.Add(actor, new List<string>());
                    }
                    ActorsInfoDict[actor].Add(VideoInfo.truename);
                }
            }

            foreach (var item in ActorsInfoDict)
            {
                AddActorInfo(item.Key, item.Value);
            }
        }

        public static void AddActorInfo(string actor_name,List<string> video_nameList)
        {
            string singleActorName;
            string[] otherNames = null;

            if(actor_name != null)
            {
                //针对演员的别名，添加到Actor_Names
                var match_result = Regex.Match(actor_name, "(.*)[（（](.*)[)）]");
                if (match_result.Success)
                {
                    singleActorName = match_result.Groups[1].Value;

                    otherNames = match_result.Groups[2].Value.Split("、");
                }
                else
                {
                    singleActorName = actor_name;
                }
            }
            else
            {
                singleActorName = string.Empty;
            }


            //插入演员信息（不存在时才插入）
            var actor_id = GetActorIdByName(singleActorName);
            //数据库中不存在该名称
            if (actor_id == -1)
            {
                ActorInfo actorInfo = new() { name=singleActorName };

                //检查演员图片是否存在
                string imagePath = Path.Combine(AppSettings.ActorInfo_SavePath, singleActorName, "face.jpg");
                if (File.Exists(imagePath))
                {
                    actorInfo.prifile_path = imagePath;
                }

                actor_id = InsertActorInfo(actorInfo);
            }

            //添加Actor_Names
            //主名称
            AddOrIgnoreActor_Names(actor_id, singleActorName);
            //别名
            if (otherNames != null)
            {
                foreach (var name in otherNames)
                {
                    AddOrIgnoreActor_Names(actor_id, name);
                }
            }

            //添加演员和作品的信息
            foreach (var video_name in video_nameList)
            {
                AddOrIgnoreActor_Video(actor_id, video_name);
            }
        }

        /// <summary>
        /// 初始化厂商信息
        /// </summary>
        /// <param name="db"></param>
        public static void InittializeProducerInfo(SqliteCommand createTable)
        {
            createTable.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='ProducerInfo';";

            var result = createTable.ExecuteScalar();
            //存在
            if (result != null)
            {
                return;
            }
            //不存在
            else
            {
                //创建
                createTable.CommandText = "CREATE TABLE IF NOT " +
                    $"EXISTS ProducerInfo ( " +
                        "name text," +
                        "is_wm is_wm," +
                        "PRIMARY KEY('name')" +
                        ") ";

                createTable.ExecuteNonQuery();

                string[] wm_producer = new string[] { "CATCHEYE", "東京熱", "スカイハイエンターテインメント", "HEYZO", "FC2-PPV", "カリビアンコム( Caribbeancom )", "天然むすめ( 10musume )", "一本道( 1pondo )", "ピンクパンチャー", "トラトラトラ", "スーパーモデルメディア", "キャットウォーク", "Gachinco", "スタジオテリヤキ", "フェアリー", "カリビアンコム", "ClimaxZipang", "パコパコママ", "Tokyo-Hot", "SkyHigh", "ファンタドリーム", "一本道", "天然むすめ" };

                //插入常见的
                foreach (var name in wm_producer)
                {
                    createTable.CommandText = "INSERT OR IGNORE INTO ProducerInfo VALUES (@name,1)";
                    createTable.Parameters.Clear();
                    createTable.Parameters.AddWithValue("@name",name);

                    createTable.ExecuteNonQuery();
                }


            }

        }

        public static string NewDBPath(string newPath)
        {
            return Path.Combine(newPath, DBNAME);
        }

        public async static void tryCreateDBFile(string TargetPath)
        {
            //获取文件夹
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(TargetPath);
            //获取数据库文件，没有则创建
            await folder.CreateFileAsync(DBNAME, CreationCollisionOption.OpenIfExists);
        }

        #region 删除
        /// <summary>
        /// 删除FilesInfo表中的所有数据
        /// </summary>
        public static void DeleteFilesInfoTable()
        {
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = $"delete from FilesInfo";

                insertCommand.ExecuteReader();

                db.Close();
            }
        }

        /// <summary>
        /// 删除SpiderLog中的所有数据
        /// </summary>
        public static void DeleteSpiderLogTable()
        {
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = $"DELETE FROM SpiderLog";

                insertCommand.ExecuteReader();

                db.Close();
            }
        }

        /// <summary>
        /// 删除SpiderTask中的所有数据
        /// </summary>
        public static void DeleteSpiderTaskTable()
        {
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = $"DELETE FROM SpiderTask";

                insertCommand.ExecuteReader();

                db.Close();
            }
        }

        /// <summary>
        /// 删除VideoInfo表中的一条记录
        /// </summary>
        public static void DeleteDataInVideoInfoTable(string truename)
        {
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = $"DELETE FROM VideoInfo WHERE truename == '{truename}'";

                insertCommand.ExecuteReader();

                db.Close();
            }
        }

        /// <summary>
        /// 删除FilesInfo表里文件夹下的所有文件和文件夹（最多两级）
        /// </summary>
        public static void DeleteDirectoryAndFiles_InFilesInfoTable(string cid)
        {
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = $"DELETE FROM FilesInfo WHERE cid == '{cid}' or pid = '{cid}'";

                insertCommand.ExecuteReader();

                db.Close();
            }
        }

        /// <summary>
        /// 删除FilesInfo表里文件夹下的所有文件和文件夹（遍历所有）
        /// </summary>
        /// <param name="cid"></param>
        public static void DeleteAllDirectroyAndFiles_InfilesInfoTabel(string cid)
        {
            var Files = GetAllFilesTraverse(cid);
            List<string> cidList = new();
            foreach (var file in Files)
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
        /// 添加115文件信息
        /// </summary>
        /// <param name="data"></param>
        public static async Task AddFilesInfoAsync(Datum data)
        {
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks

                List<string> keyList = new List<string>();
                foreach (var item in data.GetType().GetProperties())
                {
                    //fl为数组，应添加进新表中，一对多。目前暂不考虑，故跳过
                    if (item.Name == "fl") continue;

                    keyList.Add($"@{item.Name}");
                }

                //唯一值（pc）重复 则代替 （replace）
                insertCommand.CommandText = $"INSERT OR REPLACE INTO FilesInfo VALUES ({string.Join(",", keyList)});";

                foreach (var item in data.GetType().GetProperties())
                {
                    //fl为数组，应添加进新表中，一对多。目前暂不考虑，故跳过
                    if (item.Name == "fl") continue;

                    insertCommand.Parameters.AddWithValue("@" + item.Name, $"{item.GetValue(data)}");
                }

                await insertCommand.ExecuteNonQueryAsync();

                db.Close();
            }
        }

        /// <summary>
        /// 添加FileToInfo表
        /// </summary>
        /// <param name="pickCode"></param>
        /// <param name="truename"></param>
        public static void AddFileToInfo(string pickCode, string truename, bool issuccess = false, bool isReplace = false)
        {
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                //唯一值（pc）重复 则代替 （replace）
                string replaceStr = isReplace ? " OR REPLACE" : " OR IGNORE";
                insertCommand.CommandText = $"INSERT{replaceStr} INTO FileToInfo VALUES (@file_pickcode,@truename,@issuccess);";

                insertCommand.Parameters.AddWithValue("@file_pickcode", pickCode);

                if (truename == null)
                {
                    truename = string.Empty;
                }
                insertCommand.Parameters.AddWithValue("@truename", truename);
                insertCommand.Parameters.AddWithValue("@issuccess", issuccess);

                try
                {
                    insertCommand.ExecuteReader();
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"添加FileToInfo数据时发生错误:{e.Message}");
                }

                db.Close();
            }
        }

        /// <summary>
        /// 添加视频信息
        /// </summary>
        /// <param name="data"></param>
        public static void AddVideoInfo(VideoInfo data)
        {
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                Dictionary<string,string> dicts = new();
                foreach (var item in data.GetType().GetProperties())
                {
                    if (item.Name == "is_wm")
                        continue;

                    dicts.Add($"@{item.Name}", $"{item.GetValue(data)}");
                }

                //添加信息，如果已经存在则跳过
                insertCommand.CommandText = $"INSERT OR IGNORE INTO VideoInfo VALUES ({string.Join(",", dicts.Keys)});";

                foreach (var item in dicts)
                {
                    insertCommand.Parameters.AddWithValue(item.Key, item.Value);
                }
                insertCommand.ExecuteNonQuery();

                //var result = insertCommand.ExecuteReader();
                //Debug.WriteLine(result);
                //Debug.WriteLine($"添加信息{data.truename},{data.producer},{data.is_wm}");

                //添加演员信息
                AddActorInfo(data.actor, new() { data.truename });

                //添加是否步兵
                AddOrReplaceIs_Wm(data.truename,data.producer, data.is_wm);


                db.Close();
            }
        }

        /// <summary>
        /// 添加下载记录
        /// </summary>
        /// <param name="data"></param>
        public static void AddDownHistory(DownInfo data)
        {
            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                //添加信息，如果已经存在则替换
                insertCommand.CommandText = $"INSERT OR REPLACE INTO DownHistory VALUES (@file_pickcode,@file_name,@true_url,@ua,@add_time);";

                insertCommand.Parameters.AddWithValue("@file_pickcode", data.pickCode);
                insertCommand.Parameters.AddWithValue("@file_name", data.fileName);
                insertCommand.Parameters.AddWithValue("@true_url", data.trueUrl);
                insertCommand.Parameters.AddWithValue("@ua", data.ua);
                insertCommand.Parameters.AddWithValue("@add_time", data.addTime);
                insertCommand.ExecuteReader();

                db.Close();
            }
        }

        /// <summary>
        /// 插入或替换是否Is_Wm的表数据
        /// </summary>
        /// <param name="truename"></param>
        /// <param name="is_wm"></param>
        public static void AddOrReplaceIs_Wm(string truename,string producer, int is_wm)
        {
            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                //添加信息，如果已经存在则替换
                insertCommand.CommandText = $"INSERT OR REPLACE INTO Is_Wm VALUES (@truename,@is_wm)";
                insertCommand.Parameters.AddWithValue("@truename", truename);
                insertCommand.Parameters.AddWithValue("@is_wm", is_wm);

                insertCommand.ExecuteNonQuery();

                //添加厂商信息
                AddOrIgnoreWm_Producer(producer, is_wm);

                db.Close();
            }
        }

        public static void AddOrIgnoreActor_Video(long actor_id,string video_name)
        {
            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                //添加信息，如果已经存在则忽略
                insertCommand.CommandText = $"INSERT OR IGNORE INTO Actor_Video VALUES (@actor_id,@video_name)";
                insertCommand.Parameters.AddWithValue("@actor_id", actor_id);
                insertCommand.Parameters.AddWithValue("@video_name", video_name);
                insertCommand.ExecuteNonQuery();

                db.Close();
            }
        }

        public static void AddOrIgnoreActor_Names(long id,string name)
        {
            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                //添加信息，如果已经存在则忽略
                insertCommand.CommandText = $"INSERT OR IGNORE INTO Actor_Names VALUES (@id,@name)";
                insertCommand.Parameters.AddWithValue("@id", id);
                insertCommand.Parameters.AddWithValue("@name", name);
                insertCommand.ExecuteNonQuery();

                db.Close();
            }
        }


        /// <summary>
        /// 添加或忽略步兵的生厂商
        /// </summary>
        /// <param name="name"></param>
        public static void AddOrIgnoreWm_Producer(string name,int is_wm)
        {
            if (string.IsNullOrEmpty(name)) return;

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                //添加信息，如果已经存在则替换
                insertCommand.CommandText = $"INSERT OR IGNORE INTO ProducerInfo VALUES (@name,@is_wm)";
                insertCommand.Parameters.AddWithValue("@name", name);
                insertCommand.Parameters.AddWithValue("@is_wm", is_wm);
                insertCommand.ExecuteNonQuery();

                db.Close();
            }
        }

        /// <summary>
        /// 插入演员信息并返回actor_id
        /// </summary>
        /// <param name="data"></param>
        public static long InsertActorInfo(ActorInfo actorInfo)
        {
            long actor_id = 0;

            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand Command = new SqliteCommand();
                Command.Connection = db;

                long addTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                //插入新记录
                Command.CommandText = $"INSERT INTO ActorInfo VALUES (NULL,@name,@is_woman,@birthday,@bwh,@height,@works_count,@work_time,@prifile_path,@blog_url,@is_like,@addtime);";
                Command.Parameters.AddWithValue("@name", actorInfo.name);
                Command.Parameters.AddWithValue("@is_woman", actorInfo.is_woman);
                Command.Parameters.AddWithValue("@birthday", actorInfo.birthday);
                Command.Parameters.AddWithValue("@bwh", actorInfo.bwh);
                Command.Parameters.AddWithValue("@height", actorInfo.height);
                Command.Parameters.AddWithValue("@works_count", actorInfo.works_count);
                Command.Parameters.AddWithValue("@work_time", actorInfo.work_time);
                Command.Parameters.AddWithValue("@prifile_path", actorInfo.prifile_path);
                Command.Parameters.AddWithValue("@blog_url", actorInfo.blog_url);
                Command.Parameters.AddWithValue("@is_like", actorInfo.is_like);
                Command.Parameters.AddWithValue("@addtime", actorInfo.addtime);

                Command.ExecuteReader();

                //查询Task_ID
                Command = new($"SELECT id FROM ActorInfo WHERE name == '{actorInfo.name}'", db);
                actor_id = (long)Command.ExecuteScalar();

                db.Close();
            }

            return actor_id;
        }


        /// <summary>
        /// 插入搜刮日志并返回Task_id
        /// </summary>
        /// <param name="data"></param>
        public static long InsertSpiderLog()
        {
            long task_id = 0;

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand Command = new SqliteCommand();
                Command.Connection = db;

                long addTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                //插入新记录
                Command.CommandText = $"INSERT INTO SpiderLog VALUES (NULL,@time,0);";
                Command.Parameters.AddWithValue("@time", addTime);
                Command.ExecuteReader();

                //查询Task_ID
                Command = new($"SELECT task_id FROM SpiderLog WHERE time == '{addTime}'", db);
                task_id = (long)Command.ExecuteScalar();

                db.Close();
            }

            return task_id;
        }
        
        /// <summary>
        /// 添加搜刮任务
        /// </summary>
        /// <param name="data"></param>
        public static void AddSpiderTask(string name, long task_id)
        {
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                //添加信息，如果已经存在则替换
                insertCommand.CommandText = $"INSERT OR REPLACE INTO SpiderTask VALUES (@name,@bus,@jav321,@avmoo,@avsox,@libre,@fc,@db,@done,@task_id);";

                //fc只有javdb，fc2club能刮
                bool isFc = FileMatch.IsFC2(name);

                insertCommand.Parameters.AddWithValue("@name", name);
                insertCommand.Parameters.AddWithValue("@bus", !isFc && AppSettings.isUseJavBus?"ready": "done");
                insertCommand.Parameters.AddWithValue("@jav321", !isFc && AppSettings.isUseJav321?"ready": "done");
                insertCommand.Parameters.AddWithValue("@avmoo", !isFc && AppSettings.isUseAvMoo?"ready": "done");
                insertCommand.Parameters.AddWithValue("@avsox", AppSettings.isUseAvSox?"ready": "done");
                insertCommand.Parameters.AddWithValue("@libre", !isFc && AppSettings.isUseLibreDmm ? "ready" : "done");
                insertCommand.Parameters.AddWithValue("@fc", isFc && AppSettings.isUseFc2Hub ? "ready" : "done");
                insertCommand.Parameters.AddWithValue("@db", AppSettings.isUseJavDB ? "ready" : "done");
                insertCommand.Parameters.AddWithValue("@done", false);
                insertCommand.Parameters.AddWithValue("@task_id", task_id);

                insertCommand.ExecuteReader();
                db.Close();
            }
        }

        #endregion

        #region 更新

        /// <summary>
        /// 更新FileToInfo表
        /// </summary>
        /// <param name="truename"></param>
        /// <param name="isSuccess"></param>
        public static void UpdataFileToInfo(string truename, bool isSuccess)
        {
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand Command = new SqliteCommand();
                Command.Connection = db;

                //唯一值（pc）重复 则代替 （replace）
                Command.CommandText = $"UPDATE FileToInfo set issuccess = {isSuccess} WHERE truename = '{truename}'";

                try
                {
                    Command.ExecuteReader();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                db.Close();
            }
        }

        /// <summary>
        /// 更新演员头像地址
        /// </summary>
        /// <param name="truename"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void UpdateActorInfoPrifilePath(string name,string prifile_path)
        {
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"UPDATE ActorInfo SET prifile_path = '{prifile_path}' WHERE name = '{name}'", db);

                selectCommand.ExecuteNonQuery();

                db.Close();
            }

            //return data;
        }


        /// <summary>
        /// 更新数据信息，VideoInfo的单个字段
        /// </summary>
        /// <param name="truename"></param>
        /// <returns></returns>
        public static void UpdateSingleDataFromVideoInfo(string truename, string key, string value)
        {
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"UPDATE VideoInfo SET {key} = '{value}' WHERE truename = '{truename}'", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                db.Close();
            }
        }


        /// <summary>
        /// 更新演员信息，ActorInfo的单个字段
        /// </summary>
        /// <param name="truename"></param>
        /// <returns></returns>
        public static void UpdateSingleDataFromActorInfo(string id, string key, string value)
        {
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"UPDATE ActorInfo SET {key} = '{value}' WHERE id = '{id}'", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                db.Close();
            }
        }

        /// <summary>
        /// 更新VideoInfo数据（不包括个性化的内容（喜欢，稍后观看，评分））
        /// </summary>
        /// <param name="videoInfo"></param>
        public static void UpdateDataFromVideoInfo(VideoInfo videoInfo)
        {
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                //UPDATE VideoInfo SET title = 'SKY-251', title = 'SKY-251' WHERE WHERE truename == 'SKY-251'

                Dictionary<string,Tuple<string,string>> Dict = new();
                foreach (var item in videoInfo.GetType().GetProperties())
                {
                    var name = item.Name;

                    //忽略是否步兵
                    //忽略自定义数据
                    if (name == "look_later" || name == "score" || name == "is_like" || name == "is_wm")
                    continue;

                    Dict.Add($"@{name}", new Tuple<string,string>($"{name} = @{name}", $"{item.GetValue(videoInfo)}"));
                }

                //添加信息，如果已经存在则跳过
                insertCommand.CommandText = $"UPDATE VideoInfo SET {string.Join(",", Dict.Values.ToList().Select(item => item.Item1))} WHERE truename == @truename";

                foreach (var item in Dict)
                {
                    insertCommand.Parameters.AddWithValue(item.Key, item.Value.Item2 );
                }

                Debug.WriteLine($"添加信息{videoInfo.truename},{videoInfo.producer},{videoInfo.is_wm}");

                //更新是否步兵
                AddOrReplaceIs_Wm(videoInfo.truename, videoInfo.producer, videoInfo.is_wm);

                insertCommand.ExecuteReader();

                db.Close();
            }
        }

        /// <summary>
        /// 更新图片地址
        /// </summary>
        /// <param name="truename"></param>
        /// <returns></returns>
        public static void UpdateAllImagePath(string srcPath, string dstPath)
        {
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"update VideoInfo set imagepath = REPLACE(imagepath,'{srcPath}','{dstPath}')", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                db.Close();
            }
        }

        /// <summary>
        /// 更新SpiderTask表（源搜刮中，源搜刮完成，全部搜刮完成）
        /// </summary>
        /// <param name="truename"></param>
        /// <param name="isSuccess"></param>
        public static void UpdataSpiderTask(string Name,SpiderSource SpiderSource ,SpiderStates SpiderSourceStatus,bool IsAllDone = false)
        {
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand Command = new SqliteCommand();
                Command.Connection = db;

                Command.CommandText = $"update SpiderTask SET {SpiderSource.name} = '{SpiderSourceStatus}' , done = {IsAllDone} WHERE name == '{Name}'";

                try
                {
                    Command.ExecuteReader();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                db.Close();
            }
        }

        /// <summary>
        /// 更新SpiderLog中的Task_Id为已完成
        /// </summary>
        /// <param name="task_id"></param>
        public static void UpdataSpiderLogDone(long task_id)
        {
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand Command = new SqliteCommand();
                Command.Connection = db;

                Command.CommandText = $"UPDATE SpiderLog SET done = 1 WHERE task_id == {task_id}";

                try
                {
                    Command.ExecuteReader();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                db.Close();
            }
        }

        #endregion

        #region 查询

        /// <summary>
        /// 通过truename查询文件列表
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Datum>> FindFileInfoByTrueName(string truename)
        {
            List<Datum> data = new List<Datum>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                //COLLATE NOCASE 忽略大小写
                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT * FROM FilesInfo,FileToInfo WHERE FilesInfo.pc == FileToInfo.file_pickcode AND FileToInfo.truename == '{truename}' COLLATE NOCASE", db);

                SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

                while (query.Read())
                {
                    data.Add(tryCovertQueryToDatum(query));
                }
                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 加载已存在的一条videoInfo数据
        /// </summary>
        /// <param name="truename"></param>
        /// <returns></returns>
        public static VideoInfo LoadOneVideoInfoByCID(string truename)
        {
            if (truename == null) return null;

            VideoInfo data = new VideoInfo();

            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT * from VideoInfo WHERE truename = '{truename}' LIMIT 1", db);

                SqliteDataReader query = selectCommand.ExecuteReader();
                query.Read();
                data = ConvertQueryTovideoInfo(query);

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 查找truename
        /// </summary>
        /// <param name="truename"></param>
        /// <returns></returns>
        public static List<String> SelectTrueName(string truename)
        {
            List<String> entries = new List<string>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand();
                if (truename.Contains("_"))
                {
                    selectCommand = new SqliteCommand
                        ($"SELECT truename FROM VideoInfo WHERE truename COLLATE NOCASE in ('{truename}', '{truename.Replace('_', '-')}', '{truename.Replace("_", "")}' )", db);
                }
                else
                {
                    selectCommand = new SqliteCommand
                        ($"SELECT truename FROM VideoInfo WHERE truename COLLATE NOCASE in ('{truename}', '{truename.Replace("-", "_")}', '{truename.Replace("-", "")}')", db);
                }

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    entries.Add(query.GetString(0));
                }

                db.Close();
            }

            return entries;
        }

        /// <summary>
        /// 加载DBNAME数据
        /// </summary>
        /// <returns></returns>
        public static List<Datum> LoadDataAccess()
        {
            List<Datum> data = new List<Datum>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT * from FilesInfo", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    data.Add(tryCovertQueryToDatum(query));
                }
                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 查询失败列表
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Datum>> LoadFailFileInfo(int offset = 0, int limit = -1, string n = null, string orderBy = null, bool isDesc = false, FailType showType=FailType.All)
        {
            List<Datum> data = new List<Datum>();

            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
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
                    data.Add(tryCovertQueryToDatum(query));
                }
                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 检查演员列表数量
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int CheckActorInfoCount(List<string> filterList=null)
        {
            int count = 0;
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
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


        public static int CheckFailFilesCount(string n = "",FailType showType= FailType.All)
        {
            if (!string.IsNullOrEmpty(n) && n.Contains("'"))
            {
                n = n.Replace("'", "%");
            }

            int count = 0;
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
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
        /// <param name="n"></param>
        /// <returns></returns>
        public static int CheckVideoInfoCount(string orderBy = null, bool isDesc = false, List<string> filterConditionList = null, string filterKeywords = null, Dictionary<string, string> rangesDicts = null)
        {
            int count = 0;
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
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
                    selectCommand = new SqliteCommand($"SELECT COUNT(VideoInfo.truename) FROM VideoInfo LEFT JOIN Is_Wm ON VideoInfo.truename = Is_Wm.truename LEFT JOIN ProducerInfo ON VideoInfo.producer = ProducerInfo.name{filterStr}{orderStr}", db);
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
        public static Tuple<long,long> GetLatestUnfinishedSpiderLog()
        {
            Tuple<long, long> tuple = null;
            using (SqliteConnection db =
            new SqliteConnection($"Filename={dbpath}"))
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
            new SqliteConnection($"Filename={dbpath}"))
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
        /// 查询当前是否存入该演员
        /// </summary>
        /// <param name="spiderSource"></param>
        /// <returns></returns>
        public static long GetActorIdByName(string actor_name)
        {
            long actor_id = -1;

            using (SqliteConnection db =
            new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                //查询Actor_ID
                SqliteCommand Command = new($"SELECT id FROM Actor_Names WHERE name == '{actor_name}'", db);
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
        /// <param name="spiderSource"></param>
        /// <returns></returns>
        public static string GetOneSpiderTask(SpiderSource spiderSource)
        {
            string name = string.Empty;

            using (SqliteConnection db =
            new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT name from SpiderTask WHERE Done == 0 AND " +
                        $"(libre != 'doing' AND bus != 'doing' AND jav321 != 'doing' AND avmoo != 'doing' AND avsox != 'doing' AND fc != 'doing' AND db != 'doing' AND fc != 'doing') AND " +
                        $"{spiderSource.name} == 'ready' LIMIT 1", db);

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
        /// <param name="spiderSource"></param>
        /// <returns></returns>
        public static int GetWaitSpiderTaskCount(SpiderSource spiderSource)
        {
            int count = 0;

            using (SqliteConnection db =
            new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();


                //TODO 分类讨论，考虑FC2的搜刮源
                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT count(name) from SpiderTask WHERE Done == 0 AND {spiderSource.name} == 'ready'", db);

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
        /// <param name="spiderSource"></param>
        /// <returns></returns>
        public static bool IsAllSpiderSourceAttempt(string QueryName)
        {
            bool isDone = false;

            using (SqliteConnection db =
            new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT name FROM SpiderTask WHERE name == '{QueryName}' AND " +
                    $"libre == 'done' AND bus == 'done' AND jav321 == 'done' AND avmoo == 'done' AND avsox == 'done' AND fc == 'done' AND db == 'done' " +
                    $"AND done == 0", db);

                string name = (string)selectCommand.ExecuteScalar();

                if (!string.IsNullOrEmpty(name))
                    isDone= true;

                db.Close();
            }

            return isDone;
        }

        private static string GetOrderStr(string orderBy,bool isDesc)
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
                                if(filterKeywords == "")
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
        /// <param name="limit"></param>
        /// <returns></returns>
        public static async Task<List<VideoInfo>> LoadVideoInfo(int limit = 1, int offset = 0, string orderBy = null, bool isDesc = false, List<string> filterConditionList = null,string filterKeywords = null,Dictionary<string, string> rangesDicts = null)
        {
            List<VideoInfo> data = new List<VideoInfo>();

            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                //排序
                string orderStr = GetOrderStr(orderBy, isDesc);

                //筛选
                string filterStr = GetVideoInfoFilterStr(filterConditionList, filterKeywords, rangesDicts);
                SqliteCommand selectCommand;

                //多表查询
                if (rangesDicts!=null && rangesDicts.ContainsKey("Type"))
                {
                    selectCommand = new SqliteCommand($"SELECT VideoInfo.* FROM VideoInfo LEFT JOIN Is_Wm ON VideoInfo.truename = Is_Wm.truename LEFT JOIN ProducerInfo ON VideoInfo.producer = ProducerInfo.name{filterStr}{orderStr} LIMIT {limit} offset {offset}",db); 
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
                    data.Add(ConvertQueryTovideoInfo(query));
                }

                db.Close();
            }

            return data;
        }

        public static async Task<List<ActorInfo>> LoadActorInfo(int limit = 1, int offset = 0, Dictionary<string, bool> orderByList = null, List<string> filterList = null)
        {
            List<ActorInfo> data = new ();

            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
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
                if(filterList != null)
                {
                    filterStr = $" WHERE {string.Join(",", filterList)}";
                }

                SqliteCommand Command = new SqliteCommand
                    ($"SELECT ActorInfo.*,COUNT(id) FROM ActorInfo LEFT JOIN Actor_Video ON Actor_Video.actor_id = ActorInfo.id{filterStr} GROUP BY id{orderStr} LIMIT {limit} offset {offset}", db);

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
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand Command = new SqliteCommand
                    ($"SELECT actors.*, COUNT(id)  FROM ( SELECT ActorInfo.* FROM ActorInfo, VideoInfo, Actor_Video WHERE VideoInfo.truename == '{videoName}' AND VideoInfo.truename == Actor_Video.video_name AND Actor_Video.actor_id == ActorInfo.id ) AS actors LEFT JOIN Actor_Video ON Actor_Video.actor_id = actors.id GROUP BY id", db);

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
        /// <param name="limit"></param>
        /// <returns></returns>
        public static string GetLatestFolderPid(string pickCode, int timeEdit = 0)
        {
            string pid = String.Empty;

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
            new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT pid FROM FilesInfo WHERE pc == '{pickCode}' and te == '{timeEdit}'", db);

                if (timeEdit == 0)
                {
                    selectCommand = new SqliteCommand
                        ($"SELECT pid FROM FilesInfo WHERE pc == '{pickCode}'", db);
                }

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    if (query.FieldCount != 0)
                    {
                        pid = query["pid"] as string;
                        break;
                    }
                }

                db.Close();
            }

            return pid;
        }

        /// <summary>
        /// 获取演员出演的视频信息（By label）
        /// </summary>
        /// <param name="actorName"></param>
        /// <returns></returns>
        public static List<VideoInfo> loadVideoInfoByLabel(string label)
        {
            List<VideoInfo> data = new List<VideoInfo>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
                new SqliteConnection($"Filename={dbpath}"))
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
                    data.Add(ConvertQueryTovideoInfo(query));
                }

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 获取演员出演的视频信息（By ActorName）
        /// </summary>
        /// <param name="actorName"></param>
        /// <returns></returns>
        public static List<VideoInfo> loadVideoInfoByActor(string actorName)
        {
            List<VideoInfo> data = new List<VideoInfo>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
                new SqliteConnection($"Filename={dbpath}"))
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
                    data.Add(ConvertQueryTovideoInfo(query));
                }

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 获取演员出演的视频信息（By Name）
        /// </summary>
        /// <param name="actorName"></param>
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
                new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand();

                selectCommand = new SqliteCommand
                    ($"SELECT * from VideoInfo WHERE truename LIKE '%{leftName}%{rightNumber}%'", db);


                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    data.Add(ConvertQueryTovideoInfo(query));
                }

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 获取演员出演的视频信息（By label）
        /// </summary>
        /// <param name="actorName"></param>
        /// <returns></returns>
        public static List<VideoInfo> loadVideoInfoBySomeType(string type, string label)
        {
            List<VideoInfo> data = new List<VideoInfo>();

            using (SqliteConnection db =
                new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand;
                if (label == "")
                {
                    selectCommand = new SqliteCommand($"SELECT * from VideoInfo WHERE {type} == ''", db);
                }
                else
                {
                    selectCommand = new SqliteCommand($"SELECT * from VideoInfo WHERE {type} LIKE '%{label}%'", db);
                }

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    data.Add(ConvertQueryTovideoInfo(query));
                }

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 查询PickCode文件对应的字幕文件（首先匹配文件名，其次匹配上一级文件夹的名称）
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string,string> FindSubFile(string file_pickCode)
        {
            Dictionary<string, string>  subDicts = new();
            using (SqliteConnection db =
                new SqliteConnection($"Filename={dbpath}"))
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

                int numResult;
                bool NameIsNumber = Int32.TryParse(fileNameWithoutExtension, out numResult);

                //1.首先查询同名字幕文件(纯数字则跳过，长度小于10也跳过)
                if (!NameIsNumber && fileNameWithoutExtension.Length > 10)
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
                if (subDicts.Count == 0 && !NameIsNumber)
                {
                    string truename = FileMatch.MatchName(fileName);
                    if (string.IsNullOrEmpty(folderCid) || string.IsNullOrEmpty(truename))
                    {
                        db.Close();
                        return subDicts;
                    }

                    var tuple = FileMatch.SpliteLeftAndRightFromCid(truename);
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

                    if(!NameIsNumber)
                    {
                        subDicts = tmpDict;
                    }
                    else
                    {
                        foreach(var item in tmpDict)
                        {
                            Match match = Regex.Match(item.Value, $"(^|[^0-9]){fileNameWithoutExtension}($|[^0-9])");

                            if(match.Success)
                            {
                                subDicts.Add(item.Key,item.Value);
                            }
                        }
                    }
                }

                db.Close();
            }

            return subDicts;
        }

        /// <summary>
        /// 获取演员出演的视频信息（By TrueName）
        /// </summary>
        /// <param name="actorName"></param>
        /// <returns></returns>
        public static List<Datum> loadVideoInfoByTruename(string truename)
        {
            var tuple = FileMatch.SpliteLeftAndRightFromCid(truename);

            string leftName = tuple.Item1.Replace("FC2", "FC");
            string rightNumber = tuple.Item2;


            if (rightNumber.StartsWith("0"))
            {
                var match_result = Regex.Match(rightNumber, @"0*(\d+)");

                if (match_result.Success)
                    rightNumber = match_result.Groups[1].Value;
            }

            List<Datum> data = new List<Datum>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
                new SqliteConnection($"Filename={dbpath}"))
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
                    tmpList.Add(tryCovertQueryToDatum(query));
                }

                //进一步筛选，通过右侧数字
                // '%xxx%57%' 会选出 057、157、257之类的
                foreach (var datum in tmpList)
                {
                    var match_result = Regex.Match(datum.n, @$"({leftName})[-_]?0*(\d+)", RegexOptions.IgnoreCase);
                    if (match_result.Success && match_result.Groups[2].Value == rightNumber)
                        data.Add(datum);

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
        /// <param name="pid"></param>
        /// <returns></returns>
        public static List<Datum> GetFolderListByPid(string pid, int limit = -1)
        {
            List<Datum> data = new List<Datum>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT * FROM FilesInfo WHERE pid == {pid} LIMIT {limit}", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {

                    data.Add(tryCovertQueryToDatum(query));
                }

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 通过cid获取文件和文件夹列表，CID的下一级目录
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        public static List<Datum> GetListByCid(string cid, int limit = -1)
        {
            List<Datum> data = new List<Datum>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    //($"SELECT n,cid,fid,vdi,pid FROM {TABLENAME} WHERE cid == {cid} or pid == {cid}", db);
                    ($"SELECT * FROM FilesInfo WHERE cid == {cid} and uid != 0 or pid == {cid} LIMIT {limit}", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    data.Add(tryCovertQueryToDatum(query));
                }

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 通过pickCode和ua获取下载记录
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        public static DownInfo GetDownHistoryBypcAndua(string picoCode, string ua)
        {
            DownInfo data = null;

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
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
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<Datum> GetAllFilesInFolderList(List<Datum> dataList)
        {
            List<Datum>  newData = new();

            foreach (var currentFile in dataList)
            {
                //文件夹
                if (string.IsNullOrEmpty(currentFile.fid))
                {
                    List<Datum> newDataList = GetListByCid(currentFile.cid);

                    var fileInFolderList = GetAllFilesInFolderList(newDataList);

                    newData.AddRange(fileInFolderList);
                }
            }

            newData.AddRange(dataList);

            return newData;
        }

        /// <summary>
        ///  遍历cid下所有的文件（文件和文件夹）
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="AllDatumList"></param>
        /// <returns></returns>
        public static List<Datum> GetAllFilesTraverse(string cid, List<Datum> AllDatumList = null)
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
                if (datum.fid == "")
                {
                    AllDatumList = GetAllFilesTraverse(datum.cid, AllDatumList);
                }
            }

            return AllDatumList;
        }

        /// <summary>
        /// 获取标记为“稍后观看”的部分信息（*）
        /// </summary>
        /// <param name="actorName"></param>
        /// <returns></returns>
        public static List<VideoInfo> getNameAndImageFromLookLater()
        {
            List<VideoInfo> data = new List<VideoInfo>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
            new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand();

                selectCommand = new SqliteCommand
                    ($"SELECT * FROM VideoInfo WHERE look_later != 0 ORDER BY look_later DESC", db);


                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    data.Add(ConvertQueryTovideoInfo(query));
                }

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 获取标记为“稍后观看”的部分信息（*）
        /// </summary>
        /// <param name="actorName"></param>
        /// <returns></returns>
        public static List<VideoInfo> getNameAndImageFromLike()
        {
            List<VideoInfo> data = new List<VideoInfo>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
            new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand();

                selectCommand = new SqliteCommand
                    ($"SELECT * FROM VideoInfo WHERE is_like != 0", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    data.Add(ConvertQueryTovideoInfo(query));
                }

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 随机获取信息（*）RANDOM
        /// </summary>
        /// <param name="actorName"></param>
        /// <returns></returns>
        public static List<VideoInfo> getNameAndIamgeRandom()
        {
            List<VideoInfo> data = new List<VideoInfo>();

            using (SqliteConnection db =
            new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand();

                selectCommand = new SqliteCommand
                    ($"SELECT * FROM VideoInfo ORDER BY RANDOM() LIMIT 20", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    data.Add(ConvertQueryTovideoInfo(query));
                }

                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 获取最近视频的部分信息（*）
        /// </summary>
        /// <param name="actorName"></param>
        /// <returns></returns>
        public static List<VideoInfo> getNameAndIamgeRecent()
        {
            List<VideoInfo> data = new List<VideoInfo>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
            new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand();

                selectCommand = new SqliteCommand
                    ($"SELECT * FROM VideoInfo ORDER BY addtime DESC LIMIT 40", db);


                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    data.Add(ConvertQueryTovideoInfo(query));
                }
                db.Close();
            }

            return data;
        }

        /// <summary>
        /// 获取该cid文件夹上一层目录的信息
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        public static Datum getUpperLevelFolderCid(string cid)
        {
            Datum datum = new Datum();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
            new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand();

                selectCommand = new SqliteCommand
                    ($"SELECT * FROM FilesInfo WHERE cid = {cid} AND fid == '' LIMIT 1", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    datum = tryCovertQueryToDatum(query);
                }
                db.Close();
            }

            return datum;
        }

        /// <summary>
        /// 通过文件夹的Cid回溯根目录，不包括当前文件夹
        /// </summary>
        /// <param name="folderCid"></param>
        /// <returns>Cidlist: 0,……,folderCid</returns>
        public static List<Datum> getRootByCid(string folderCid)
        {
            List<Datum> ForlderToRootList = new();

            Datum upperLevelFolderInfo = new() { pid = folderCid };

            int maxDepth = 30;

            ////获取当前目录信息，pid即为上一级目录信息
            //Datum currentFolderInfo = getUpperLevelFolderCid(folderCid);

            for (int i = 0; i < maxDepth; i++)
            {
                upperLevelFolderInfo = getUpperLevelFolderCid(upperLevelFolderInfo.pid);

                if (upperLevelFolderInfo == null) break;

                ForlderToRootList.Add(upperLevelFolderInfo);

                if (upperLevelFolderInfo.pid == "0")
                {
                    ForlderToRootList.Add(new Datum() { cid = "0", n = "根目录" });
                    break;
                }
            }

            ForlderToRootList.Reverse();

            return ForlderToRootList;
        }

        #endregion

        #region 格式转换

        /// <summary>
        /// 转换数据库查询结果到VideoInfo格式
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private static VideoInfo ConvertQueryTovideoInfo(SqliteDataReader query)
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
        /// 转换数据库查询结果到VideoInfo格式
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private static ActorInfo ConvertQueryToActorInfo(SqliteDataReader query)
        {
            ActorInfo dataInfo = new ();
            dataInfo.id = Convert.ToInt32(query["id"]);
            dataInfo.name = query["name"] as string;
            dataInfo.is_woman = Convert.ToInt32(query["is_woman"]);
            dataInfo.birthday = query["birthday"] as string;
            dataInfo.bwh = query["bwh"] as string;
            dataInfo.height = query["height"] as string;
            dataInfo.works_count = Convert.ToInt32(query["works_count"]);
            dataInfo.work_time = query["work_time"] as string;
            dataInfo.prifile_path = query["prifile_path"] as string;
            dataInfo.blog_url = query["blog_url"] as string;
            dataInfo.is_like = Convert.ToInt32(query["is_like"]);
            dataInfo.addtime = Convert.ToInt64(query["addtime"]);
            dataInfo.addtime = Convert.ToInt64(query["addtime"]);
            dataInfo.video_count = Convert.ToInt32(query["COUNT(id)"]);

            return dataInfo;
        }

        /// <summary>
        /// 转换数据库查询结果到Datum格式
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private static Datum tryCovertQueryToDatum(SqliteDataReader query)
        {
            var dataInfo = new Datum();

            if (query.FieldCount == 0) return dataInfo;

            dataInfo.fid = query["fid"] as string;
            dataInfo.uid = Convert.ToInt64(query["uid"]);
            dataInfo.aid = Convert.ToInt32(query["aid"]);
            dataInfo.cid = query["cid"] as string;
            dataInfo.n = query["n"] as string;
            dataInfo.s = Convert.ToInt64(query["s"]);
            dataInfo.sta = Convert.ToInt32(query["sta"]);
            dataInfo.pt = query["pt"] as string;
            dataInfo.pid = query["pid"] as string;
            dataInfo.pc = query["pc"] as string;
            dataInfo.p = Convert.ToInt32(query["p"]);
            dataInfo.m = Convert.ToInt32(query["m"]);
            dataInfo.t = query["t"] as string;
            dataInfo.te = Convert.ToInt32(query["te"]);
            dataInfo.tp = Convert.ToInt32(query["tp"]);
            dataInfo.d = Convert.ToInt32(query["d"]);
            dataInfo.c = Convert.ToInt32(query["c"]);
            dataInfo.sh = Convert.ToInt32(query["sh"]);
            dataInfo.e = query["e"] as string;
            dataInfo.ico = query["ico"] as string;
            dataInfo.sha = query["sha"] as string;
            dataInfo.fdes = query["fdes"] as string;
            dataInfo.q = Convert.ToInt32(query["q"]);
            dataInfo.hdf = Convert.ToInt32(query["hdf"]);
            dataInfo.u = query["u"] as string;
            dataInfo.iv = Convert.ToInt32(query["iv"]);
            dataInfo.current_time = Convert.ToInt32(query["current_time"]);
            dataInfo.played_end = Convert.ToInt32(query["played_end"]);
            dataInfo.last_time = query["last_time"] as string;
            dataInfo.vdi = Convert.ToInt32(query["vdi"]);
            dataInfo.play_long = Convert.ToSingle(query["play_long"]);

            return dataInfo;

        }

        /// <summary>
        /// 添加数据库查询结果到DownInfo格式
        /// </summary>
        /// <param name="query"></param>
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

        #endregion
    }

}
