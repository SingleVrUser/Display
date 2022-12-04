using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace Data
{
    public static class DataAccess
    {
        private const string DBNAME = "115_uwp.db";
        private const string TABLENAME = "FilesInfo";
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
            //dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);

            string newDBPath = Path.Combine(DataAccess_SavePath, DBNAME);

            //获取文件夹，没有则创建
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(AppSettings.DataAccess_SavePath);
            //获取数据库文件，没有则创建
            await folder.CreateFileAsync(DBNAME, CreationCollisionOption.OpenIfExists);

            //dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);

            using (SqliteConnection db =
               new SqliteConnection($"Filename={newDBPath}"))
            {
                db.Open();

                string tableCommand = "CREATE TABLE IF NOT " +
                    $"EXISTS {TABLENAME} ( " +
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
                createTable.ExecuteReader();

                tableCommand = "CREATE TABLE IF NOT " +
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

                createTable = new SqliteCommand(tableCommand, db);
                createTable.ExecuteReader();

                tableCommand = "CREATE TABLE IF NOT " +
                    $"EXISTS FileToInfo ( " +
                      "file_pickcode text," +
                      "truename text," +
                      "issuccess integer," +
                      "PRIMARY KEY('file_pickcode')" +
                      ") ";

                createTable = new SqliteCommand(tableCommand, db);
                createTable.ExecuteReader();
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
            foreach(var file in Files)
            {
                if (!cidList.Contains(file.cid))
                {
                    cidList.Add(file.cid);
                }
            }
            foreach(var cidFolder in cidList)
            {
                DeleteDirectoryAndFiles_InFilesInfoTable(cidFolder);
            }

        }

        /// <summary>
        /// 添加115文件信息
        /// </summary>
        /// <param name="data"></param>
        public static void AddFilesInfo(Datum data)
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
                insertCommand.CommandText = $"INSERT OR REPLACE INTO {TABLENAME} VALUES ({string.Join(",", keyList)});";

                foreach (var item in data.GetType().GetProperties())
                {
                    //fl为数组，应添加进新表中，一对多。目前暂不考虑，故跳过
                    if (item.Name == "fl") continue;

                    insertCommand.Parameters.AddWithValue("@" + item.Name, $"{item.GetValue(data)}");
                }

                insertCommand.ExecuteReader();

                db.Close();
            }
        }

        /// <summary>
        /// 添加FileToInfo表
        /// </summary>
        /// <param name="pickCode"></param>
        /// <param name="truename"></param>
        public static void AddFileToInfo(string pickCode,string truename,bool issuccess= false,bool isReplace = false)
        {
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                //唯一值（pc）重复 则代替 （replace）
                string replaceStr = isReplace ? " OR REPLACE":string.Empty;
                insertCommand.CommandText = $"INSERT{replaceStr} INTO FileToInfo VALUES (@file_pickcode,@truename,@issuccess);";

                insertCommand.Parameters.AddWithValue("@file_pickcode", pickCode);

                if(truename == null)
                {
                    truename = string.Empty;
                }
                insertCommand.Parameters.AddWithValue("@truename", truename);
                insertCommand.Parameters.AddWithValue("@issuccess", issuccess);

                try
                {
                    insertCommand.ExecuteReader();
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                db.Close();
            }
        }

        /// <summary>
        /// 更新FileToInfo表
        /// </summary>
        /// <param name="truename"></param>
        /// <param name="isSuccess"></param>
        public static void UpdataFileToInfo(string truename,bool isSuccess)
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
        /// 添加视频信息
        /// </summary>
        /// <param name="data"></param>
        public static void AddVideoInfo(VideoInfo data)
        {
            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                List<string> keyList = new List<string>();
                foreach (var item in data.GetType().GetProperties())
                {
                    ////fl为数组，应添加进新表中，一对多。目前暂不考虑，故跳过
                    //if (item.Name == "fl") continue;

                    keyList.Add($"@{item.Name}");
                }

                //添加信息，如果已经存在则跳过
                insertCommand.CommandText = $"INSERT OR IGNORE INTO VideoInfo VALUES ({string.Join(",", keyList)});";

                foreach (var item in data.GetType().GetProperties())
                {
                    ////fl为数组，应添加进新表中，一对多。目前暂不考虑，故跳过
                    //if (item.Name == "fl") continue;

                    insertCommand.Parameters.AddWithValue("@" + item.Name, $"{item.GetValue(data)}");
                }

                insertCommand.ExecuteReader();

                db.Close();
            }
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
                    ($"SELECT * from {TABLENAME}", db);

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
        public static async Task<List<Datum>> LoadFailFileInfo(int offset = 0,int limit = -1,string n = "")
        {
            List<Datum> data = new List<Datum>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                if (n.Contains("'"))
                {
                    n = n.Replace("'","%");
                }

                string queryStr = string.IsNullOrEmpty(n) ? string.Empty : $" And FilesInfo.n LIKE '%{n}%'";

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT * FROM FilesInfo,FileToInfo WHERE FileToInfo.issuccess == 0 AND FilesInfo.pc == FileToInfo.file_pickcode{queryStr} LIMIT {limit} offset {offset}", db);

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

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
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
        /// 更新数据信息
        /// </summary>
        /// <param name="truename"></param>
        /// <returns></returns>
        public static void UpdateSingleDataFromVideoInfo(string truename, string key, string value)
        {
            //VideoInfo data = new VideoInfo();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"UPDATE VideoInfo SET {key} = '{value}' WHERE truename = '{truename}'", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                db.Close();
            }

            //return data;
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

            //return data;
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
        /// 加载已存在的videoInfo数据
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static List<VideoInfo> LoadAllVideoInfo(int limit = 1,int offset = 0)
        {
            List<VideoInfo> data = new List<VideoInfo>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT * from VideoInfo LIMIT {limit} offset {offset}", db);

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
        /// 通过PickCode以及TimeEdit判断文件夹是否存在且未更新。
        /// 针对的是文件夹
        /// 如果条件为真，则返回pid（如果是文件夹，则为上一级目录的cid；如果是文件则为空）
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static string GetLastestFolderPid(string pickCode,int timeEdit = 0)
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
                    if(query.FieldCount != 0)
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

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
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
        /// 获取演员出演的视频信息（By TrueName）
        /// </summary>
        /// <param name="actorName"></param>
        /// <returns></returns>
        public static List<Datum> loadVideoInfoByTruename(string truename)
        {
            truename = truename.ToUpper();
            string[] splitList = truename.Split(new char[] {'-','_'});
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

            leftName = leftName.Replace("FC2", "FC");

            List<Datum> data = new List<Datum>();

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
            using (SqliteConnection db =
                new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand();

                //vdi = 0，即视频转码未成功，无法在线观看
                //selectCommand = new SqliteCommand ($"SELECT * from FilesInfo WHERE uid != 0 AND vdi != 0 AND n LIKE '%{leftName}%{rightNumber}%'", db);
                selectCommand = new SqliteCommand ($"SELECT * from FilesInfo WHERE uid != 0 AND iv = 1 AND n LIKE '%{leftName}%{rightNumber}%'", db);


                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    data.Add(tryCovertQueryToDatum(query));
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
                    ($"SELECT * FROM {TABLENAME} WHERE pid == {pid} LIMIT {limit}", db);

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
                    ($"SELECT * FROM {TABLENAME} WHERE cid == {cid} and uid != 0 or pid == {cid} LIMIT {limit}", db);

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
        /// 获取当前文件列表下所有的文件
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<Datum> GetAllFilesInFolderList(List<Datum> dataList)
        {
            var newData = new List<Datum>();

            foreach (var currentFile in dataList)
            {
                //文件夹
                if(currentFile.fid == "")
                {
                    List<Datum> newDataList = GetListByCid(currentFile.cid);
                    newData.AddRange( GetAllFilesInFolderList(newDataList));
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
            if(AllDatumList == null)
            {
                AllDatumList = new List<Datum>();
            }

            List<Datum> datumList = GetListByCid(cid);
            AllDatumList.AddRange(datumList);

            foreach(Datum datum in datumList)
            {
                //文件夹
                if(datum.fid == "")
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

            //string dbpath = Path.Combine(AppSettings.DataAccess_SavePath, DBNAME);
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
        /// 获取该cid文件夹上一层目录的cid
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        private static Datum getUpperLevelFolderCid(string cid)
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
                    ForlderToRootList.Add(new Datum() { cid = "0" , n="根目录"});
                    break;
                }
            }

            ForlderToRootList.Reverse();

            return ForlderToRootList;
        }

        /// <summary>
        /// 添加数据库查询结果到VideoInfo格式
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

            return dataInfo;
        }

        /// <summary>
        /// 添加数据库查询结果到Datum格式
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

    }

}
