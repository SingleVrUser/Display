#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Provider;
using Windows.System;

namespace Display.Helper.Data
{
    internal class LocalCacheHelper
    {
        private const long MaxCount = 100;
        private const long MaxSize = 1024 * 1024 * 460;

        public static readonly string CachePath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "Images");

        public static string? GetCacheFilePath(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            var path = Path.Combine(CachePath, name);
            return !File.Exists(path) ? null : path;
        }

        public static async Task<string?> SaveCacheFilePath(string name, Stream stream, IProgress<long>? progress = null, CancellationToken token = default)
        {
            if (!Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
            }

            string path;
            try
            {
                path = Path.Combine(CachePath, name);

                var buffer = new byte[81920];

                await using var newStream = File.Create(path);
                int bytesRead;
                var position = 0;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) != 0)
                {
                    await newStream.WriteAsync(buffer, 0, bytesRead, token);

                    position += bytesRead;
                    progress?.Report(position);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"保存缓存文件时发生错误：{ex.Message}");

                return null;
            }

            return path;
        }

        public static async Task<bool> OpenCachePath()
        {
            if (!Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
            }

            return await Launcher.LaunchFolderPathAsync(CachePath);
        }

        public static async void ClearAllCacheIfSizeTooLarge()
        {
            if (!Directory.Exists(CachePath)) return;

            // 检查缓存占用大小
            var folder = await StorageFolder.GetFolderFromPathAsync(CachePath);

            DirectoryInfo directoryInfo = new DirectoryInfo(CachePath);

            long length = 0;
            var files = directoryInfo.GetFiles();

            // 检查数量
            if (files.Length > MaxCount)
            {
                Debug.WriteLine("清空缓存");
                ClearAllCache();
                return;
            }

            // 检查大小
            foreach (var file in files)
            {
                length += file.Length;

                if (length <= MaxSize) continue;

                Debug.WriteLine("清空缓存");
                ClearAllCache();
                break;
            }

            Debug.WriteLine($"缓存占用：{length}");
        }

        public static void ClearAllCache()
        {
            var dir = new DirectoryInfo(CachePath);
            var infos = dir.GetFileSystemInfos();
            foreach (var info in infos)
            {
                switch (info)
                {
                    // 文件
                    case FileInfo:
                        info.Delete();
                        break;
                    // 文件夹
                    case DirectoryInfo:
                        {
                            // 遍历删除
                            //var subDir = new DirectoryInfo(info.FullName);
                            //subDir.Delete(true);
                            break;
                        }
                }
            }
        }

        public static async Task<bool> ExportFile(string srcName, StorageFile saveFile)
        {
            var path = Path.Combine(CachePath, srcName);

            if (!File.Exists(path)) return false;

            CachedFileManager.DeferUpdates(saveFile);
            await using var srcStream = File.OpenRead(path);
            await using var dstStream = await saveFile.OpenStreamForWriteAsync();

            await srcStream.CopyToAsync(dstStream);
            var status = await CachedFileManager.CompleteUpdatesAsync(saveFile);
            switch (status)
            {
                case FileUpdateStatus.Complete:
                    Debug.WriteLine("File " + srcName + " was saved.");
                    return true;
                case FileUpdateStatus.CompleteAndRenamed:
                    Debug.WriteLine("File " + srcName + " was renamed and saved.");
                    return true;
                default:
                    Debug.WriteLine("File " + srcName + " couldn't be saved.");
                    break;
            }

            return false;
        }
    }
}
