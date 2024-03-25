using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Display.Models.Upload;
using Display.Services.Upload;
using Display.ViewModels;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.IO;

namespace Display.Services;

internal partial class UploadSubItem(string path, long cid) : ObservableObject
{
    private FileUploadService _fileUploadService;

    public readonly string UploadFilePath = path;
    public readonly string Name = Path.GetFileName(path);
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

    public const int Maximum = 100;

    [ObservableProperty]
    private int _position;

    public UploadState State;

    private void FileUploadServiceStateChanged(UploadState state)
    {
        State = state;

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
                PauseButtonVisibility = Visibility.Collapsed;
                ProgressShowError = true;
                // Running在后，Running改变后有Action
                IsFinish = true;
                Running = false;
                break;
            case UploadState.Succeed:
                Debug.WriteLine("上传成功");
                Content = "上传成功";
                ProgressVisibility = Visibility.Collapsed;
                UploadButtonVisibility = Visibility.Collapsed;
                PauseButtonVisibility = Visibility.Collapsed;
                DeleteButtonVisibility = Visibility.Visible;

                // Running在后，Running改变后有Action
                IsFinish = true;
                Running = false;
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

        _fileUploadService = new FileUploadService(UploadFilePath, cid);

        ProgressContent = _fileUploadService.Content;
        var length = _fileUploadService.Length;
        _fileUploadService.ContentChanged += c => ProgressContent = c;
        _fileUploadService.StateChanged += FileUploadServiceStateChanged;
        _fileUploadService.PositionCallback += p =>
        {
            Position = p == 0 ? 0 : length == p ? Maximum : (int)(Maximum * (p / (double)length));
        };

        await _fileUploadService.Init();

        await _fileUploadService.Start();

        UploadFinish?.Invoke(_fileUploadService.FileUploadResult);
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
        _fileUploadService?.Resume();
    }

    [RelayCommand]
    private void Pause()
    {
        _fileUploadService?.Pause();
    }

    [RelayCommand]
    private void DeleteAsync()
    {
        _fileUploadService?.Stop();
        App.GetService<UploadViewModel>().UploadCollection.Remove(this);
    }
}