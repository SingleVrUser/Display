using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Display.Services.Upload;

namespace Display.Models
{
    internal partial class UploadSubItem : ObservableObject
    {
        public readonly string UploadFilePath;
        public readonly string Name;

        private readonly long _saveFolderCid;

        private FileUpload _fileUpload;

        [ObservableProperty]
        private string _content;

        [ObservableProperty]
        private string _progressContent;

        [ObservableProperty]
        private Visibility _progressVisibility;

        [ObservableProperty]
        private Visibility _uploadButtonVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility _pauseButtonVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility _deleteButtonVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _progressIsIndeterminate;

        [ObservableProperty]
        private bool _progressShowError;

        [ObservableProperty]
        private bool _progressShowPaused;

        [ObservableProperty]
        private double _maximum;

        [ObservableProperty]
        private double _position;

        public UploadSubItem(string path, long cid)
        {
            UploadFilePath = path;
            Name = Path.GetFileName(path);
            _saveFolderCid = cid;
        }

        [RelayCommand]
        private void Resume()
        {

        }


        [RelayCommand]
        private void Pause()
        {

        }


        [RelayCommand]
        private void DeleteAsync()
        {
        }
    }
}
