using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Display.Services.Upload;
using Display.ViewModels;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Display.Models
{
    internal partial class UploadSubItem : ObservableObject
    {
        private readonly long _saveFolderCid;
        private FileUpload _fileUpload;

        public readonly string UploadFilePath;
        public readonly string Name;
        public bool IsFinish;

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

        private void FileUpload_StateChanged(UploadState state)
        {
            switch (state)
            {
                case UploadState.Initializing:
                    Debug.WriteLine("正在初始化");
                    Content = "正在初始化";
                    ProgressVisibility = Visibility.Visible;
                    ProgressIsIndeterminate = true;
                    Running = true;
                    break;
                case UploadState.Initialized:
                    Debug.WriteLine("初始化成功");
                    Content = "初始化成功";
                    ProgressIsIndeterminate = false;
                    Running = true;
                    break;
                case UploadState.FastUploading:
                    Debug.WriteLine("秒传中");
                    Content = "秒传中";
                    UploadButtonVisibility = Visibility.Collapsed;
                    DeleteButtonVisibility = Visibility.Visible;
                    ProgressShowPaused = false;
                    Running = true;
                    break;
                case UploadState.OssUploading:
                    Debug.WriteLine("分片上传中");
                    Content = "分片上传中";
                    UploadButtonVisibility = Visibility.Collapsed;
                    DeleteButtonVisibility = Visibility.Visible;
                    PauseButtonVisibility = Visibility.Visible;
                    ProgressShowPaused = false;
                    Running = true;
                    break;
                case UploadState.Paused:
                    Debug.WriteLine("暂停上传");
                    Content = "暂停上传";
                    ProgressShowPaused = true;
                    UploadButtonVisibility = Visibility.Visible;
                    PauseButtonVisibility = Visibility.Collapsed;
                    Running = false;
                    break;
                case UploadState.Faulted:
                    Debug.WriteLine("上传失败");
                    Content = "上传失败";
                    UploadButtonVisibility = Visibility.Collapsed;
                    ProgressShowError = true;
                    Running = false;
                    IsFinish = true;
                    break;
                case UploadState.Succeed:
                    Debug.WriteLine("上传成功");
                    Content = "上传成功";
                    ProgressVisibility = Visibility.Collapsed;
                    UploadButtonVisibility = Visibility.Collapsed;
                    PauseButtonVisibility = Visibility.Collapsed;
                    DeleteButtonVisibility = Visibility.Visible;
                    Running = false;
                    IsFinish = true;
                    break;
            }
        }

        public async void Start()
        {
            if (!File.Exists(UploadFilePath))
            {
                Debug.WriteLine("需要上传的文件不存在");
                Content = "文件不存在";
                ProgressVisibility = Visibility.Collapsed;

                return;
            }

            _fileUpload = new FileUpload(UploadFilePath, _saveFolderCid);

            _fileUpload.LengthCallback += l =>
            {
                Maximum = l;
            };
            _fileUpload.StateChanged += FileUpload_StateChanged;
            _fileUpload.ContentChanged += c => ProgressContent = c;
            _fileUpload.PositionCallback += p =>
            {
                Position = p;
            };

            await _fileUpload.Init();

            await _fileUpload.Start();

            UploadFinish?.Invoke(_fileUpload.FileUploadResult);
        }

        private bool _running;
        public bool Running
        {
            get => _running;
            set
            {
                if (_running == value) return;

                _running = value;

                RunChanged?.Invoke(_running);
            }
        }
        public event Action<bool> RunChanged;


        public event Action<FileUploadResult> UploadFinish;

        [RelayCommand]
        private void Resume()
        {
            _fileUpload?.Resume();
        }

        [RelayCommand]
        private void Pause()
        {
            _fileUpload?.Pause();
        }


        [RelayCommand]
        private void DeleteAsync()
        {
            _fileUpload?.Stop();
            UploadViewModel.Instance.UploadCollection.Remove(this);
        }
    }
}
