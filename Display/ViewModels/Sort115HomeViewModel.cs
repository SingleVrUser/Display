using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Display.ContentsPage.Sort115;
using Display.Data;
using Display.Helper;
using Display.Models;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Display.ViewModels
{
    public partial class Sort115HomeViewModel : ObservableObject
    {
        public ObservableCollection<Sort115HomeModel> SelectedFiles = new();
        public ObservableCollection<Sort115HomeModel> SingleFolderSaveVideo = new();
        private Sort115Settings _settings = Settings18Page.Settings;
        private readonly WebApi _webApi = WebApi.GlobalWebApi;

        private const string StartName = "开始";
        private const string ResumeName = "继续";

        [ObservableProperty]
        private string _buttonName = StartName;

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

            var toBeDeleted = SelectedFiles.FirstOrDefault(c => c.Info.Name == parameter);

            SelectedFiles.Remove(toBeDeleted);
        }

        private Dictionary<string, List<Sort115HomeModel>> _tmpTaskList;

        [RelayCommand]
        private async void Start()
        {
            if (ButtonName == StartName)
            {
                MatchName();

                ButtonName = ResumeName;
            }
            else if (ButtonName == ResumeName)
            {
                // 不应该使用这个_tmpTaskList，在正式开始时可删除SelectedFiles项。但_tmpTaskList已经确定了
                if (_tmpTaskList == null) return;

                var singleInfoList = new List<Sort115HomeModel>();

                // 正式开始整理
                foreach (var (matchName,infoList) in _tmpTaskList)
                {
                    // 单集
                    if (infoList.Count == 1)
                    {
                        var info = infoList.First();

                        info.Status = Status.Doing;

                        // 原文件名与目标名称一致，不修改
                        if (info.DestinationName == info.Info.NameWithoutExtension)
                        {
                            singleInfoList.Add(info);
                        }
                        // 重命名
                        else
                        {
                            var renameRequest = await _webApi.RenameFile(info.Info.Id, info.DestinationName);
                            if (renameRequest == null)
                            {
                                info.Status = Status.Error;
                            }
                            else
                            {
                                singleInfoList.Add(info);
                            }
                        }
                    }
                    // 多集
                    else
                    {
                        infoList.ForEach(i =>
                        {
                            i.Status = Status.Doing;
                        });

                        // 新建文件夹
                        var makeDirResult = await _webApi.RequestMakeDir(_settings.MultipleVideoSaveExplorerItem.Id, matchName);
                        if (makeDirResult == null)
                        {
                            infoList.ForEach(x => x.Status = Status.Error);
                            continue;
                        }

                        // 移动到
                        await _webApi.MoveFiles(makeDirResult.cid, infoList.Select(x => x.Info.Id).ToArray());

                        foreach (var info in infoList.Where(info => info.Status != Status.Error))
                        {
                            info.Status = Status.Success;
                        }
                    }
                }

                //单集文件最后移动到一个指定目录
                await _webApi.MoveFiles(_settings.SingleVideoSaveExplorerItem.Id, singleInfoList.Select(x => x.Info.Id).ToArray());

                singleInfoList.ForEach(i =>
                {
                    i.Status = Status.Success;
                });

            }
        }

        private void MatchName()
        {
            var infos = SelectedFiles.ToArray();

            _tmpTaskList = new Dictionary<string, List<Sort115HomeModel>>();

            // 整理预览
            foreach (var info in infos)
            {
                Debug.WriteLine($"{info.Info.Name}");

                var name = info.Info.NameWithoutExtension;

                var matchName = FileMatch.MatchName(name)?.ToUpper();

                if (matchName == null)
                {
                    info.Status = Status.Error;
                    continue;
                }

                if (_tmpTaskList.TryGetValue(matchName, out var value))
                {
                    value.Add(info);
                }
                else
                {
                    _tmpTaskList[matchName] = new List<Sort115HomeModel> { info };
                }
            }

            // 根据数量判断单集和多集
            foreach (var (matchName, itemList) in _tmpTaskList)
            {
                // 单集
                if (itemList.Count == 1)
                {
                    var item = itemList.First();
                    item.DestinationPathName = _settings.SingleVideoSaveExplorerItem.Name;
                    item.DestinationName = matchName;
                }
                // 多集
                else
                {
                    foreach (var item in itemList)
                    {
                        item.DestinationPathName = $"{_settings.MultipleVideoSaveExplorerItem.Name}/{matchName}";
                        item.DestinationName = item.Info.NameWithoutExtension;
                    }
                }
            }
        }

        public Visibility IsShowFolderVideoCount => SelectedFiles.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsShowFolderVideoTip => IsShowFolderVideoCount == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

}
