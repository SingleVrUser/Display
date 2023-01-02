// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Data;
using Display.ContentsPage.Import115DataToLocalDataAccess;
using Display.Model;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using static System.Net.Mime.MediaTypeNames;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.DatumList;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class FileListPage : Page, INotifyPropertyChanged
{
    ObservableCollection<MetadataItem> _units;

    IncrementallLoadDatumCollection _filesInfos;
    IncrementallLoadDatumCollection filesInfos
    {
        get => _filesInfos;
        set
        {
            if(_filesInfos == value) return;
            _filesInfos = value;

            OnPropertyChanged();
        }
    }

    WebApi webApi;


    /// <summary>
    /// ��תվ�ļ�
    /// </summary>
    ObservableCollection<TransferStationFiles> transferStationFiles;

    ///// <summary>
    ///// ����վ�����ļ�
    ///// </summary>
    //List<FilesInfo> RecyFiles;

    public FileListPage()
    {
        this.InitializeComponent();

        _units = new ObservableCollection<MetadataItem>() { new MetadataItem { Label = "��Ŀ¼", Command = OpenFolderCommand, CommandParameter = "0" } };
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        // Raise the PropertyChanged event, passing the name of the property whose value has changed.
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Grid����ʱ����
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Grid_loaded(object sender, RoutedEventArgs e)
    {
        ProgressRing.IsActive = true;

        filesInfos = new("0");
        BaseExample.ItemsSource = filesInfos;
        metadataControl.Items = _units;
        filesInfos.GetFileInfoCompleted += FilesInfos_GetFileInfoCompleted;

        ProgressRing.IsActive = false;

    }

    private void FilesInfos_GetFileInfoCompleted(object sender, GetFileInfoCompletedEventArgs e)
    {
        ChangedOrderIcon(e.orderby, e.asc);
    }

    private RelayCommand<string> _openFolderCommand;

    private RelayCommand<string> OpenFolderCommand =>
        _openFolderCommand ??= new RelayCommand<string>(OpenFolder);

    private async void OpenFolder(string cid)
    {
        var currentItem = _units.FirstOrDefault(item => item.CommandParameter.ToString() == cid);

        //�����ڣ�����
        if (currentItem.CommandParameter == null) return;

        //ɾ��ѡ��·�������·��
        var index = _units.IndexOf(currentItem);

        //�����ڣ�����
        if(index <0 ) return;

        for(int i= _units.Count-1; i> index; i--)
        {
            _units.RemoveAt(i);
        }

        await filesInfos.SetCid(cid);
    }

    private void OpenFolder_Tapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (!(sender is Grid grid)) return;
        if (!(grid.DataContext is FilesInfo filesInfo)) return;


        ChangedFolder(filesInfo);
    }

    private void ChangedFolder(FilesInfo filesInfo)
    {
        //�����ļ�
        if (filesInfo.Type == FilesInfo.FileType.File) return;

        filesInfos.SetCid(filesInfo.Cid);

        _units.Add(new MetadataItem
        {
            Label = filesInfo.Name,
            Command = OpenFolderCommand,
            CommandParameter = filesInfo.Cid,
        });
    }

    private void TextBlock_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (!(sender is TextBlock textBlock)) return;
        if (!(textBlock.DataContext is FilesInfo filesInfo)) return;

        ChangedFolder(filesInfo);
    }

    private void ToTopButton_Click(object sender, RoutedEventArgs e)
    {
        if(BaseExample.ItemsSource is IncrementallLoadDatumCollection Collection)
            BaseExample.ScrollIntoView(Collection.First());
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        //���ѡ�е��ļ����ļ���
        if (BaseExample.SelectedItems.FirstOrDefault() is not FilesInfo) return;

        List<FilesInfo> filesInfo = new();
        foreach(var item in BaseExample.SelectedItems)
        {
            filesInfo.Add((FilesInfo)item);
        }

        var page = new VideoDisplay.MainPage(filesInfo, BaseExample);
        page.CreateWindow();
    }

    private void OrderBy_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (sender is not TextBlock textBlock) return;

        if(textBlock.Inlines.FirstOrDefault() is not Run run) return;

        //string DownSortIconRun = "\uE015";

        switch (run.Text)
        {
            case "����":
                ChangedOrder(WebApi.OrderBy.file_name, Name_Run);
                break;
            case "�޸�ʱ��":
                ChangedOrder(WebApi.OrderBy.user_ptime, Time_Run);
                break;
            case "��С":
                ChangedOrder(WebApi.OrderBy.file_size, Size_Run);
                break;
        }
    }

    private async void ChangedOrder(WebApi.OrderBy orderBy,Run run)
    {
        string UpSortIconRun = "\uE014";
        int asc = run.Text == UpSortIconRun ? 0 : 1;

        await filesInfos.SetOrder(orderBy,asc);
    }

    private void ChangedOrderIcon(WebApi.OrderBy orderBy, int asc)
    {
        string UpSortIconRun = "\uE014";
        string DownSortIconRun = "\uE015";

        Run[] OrderIconRunList = new[] { Time_Run, Name_Run, Size_Run };

        Run run;

        switch (orderBy)
        {
            case WebApi.OrderBy.file_name:
                run = Name_Run;
                break;
            case WebApi.OrderBy.user_ptime:
                run = Time_Run;
                break;
            case WebApi.OrderBy.file_size:
                run = Size_Run;
                break;
            default:
                run = Time_Run;
                break;
        }

        foreach (var itemRun in OrderIconRunList)
        {
            if (itemRun == run)
            {
                itemRun.Text = asc == 1 ? UpSortIconRun : DownSortIconRun;
            }
            else if (itemRun.Text != string.Empty)
            {
                itemRun.Text = string.Empty;
            }
        }
    }



    private void Source_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
    {
        RecyStationGrid.Visibility = Visibility.Visible;

        TransferStation_Grid.Visibility = Visibility.Visible;

        List<Data.FilesInfo> filesInfos = new();

        foreach (Data.FilesInfo item in e.Items)
        {
            filesInfos.Add(item);
        }

        e.Data.Properties.Add("items", filesInfos);

    }

    private void TransferStationGrid_Drop(object sender, DragEventArgs e)
    {
        if (sender is not Grid) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<Data.FilesInfo> sourceFilesInfos) return;

        if (transferStationFiles == null)
        {
            transferStationFiles = new();
            TransferStation_ListView.ItemsSource = transferStationFiles;
        };

        transferStationFiles.Add(new(sourceFilesInfos));
    }

    private void Move_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
    }
    private void Link_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Link;
    }

    private void EmptyTranferStationButton_Click(object sender, RoutedEventArgs e)
    {
        if (transferStationFiles == null) return;

        transferStationFiles.Clear();

        TransferStation_Grid.Visibility = Visibility.Collapsed;
    }

    private async void RecyStationGrid_Drop(object sender, DragEventArgs e)
    {
        if (sender is not Grid) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<Data.FilesInfo> sourceFilesInfos) return;

        //if (RecyFiles == null) RecyFiles = new();

        //RecyFiles = sourceFilesInfos;

        //115ɾ��
        ContentDialog dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = "ȷ��",
            PrimaryButtonText = "ɾ��",
            CloseButtonText = "����",
            DefaultButton = ContentDialogButton.Close,
            Content = "�ò�����ɾ��115�����е��ļ���ȷ��ɾ����"
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            //System.Diagnostics.Debug.WriteLine("ɾ��");

            sourceFilesInfos.ForEach(item => filesInfos.Remove(item));

            if(webApi==null) webApi = new();
            await webApi.DeleteFiles(sourceFilesInfos.FirstOrDefault().datum.pid, sourceFilesInfos.Select(item =>
            {
                //�ļ�
                if (item.Fid != null)
                    return item.Fid;
                //�ļ���
                else
                {
                    return item.Cid;
                }
            }).ToList());
        }



        VisualStateManager.GoToState(this, "NoDelete", true);
    }


    private void RecyStationGrid_DragEnter(object sender, DragEventArgs e)
    {
        VisualStateManager.GoToState(this, "ReadyDelete", true);
    }

    private void RecyStationGrid_DragLeave(object sender, DragEventArgs e)
    {
        VisualStateManager.GoToState(this, "NoDelete", true);
    }

    private async void BaseExample_Drop(object sender, DragEventArgs e)
    {
        if (sender is not ListView target) return;

        if (e.DataView.Properties.Values.FirstOrDefault() is not List<Data.FilesInfo> sourceFilesInfos) return;

        int index = GetInsertIndex(target, e);

        //�ڷ�Χ֮�⣬�˳�
        if (index == -1)
        {
            System.Diagnostics.Debug.WriteLine("�ڷ�Χ֮�⣬�˳�");

            //�ļ��б��е�
            if (filesInfos.Contains(sourceFilesInfos.FirstOrDefault()))
            {

            }
            //��תվ��
            else
            {
                await Move115Files(filesInfos.cid, sourceFilesInfos);

                sourceFilesInfos.ForEach(item => filesInfos.Add(item));
            }


            return;
        }
        else
        {
            //Ŀ�����ƶ��ļ����غϣ��˳�
            var item = filesInfos[index];
            if (sourceFilesInfos.Contains(item))
            {
                System.Diagnostics.Debug.WriteLine("Ŀ�����ƶ��ļ����غϣ��˳�");
                return;
            }

            await Move115Files(item.Cid, sourceFilesInfos);

        }
    }

    /// <summary>
    /// �ƶ�115�ļ�
    /// </summary>
    /// <returns></returns>
    private async Task Move115Files(string pid, List<FilesInfo> files)
    {
        if (webApi == null) webApi = new();
        await webApi.MoveFiles(pid, files.Select(item =>
        {
            //�ļ���
            if(item.Type == FilesInfo.FileType.Folder)
            {
                return item.Cid;
            }
            //�ļ�
            else
            {
                return item.Fid;
            }
        }).ToList());

        //ɾ���б��ļ�
        foreach(var item in files)
        {
            //�ļ��б��е�
            if (filesInfos.Contains(item))
            {
                filesInfos.Remove(item);
            }
        }

        // ɾ����תվ�б�
        if (transferStationFiles != null)
        {
            var transferFiles = transferStationFiles.Where(item =>
            {
                if (item.TransferFiles.Count != files.Count)
                    return false;

                foreach (var file in item.TransferFiles)
                {
                    if (!files.Contains(file))
                        return false;
                }

                return true;
            }).FirstOrDefault();

            if (transferFiles != null)
                transferStationFiles.Remove(transferFiles);
        }

    }

    private int GetInsertIndex(ListView target, DragEventArgs e)
    {
        // Find the insertion index:
        Windows.Foundation.Point pos = e.GetPosition(target.ItemsPanelRoot);

        // If the target ListView has items in it, use the height of the first item
        //      to find the insertion index.
        int index = -1;
        if (target.Items.Count != 0)
        {
            // Get a reference to the first item in the ListView
            ListViewItem sampleItem = (ListViewItem)target.ContainerFromIndex(0);

            // Adjust itemHeight for margins
            double itemHeight = sampleItem.ActualHeight + sampleItem.Margin.Top + sampleItem.Margin.Bottom;

            // Find index based on dividing number of items by height of each item
            int tmp = (int)(pos.Y / itemHeight);

            if(tmp <= target.Items.Count - 1)
            {
                index = tmp;
            }
        }

        return index;
    }


    private void FilesTransferStation_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
    {
        List<Data.FilesInfo> filesInfos = new();

        foreach (TransferStationFiles item in e.Items)
        {
            filesInfos.AddRange(item.TransferFiles);
        }

        e.Data.Properties.Add("items", filesInfos);
    }


    private void Source_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
    {
        RecyStationGrid.Visibility = Visibility.Collapsed;

        if(transferStationFiles == null || transferStationFiles.Count == 0)
        {
            TransferStation_Grid.Visibility = Visibility.Collapsed;
        }
    }

    private async void ImportDataButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button) return;

        List<string> cids = new();
        List<string> nameList = new();
        foreach (FilesInfo item in BaseExample.SelectedItems)
        {
            if(item.Type == FilesInfo.FileType.Folder)
            {
                cids.Add(item.Cid);
                nameList.Add(item.Name);
            }
        }

        if(cids.Count == 0)
        {
            SelectedNull_TeachingTip.IsOpen = true;
            return;
        }

        //ȷ�϶Ի���
        var receiveResult = await ShowContentDialog(nameList);
        if (receiveResult == ContentDialogResult.Primary)
        {
            var page = new Import115DataToLocalDataAccess.Progress(cids);
            page.CreateWindow();
        }

    }

    /// <summary>
    /// ��ʾȷ����ʾ��
    /// </summary>
    /// <param name="selectedItemList"></param>
    /// <returns></returns>
    private async Task<ContentDialogResult> ShowContentDialog(List<string> NameList)
    {
        StackPanel readyStackPanel = new StackPanel();

        readyStackPanel.Children.Add(new TextBlock() { Text = "ѡ���ļ��У�" });
        int index = 0;

        foreach (var name in NameList)
        {
            index++;
            TextBlock textBlock = new TextBlock()
            {
                Text = $"  {index}.{name}",
                IsTextSelectionEnabled = true,
                Margin = new Thickness(0, 2, 0, 0)
            };

            readyStackPanel.Children.Add(textBlock);
        }

        readyStackPanel.Children.Add(
            new TextBlock()
            {
                Text = "δ��������ϵͳ������£����ص��ļ�������������֪��",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.LightGray),
                MaxWidth = 300,
                Margin = new Thickness(0, 8, 0, 0)
            });

        ContentDialog dialog = new ContentDialog();

        dialog.XamlRoot = this.XamlRoot;
        dialog.Title = "ȷ�Ϻ����";
        dialog.PrimaryButtonText = "����";
        dialog.CloseButtonText = "ȡ��";
        dialog.DefaultButton = ContentDialogButton.Primary;
        dialog.Content = readyStackPanel;

        var result = await dialog.ShowAsync();

        return result;
    }

    private async void deleData_Click(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new ContentDialog();

        dialog.XamlRoot = this.XamlRoot;
        dialog.Title = "ȷ�Ϻ����";
        dialog.PrimaryButtonText = "����";
        dialog.CloseButtonText = "ȡ��";
        dialog.DefaultButton = ContentDialogButton.Close;

        RichTextBlock TextHighlightingRichTextBlock = new();

        Paragraph paragraph = new();
        paragraph.Inlines.Add(new Run() { Text = "�ò�����" });
        paragraph.Inlines.Add(new Run() { Text = "ɾ��", Foreground = new SolidColorBrush(Colors.OrangeRed), FontWeight = FontWeights.Bold, FontSize = 15 });
        paragraph.Inlines.Add(new Run() { Text = "֮ǰ�����" });
        paragraph.Inlines.Add(new Run() { Text = "����", Foreground = new SolidColorBrush(Colors.OrangeRed) });
        paragraph.Inlines.Add(new Run() { Text = "115����" });

        TextHighlightingRichTextBlock.Blocks.Add(paragraph);

        dialog.Content = TextHighlightingRichTextBlock;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            DataAccess.DeleteFilesInfoTable();
        }
    }
}

class TransferStationFiles
{
    public string Name { get; set; }
    public List<FilesInfo> TransferFiles { get; set; }

    public TransferStationFiles(List<FilesInfo> transferFiles)
    {
        if(transferFiles.Count == 1)
        {
            this.Name = $"{transferFiles.FirstOrDefault().Name}";
        }
        else if(transferFiles.Count > 1)
        {
            this.Name = $"{transferFiles.FirstOrDefault().Name} ��{transferFiles.Count}���ļ�";
        }

        this.TransferFiles = transferFiles;


    }
}
