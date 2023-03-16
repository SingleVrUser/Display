using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Display.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Data;

namespace Display.ViewModels
{
    public class Sort115HomeViewModel:ObservableObject
    {
        public ObservableCollection<FilesInfo> FoldersVideo = new();

        public ObservableCollection<FilesInfo> SingleFolderSaveVideo = new();

        public Sort115HomeViewModel()
        {
        }

        public void SetFolders(List<FilesInfo> folders)
        {
            folders.ForEach(c => FoldersVideo.Add(c));

            //FoldersVideo = new ObservableCollection<FilesInfo>(folders);

            FoldersVideo.CollectionChanged += FoldersVideo_CollectionChanged;
        }

        private void FoldersVideo_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsShowFolderVideoCount));
            OnPropertyChanged(nameof(IsShowFolderVideoTip));
        }

        public ICommand DeleteCommand => new RelayCommand<string>(DeleteCommand_Executed);
        public ICommand DeleteFolderSaveVideoCommand => new RelayCommand<string>(DeleteFolderSaveVideoCommand_Executed);

        private void DeleteFolderSaveVideoCommand_Executed(string parameter)
        {
            if (parameter is null) return;

            var toBeDeleted = SingleFolderSaveVideo.FirstOrDefault(c => c.Name == parameter);

            SingleFolderSaveVideo.Remove(toBeDeleted);
        }


        private void DeleteCommand_Executed(string parameter)
        {
            if (parameter is null) return;

            Debug.WriteLine("删除该文件夹");

            var toBeDeleted = FoldersVideo.FirstOrDefault(c => c.Name == parameter);

            FoldersVideo.Remove(toBeDeleted);
        }

        public Visibility IsShowFolderVideoCount => FoldersVideo.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsShowFolderVideoTip => IsShowFolderVideoCount == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }
}
