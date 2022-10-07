using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using static Data.FilesInfo;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CalculateLocalFileSha1 : Page
    {

        ObservableCollection<LocalFileSha1Info> sha1InfoList = new();

        public CalculateLocalFileSha1()
        {
            this.InitializeComponent();
        }


        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Link;

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
                sha1InfoList.Clear();
                foreach (var item in items)
                {
                    if (item.IsOfType(StorageItemTypes.Folder))
                    {
                        sha1InfoList.Add(new LocalFileSha1Info() { Name = item.Name, FullPath= item.Path, FileType=FileType.Folder });


                    }
                    else if (item.IsOfType(StorageItemTypes.File))
                    {
                        var File = item as StorageFile;
                        LocalFileSha1Info currentSha1Info = new();
                        currentSha1Info.Name = File.Name;
                        currentSha1Info.FullPath = File.Path;
                        currentSha1Info.Icon = File.FileType.Replace(".", "");

                        sha1InfoList.Add(currentSha1Info);
                    }
                }

                Tip_TextBlock.Text = $"拖入文件数量：{items.Count}";
                
                CurrentProgressRing.IsActive = false;
                Operation_StackPanel.Visibility = Visibility.Visible;
                CopyAllLink_Button.Visibility = Visibility.Visible;
            }

            //开始生成Sha1
            foreach(var item in sha1InfoList)
            {
                //文件夹
                if(item.FileType == FileType.Folder)
                {
                    var Fileitems = GetAllFilesInDiretory(item.FullPath);
                    item.ProgressBarMaximum = Fileitems.Count;
                    foreach(var file in Fileitems)
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
            List<LocalFileSha1Info> Items = new();

            DirectoryInfo TheFolder = new DirectoryInfo(folderFullName);
            //遍历文件夹
            foreach (DirectoryInfo NextFolder in TheFolder.GetDirectories())
            {
                Items.AddRange(GetAllFilesInDiretory(NextFolder.FullName));
            }
            //文件
            foreach (FileInfo NextFile in TheFolder.GetFiles())
            {
                Items.Add(new LocalFileSha1Info() { Name = NextFile.Name, FullPath = NextFile.FullName });
            }

            return Items;
        }


        private async Task<string> GetFileShareSha1Link(string filePath)
        {
            string shareSha1Link = "";

            //退出操作
            //CancellationTokenSource tokenSource = new CancellationTokenSource();

            //文件大小
            System.IO.FileInfo file = new System.IO.FileInfo(filePath);

            var size = file.Length;

            using (var stream = File.Open(filePath, FileMode.Open))
            {
                //前一部分Sha1
                int bufferSize = 128 * 1024; //每次读取的字节数
                byte[] buffer = new byte[bufferSize];
                var item = stream.Read(buffer, 0, bufferSize);
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
            }

            return shareSha1Link;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {

            string ClipboardText = string.Join("\n", (e.ClickedItem as LocalFileSha1Info).Sha1ShareLinkList);

            AddLinkToClipboard(ClipboardText);
        }

        private void CopyAllLinkButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> Sha1ShareLinkList = new();
            foreach (var sha1Info in sha1InfoList)
            {
                Sha1ShareLinkList.AddRange(sha1Info.Sha1ShareLinkList);
            }

            string ClipboardText = string.Join("\n", Sha1ShareLinkList);

            AddLinkToClipboard(ClipboardText);

        }

        private async void AddLinkToClipboard(string ClipboardText)
        {
            //创建一个数据包
            DataPackage dataPackage = new DataPackage();

            //设置创建包里的文本内容
            dataPackage.SetText(ClipboardText);

            //把数据包放到剪贴板里
            Clipboard.SetContent(dataPackage);

            DataPackageView dataPackageView = Clipboard.GetContent();
            string text = await dataPackageView.GetTextAsync();
            if (text == ClipboardText)
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
        public string IconPath
        {
            get
            {
                string iconpath;
                if (FileType == FileType.File)
                {
                    iconpath = getPathFromFileType(Icon);
                }
                else
                {
                    iconpath = "ms-appx:///Assets/115/file_type/folder/folder.svg";
                }
                return iconpath;
            }
        }

        public string Size;
        public string FullPath;

        private int _progressBarMaximum { get; set; } = 100;
        public int ProgressBarMaximum
        {
            get
            {
                return _progressBarMaximum;
            }
            set
            {
                _progressBarMaximum = value;
                FilesCount = _progressBarMaximum;
                OnPropertyChanged();
                OnPropertyChanged("FilesCount");
            }
        }

        public bool ProgressRingActive
        {
            get
            {
                bool isActive = false;

                if(ProgressBarValue < ProgressBarMaximum)
                {
                    isActive = true;
                }
                else if(ProgressBarValue == ProgressBarMaximum)
                {
                    Sha1TextColorBrush = new SolidColorBrush(Colors.DodgerBlue);
                    OnPropertyChanged("Sha1TextColorBrush");
                }
                return isActive;
            }
        }

        private int _progressBarValue { get; set; } = 0;
        public int ProgressBarValue
        {
            get
            {
                return _progressBarValue;
            }
            set
            {
                //bool isUpdateProgressBar = _progressBarValue == 0 ? true : false;

                _progressBarValue = value;
                OnPropertyChanged();
                OnPropertyChanged("Sha1ShareLink");
                OnPropertyChanged("ProgressBarVisibility");
                OnPropertyChanged("ProgressRingActive");
            }
        }

        public Visibility ProgressBarVisibility
        {
            get
            {
                Visibility visibility = Visibility.Collapsed ;
                if (ProgressBarValue > 0 && ProgressBarValue < ProgressBarMaximum)
                {
                    visibility = Visibility.Visible;
                }
                else { 
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
}
