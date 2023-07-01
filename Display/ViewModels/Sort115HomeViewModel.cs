using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Display.ContentsPage.Sort115;
using Display.Data;
using Display.Models;
using System.Security.Cryptography;

namespace Display.ViewModels
{
    public partial class Sort115HomeViewModel : ObservableObject
    {
        public ObservableCollection<Sort115HomeModel> SelectedFiles = new();

        public ObservableCollection<Sort115HomeModel> SingleFolderSaveVideo = new();

        public int FolderCount => SelectedFiles.Count(model => model.Info.Type == FilesInfo.FileType.Folder);

        public int FileCount => SelectedFiles.Count(model => model.Info.Type == FilesInfo.FileType.File);

        public void SetFiles(List<FilesInfo> files)
        {
            foreach (var info in files.Where(info => info is { Type: FilesInfo.FileType.File }).OrderBy(info=>info.Type))
            {
                SelectedFiles.Add(new Sort115HomeModel(info));
            }

            SelectedFiles.CollectionChanged += SelectedFilesCollectionChanged;
        }

        private void SelectedFilesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(FolderCount));
            OnPropertyChanged(nameof(FileCount));

            OnPropertyChanged(nameof(IsShowFolderVideoCount));
            OnPropertyChanged(nameof(IsShowFolderVideoTip));
        }

        [RelayCommand]
        private void DeleteFolderSaveVideo(string parameter)
        {
            if (parameter is null) return;

            var toBeDeleted = SingleFolderSaveVideo.FirstOrDefault(c => c.Info.Name == parameter);

            SingleFolderSaveVideo.Remove(toBeDeleted);
        }

        [RelayCommand]
        private void Delete(string parameter)
        {
            if (parameter is null) return;

            Debug.WriteLine("删除该文件夹");

            var toBeDeleted = SelectedFiles.FirstOrDefault(c => c.Info.Name == parameter);

            SelectedFiles.Remove(toBeDeleted);
        }

        [RelayCommand]
        private async void Start()
        {
            var infos = SelectedFiles.ToList();
            
            var settings = Settings18Page.Settings;

            // 单集保存路径
            Debug.WriteLine($"单集保存路径：{settings.SingleVideoSaveExplorerItem.Name}");

            // 多集保存路径
            Debug.WriteLine($"多集保存路径：{settings.MultipleVideoSaveExplorerItem.Name}");

            var multipleDict = new Dictionary<string, List<Sort115HomeModel>>();
            var singleList = new List<Sort115HomeModel>();

            const string matchRegex = "(\\d?[a-z]+-?\\d+)([_-]([2468]ks?([36]0fps)?)|hhb|.part)?(\\d?)(_8k)?$";

            // 整理预览
            foreach (var info in infos)
            {
                Debug.WriteLine($"{info.Info.Name}");

                var name = info.Info.NameWithoutExtension;

                var match = Regex.Match(name, matchRegex, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    var trueName = match.Groups[1].Value;
                    Debug.WriteLine($"  名称:{trueName}");
                    Debug.WriteLine($"  规格:{match.Groups[3].Value}");

                    info.DestinationName = match.Value;

                    // 单集
                    if (string.IsNullOrEmpty(match.Groups[^2].Value))
                    {
                        info.DestinationPathName = settings.SingleVideoSaveExplorerItem.Name;

                        singleList.Add(info);
                    }
                    // 多集
                    else
                    {
                        if (int.TryParse(match.Groups[^2].Value, out var num))
                        {
                            info.DestinationPathName = $"{settings.MultipleVideoSaveExplorerItem.Name}/{trueName}";

                            Debug.WriteLine($"  当前集为：{num}");

                            if (multipleDict.TryGetValue(trueName, out var lists))
                            {
                                lists.Add(info);
                            }
                            else
                            {
                                multipleDict[trueName] =
                                    new List<Sort115HomeModel> { info };
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("  匹配失败");

                    info.Status = Status.Error;
                }
            }

            // 正式开始整理

            // 整理单集
            var singleFids = new List<long?>();
            var webApi = WebApi.GlobalWebApi;

            foreach (var info in singleList)
            {
                info.Status = Status.Doing;

                var id = info.Info.Id;

                singleFids.Add(id);

                // 重命名
                if (info.DestinationName == info.Info.NameWithoutExtension) continue;

                var renameRequest = await webApi.RenameFile(id, info.DestinationName);
                if (renameRequest == null)
                {
                    info.Status = Status.Error;
                }

            }
            // 移动到
            await webApi.MoveFiles(settings.SingleVideoSaveExplorerItem.Id, singleFids.ToArray());
            foreach (var info in singleList.Where(info => info.Status != Status.Error))
            {
                info.Status = Status.Success;
            }

            // 整理多集
            foreach (var item in multipleDict)
            {
                var folderName = item.Key;
                var fileInfo = item.Value;
                
                fileInfo.ForEach(x => x.Status = Status.Doing);

                // 新建文件夹
                var makeDirResult = await webApi.RequestMakeDir(settings.MultipleVideoSaveExplorerItem.Id, folderName);
                if (makeDirResult == null)
                {
                    fileInfo.ForEach(x=>x.Status = Status.Error);
                    return;
                }

                // 移动到
                await webApi.MoveFiles(makeDirResult.cid, fileInfo.Select(x=>x.Info.Id).ToArray());

                foreach (var info in fileInfo.Where(info => info.Status != Status.Error))
                {
                    info.Status = Status.Success;
                }
            }
        }

        public Visibility IsShowFolderVideoCount => SelectedFiles.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsShowFolderVideoTip => IsShowFolderVideoCount == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

}
