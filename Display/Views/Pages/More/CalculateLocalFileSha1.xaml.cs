using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Display.Models.Api.OneOneFive.File;
using Display.Models.Dto.OneOneFive;
using Display.Models.Enums;
using Display.Models.Vo.OneOneFive;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Display.Views.Pages.More;

public sealed partial class CalculateLocalFileSha1
{
    readonly ObservableCollection<LocalFileSha1Info> _sha1InfoList = new();

    public CalculateLocalFileSha1()
    {
        this.InitializeComponent();
    }


    private void Grid_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Link;

        e.DragUIOverride.Caption = "拖拽开始获取sha1";

    }

    private async void Grid_Drop(object sender, DragEventArgs e)
    {
        //获取拖入文件信息
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            Operation_StackPanel.Visibility = Visibility.Collapsed;
            CurrentProgressRing.IsActive = true;

            var items = await e.DataView.GetStorageItemsAsync();

            await Task.Delay(1000);
            _sha1InfoList.Clear();
            foreach (var item in items)
            {
                if (item.IsOfType(StorageItemTypes.Folder))
                {
                    _sha1InfoList.Add(new LocalFileSha1Info() { Name = item.Name, FullPath = item.Path, FileType = FileType.Folder });

                }
                else if (item.IsOfType(StorageItemTypes.File))
                {
                    if (item is not StorageFile file) continue;
                    LocalFileSha1Info currentSha1Info = new()
                    {
                        Name = file.Name,
                        FullPath = file.Path,
                        Icon = file.FileType.Replace(".", "")
                    };

                    _sha1InfoList.Add(currentSha1Info);
                }
            }

            Tip_TextBlock.Text = $"拖入文件数量：{items.Count}";

            CurrentProgressRing.IsActive = false;
            Operation_StackPanel.Visibility = Visibility.Visible;
            CopyAllLink_Button.Visibility = Visibility.Visible;
        }

        //开始生成Sha1
        foreach (var item in _sha1InfoList)
        {
            //文件夹
            if (item.FileType == FileType.Folder)
            {
                var fileitems = GetAllFilesInDiretory(item.FullPath);
                item.ProgressBarMaximum = fileitems.Count;
                foreach (var file in fileitems)
                {
                    item.Sha1ShareLink = await GetFileShareSha1Link(file.FullPath);
                    item.ProgressBarValue++;
                }
            }
            //文件
            else
            {
                item.ProgressBarMaximum = 1;
                item.Sha1ShareLink = await GetFileShareSha1Link(item.FullPath);
                item.ProgressBarValue++;
            }
        }

    }

    private List<LocalFileSha1Info> GetAllFilesInDiretory(string folderFullName)
    {
        List<LocalFileSha1Info> items = new();

        DirectoryInfo theFolder = new DirectoryInfo(folderFullName);
        //遍历文件夹
        foreach (DirectoryInfo nextFolder in theFolder.GetDirectories())
        {
            items.AddRange(GetAllFilesInDiretory(nextFolder.FullName));
        }
        //文件
        foreach (FileInfo nextFile in theFolder.GetFiles())
        {
            items.Add(new LocalFileSha1Info() { Name = nextFile.Name, FullPath = nextFile.FullName });
        }

        return items;
    }


    private async Task<string> GetFileShareSha1Link(string filePath)
    {
        string shareSha1Link;

        //退出操作
        //CancellationTokenSource tokenSource = new CancellationTokenSource();

        //文件大小
        var file = new FileInfo(filePath);

        var size = file.Length;

        await using var stream = File.Open(filePath, FileMode.Open);

        //前一部分Sha1
        int bufferSize = 128 * 1024; //每次读取的字节数
        byte[] buffer = new byte[bufferSize];
        _ = stream.Read(buffer, 0, bufferSize);
        Stream partStream = new MemoryStream(buffer);
        var hashDlg = SHA1.Create();

        //byte[] hashBytes = await hashDlg.ComputeHashAsync(partStream, tokenSource.Token);
        byte[] hashBytes = await hashDlg.ComputeHashAsync(partStream);
        var partSha1 = Convert.ToHexString(hashBytes);

        //全部Sha1
        stream.Seek(0, SeekOrigin.Begin);
        //hashBytes = await hashDlg.ComputeHashAsync(stream, tokenSource.Token);
        hashBytes = await hashDlg.ComputeHashAsync(stream);

        var allSha1 = Convert.ToHexString(hashBytes);

        shareSha1Link = $"115://{Path.GetFileName(filePath)}|{size}|{allSha1}|{partSha1}";

        return shareSha1Link;
    }

    private void ListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not LocalFileSha1Info info) return;
        var clipboardText = string.Join("\n", info.Sha1ShareLinkList);
        AddLinkToClipboard(clipboardText);
    }

    private void CopyAllLinkButton_Click(object sender, RoutedEventArgs e)
    {
        List<string> sha1ShareLinkList = [];
        foreach (var sha1Info in _sha1InfoList)
        {
            sha1ShareLinkList.AddRange(sha1Info.Sha1ShareLinkList);
        }

        var clipboardText = string.Join("\n", sha1ShareLinkList);

        AddLinkToClipboard(clipboardText);

    }

    private async void AddLinkToClipboard(string clipboardText)
    {
        //创建一个数据包
        DataPackage dataPackage = new DataPackage();

        //设置创建包里的文本内容
        dataPackage.SetText(clipboardText);

        //把数据包放到剪贴板里
        Clipboard.SetContent(dataPackage);

        DataPackageView dataPackageView = Clipboard.GetContent();
        string text = await dataPackageView.GetTextAsync();
        if (text == clipboardText)
        {
            LightDismissTeachingTip.Content = "Sha1已添加到剪贴板";
            LightDismissTeachingTip.IsOpen = true;
            await Task.Delay(1000);
            LightDismissTeachingTip.IsOpen = false;
        }
    }
}

