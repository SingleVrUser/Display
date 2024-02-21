using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Display.Helper.FileProperties.Name;
using Display.Models.Data;
using Microsoft.UI.Xaml;
using Settings18Page = Display.Views.Sort115.Settings18Page;

namespace Display.ViewModels
{
    public partial class Sort115HomeViewModel : ObservableObject
    {
        public ObservableCollection<Models.Disk._115.Sort115HomeModel> SelectedFiles = new();
        private readonly Models.Disk._115.Sort115Settings _settings = Settings18Page.Settings;
        private readonly WebApi _webApi = WebApi.GlobalWebApi;

        private const string StartName = "开始";
        private const string ResumeName = "继续";
        private const string ClearName = "清空";

        [ObservableProperty]
        private string _buttonName = StartName;

        public int FolderCount => SelectedFiles.Count(model => model.Info.Type == FilesInfo.FileType.Folder);

        public int FileCount => SelectedFiles.Count(model => model.Info.Type == FilesInfo.FileType.File);

        public void SetFiles(List<FilesInfo> files)
        {
            foreach (var info in files.Where(info => info is { Type: FilesInfo.FileType.File }).OrderBy(info=>info.Type))
            {
                SelectedFiles.Add(new Models.Disk._115.Sort115HomeModel(info));
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
        private void Delete(string parameter)
        {
            if (parameter is null) return;

            var toBeDeleted = SelectedFiles.FirstOrDefault(c => c.Info.Name == parameter);

            SelectedFiles.Remove(toBeDeleted);
        }

        [RelayCommand]
        private async Task Start()
        {
            switch (ButtonName)
            {
                case StartName:
                    MatchName();

                    ButtonName = ResumeName;
                    break;
                case ResumeName:
                    await StartRenameAndMove();

                    ButtonName = ClearName;
                    break;
                case ClearName:
                    SelectedFiles.Clear();

                    ButtonName = StartName;
                    break;
            }
        }

        private async Task StartRenameAndMove()
        {
            var infos = SelectedFiles.ToArray();
            var singleInfoList = new List<Models.Disk._115.Sort115HomeModel>();
            var multipleList = new Dictionary<string, List<Models.Disk._115.Sort115HomeModel>>();

            // 正式开始整理
            foreach (var info in infos)
            {
                if(info.Status == Status.Error) continue;

                var destinationPathArray = info.DestinationPathName.Split("\\");

                // 单集
                if (destinationPathArray.Length == 1)
                {
                    info.Status = Status.Doing;

                    // 原文件名与目标名称一致，不修改
                    if (info.DestinationName == info.Info.NameWithoutExtension)
                    {
                        singleInfoList.Add(info);
                    }
                    // 重命名
                    else
                    {
                        await GetInfoFromNetwork.RandomTimeDelay(1, 2);

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
                else if(destinationPathArray.Length == 2)
                {
                    var destinationPath = destinationPathArray[1];

                    if (multipleList.TryGetValue(destinationPath, out var value))
                    {
                        value.Add(info);
                    }
                    else
                    {
                        multipleList[destinationPath] = new List<Models.Disk._115.Sort115HomeModel> { info };
                    }
                }
            }

            // 单集文件最后移动到一个指定目录
            var moveResult = await _webApi.MoveFiles(_settings.SingleVideoSaveExplorerItem.Id, singleInfoList.Select(x => x.Info.Id).ToArray());
            var moveState = moveResult.state ? Status.Success : Status.Error;
            singleInfoList.ForEach(i => { i.Status = moveState; });

            // 多集移动到对应目录
            foreach (var (matchName, infoList) in multipleList)
            {
                // 单集
                if (infoList.Count == 1) continue;

                infoList.ForEach(i => { i.Status = Status.Doing; });


                await GetInfoFromNetwork.RandomTimeDelay(1, 2);

                // 新建文件夹
                var makeDirResult = await _webApi.RequestMakeDir(_settings.MultipleVideoSaveExplorerItem.Id, matchName);
                if (makeDirResult == null)
                {
                    infoList.ForEach(x => x.Status = Status.Error);
                    continue;
                }

                await GetInfoFromNetwork.RandomTimeDelay(1, 2);

                // 移动到
                moveResult = await _webApi.MoveFiles(makeDirResult.cid, infoList.Select(x => x.Info.Id).ToArray());

                moveState = moveResult.state ? Status.Success : Status.Error;
                foreach (var info in infoList.Where(info => info.Status != Status.Error))
                {
                    info.Status = moveState;
                }
            }
        }

        private void MatchName()
        {
            var infos = SelectedFiles.ToArray();

            var tmpTaskList = new Dictionary<string, List<Models.Disk._115.Sort115HomeModel>>();

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

                if (tmpTaskList.TryGetValue(matchName, out var value))
                {
                    value.Add(info);
                }
                else
                {
                    tmpTaskList[matchName] = new List<Models.Disk._115.Sort115HomeModel> { info };
                }
            }

            // 根据数量判断单集和多集
            foreach (var (matchName, itemList) in tmpTaskList)
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