public class LocalFileSha1Info : INotifyPropertyChanged
{
    public string Name;
    public FileType FileType { get; set; } = FileType.File;
    public long FilesCount { get; set; } = 1;
    public string Icon;
    public string IconPath => FileType == FileType.File ? DetailFileInfo.GetPathFromIcon(Icon) : Constants.FileType.FolderSvgPath;

    public string FullPath;

    public int ProgressBarMaximum
    {
        get => _progressBarMaximum;
        set
        {
            _progressBarMaximum = value;
            FilesCount = _progressBarMaximum;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FilesCount));
        }
    }

    public bool ProgressRingActive
    {
        get
        {
            var isActive = false;

            if (ProgressBarValue < ProgressBarMaximum)
            {
                isActive = true;
            }
            else if (ProgressBarValue == ProgressBarMaximum)
            {
                Sha1TextColorBrush = new SolidColorBrush(Colors.DodgerBlue);
                OnPropertyChanged(nameof(Sha1TextColorBrush));
            }
            return isActive;
        }
    }

    private int _progressBarMaximum { get; set; } = 100;
    private int _progressBarValue { get; set; }
    public int ProgressBarValue
    {
        get => _progressBarValue;
        set
        {
            //bool isUpdateProgressBar = _progressBarValue == 0 ? true : false;

            _progressBarValue = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Sha1ShareLink));
            OnPropertyChanged(nameof(ProgressBarVisibility));
            OnPropertyChanged(nameof(ProgressRingActive));
        }
    }

    public Visibility ProgressBarVisibility
    {
        get
        {
            Visibility visibility;
            if (ProgressBarValue > 0 && ProgressBarValue < ProgressBarMaximum)
            {
                visibility = Visibility.Visible;
            }
            else
            {
                visibility = Visibility.Collapsed;
            }

            return visibility;
        }
    }

    public string Path
    {
        get
        {
            return System.IO.Path.GetDirectoryName(FullPath);
        }
    }

    public SolidColorBrush Sha1TextColorBrush { get; set; } = Application.Current.Resources["ButtonForeground"] as SolidColorBrush;

    private string _sha1ShareLink;
    public string Sha1ShareLink
    {
        get
        {
            return _sha1ShareLink;
        }
        set
        {
            _sha1ShareLink = value;

            Sha1ShareLinkList.Add(_sha1ShareLink);
        }
    }

    public List<string> Sha1ShareLinkList = new();

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}