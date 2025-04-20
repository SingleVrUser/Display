using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Display.Models.Enums;
using Display.Models.Records;
using Display.Models.Vo;
using Display.Models.Vo.OneOneFive;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Controls.UserController;

public sealed partial class Explorer
{
    private static readonly DependencyProperty FileMenuFlyoutProperty =
        DependencyProperty.Register(nameof(FileMenuFlyout), typeof(MenuFlyout), typeof(Explorer), null);

    public MenuFlyout FileMenuFlyout
    {
        get => (MenuFlyout)GetValue(FileMenuFlyoutProperty);
        set => SetValue(FileMenuFlyoutProperty, value);
    }

    private ObservableCollection<ExplorerItem> TreeViewDataSource { get; }
    internal ObservableCollection<ExplorerItem> SelectFolderName { get; }
    private ObservableCollection<DetailFileInfo> FileInSelectFolder { get; }

    //存储获取过的Datum，避免重复获取
    private readonly List<StoreDatum> _storeDataList = [];

    private readonly IFilesInfoDao _filesInfoDao = App.GetService<IFilesInfoDao>();
    
    public Explorer()
    {
        InitializeComponent();

        TreeViewDataSource = GetRootFolder();
        foreach (var item in TreeViewDataSource)
        {
            var node = new TreeViewNode
            {
                Content = item,
                HasUnrealizedChildren = item.HasUnrealizedChildren,
                IsExpanded = item.IsExpanded
            };
            FolderTreeView.RootNodes.Add(node);
        }

        FileInSelectFolder = [];
        SelectFolderName = [];

        TryUpdateFolderInfo(0);
    }


    /// <summary>
    /// 获取当前Cid下的文件（文件和文件夹，一层）
    /// </summary>
    /// <param name="folderCid"></param>
    /// <param name="outType"></param>
    /// <returns></returns>
    public List<FilesInfo> GetFilesFromItems(long folderCid, FileType outType)
    {
        List<FilesInfo> items;
    
        //先从存储的List中获取
        var item = _storeDataList.FirstOrDefault(x => x.Cid == folderCid);
        if (item == null)
        {
            items = _filesInfoDao.GetPartFileListByPid(folderCid);

            //排序
            items = items.OrderByDescending(x => x.TimeEdit).ToList();

            _storeDataList.Add(new StoreDatum(folderCid, items));
        }
        else
        {
            items = item.DatumList;
        }

        if (outType == FileType.Folder)
        {
            items = items.Where(x => x.FileId == default).ToList();
        }

        return items;
    }

    //获取根目录下的文件夹
    private ObservableCollection<ExplorerItem> GetRootFolder()
    {
        var list = new ObservableCollection<ExplorerItem>();

        //位于根目录下的文件夹
        var data = GetFilesFromItems(0, FileType.Folder);
        if (data == null) return list;

        foreach (var item in data)
        {
            var hasUnrealizedChildren = _filesInfoDao.GetPartFolderListByPid(item.CurrentId, 1).Count != 0;

            var folders = new ExplorerItem
            {
                Name = item.Name,
                Type = FileType.Folder,
                Id = item.CurrentId,
                HasUnrealizedChildren = hasUnrealizedChildren,
                Datum = item
            };

            list.Add(folders);
        }

        return list;
    }

    /// <summary>
    /// 点击了TreeView选项
    /// </summary>
    private long _lastInvokedCid = -1;

    public event TypedEventHandler<TreeView, TreeViewItemInvokedEventArgs> ItemInvoked;
    private void TreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is not TreeViewNode { Content: ExplorerItem content }) return;
        
        var cid = content.Id;

        TryUpdateFolderInfo(cid);

        ItemInvoked?.Invoke(sender, args);

    }

    /// <summary>
    /// 根据cid更新右侧的信息（显示目录 和 文件详情）
    /// </summary>
    /// <param name="folderCid"></param>
    private void TryUpdateFolderInfo(long folderCid)
    {
        //避免重复点击
        if (folderCid == _lastInvokedCid) return;

        _lastInvokedCid = folderCid;
        
        var items = GetFilesFromItems(folderCid, FileType.File);

        //更新右侧文件夹目录
        SelectFolderName.Clear();
        TryUpdateFolder(folderCid);


        //更新右侧文件列表
        TryUpdateFileInSelectFolder(items);

    }

    /// <summary>
    /// 更新详细信息的目录
    /// </summary>
    /// <param name="folderCid"></param>
    private void TryUpdateFolder(long folderCid)
    {
        if (folderCid == 0)
        {
            SelectFolderName.Clear();
            SelectFolderName.Add(new ExplorerItem
            {
                Name = "根目录",
                Id = 0
            });
        }
        else
        {
            //从数据库中获取根目录信息
            var folderToRootList = _filesInfoDao.GetFolderListToRootByFolderId(folderCid);
            foreach (var info in folderToRootList)
            {
                SelectFolderName.Add(new ExplorerItem
                {
                    Name = info.Name,
                    Id = info.CurrentId
                });
            }
        }

    }

    /// <summary>
    /// 更新所选文件夹的文件列表
    /// </summary>
    /// <param name="items"></param>
    private void TryUpdateFileInSelectFolder(List<FilesInfo> items)
    {
        FileInSelectFolder.Clear();

        if (items == null) return;

        //排序
        items = items.OrderByDescending(x => x.ParentId).ToList();

        foreach (var file in items)
        {
            FileInSelectFolder.Add(new DetailFileInfo(file));
        }
    }

    /// <summary>
    /// TreeView 展开
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void TreeView_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
    {
        //标记为 内含未加载项
        if (!args.Node.HasUnrealizedChildren) return;
        var isNeedLoad = true;

        if (_markShowPartFolderItemList.Count > 0)
        {
            var currentCid = ((ExplorerItem)args.Node.Content).Id;
            foreach (var item in _markShowPartFolderItemList)
            {
                //找到之前未加载完成的记录，
                //即之前加载过了，无需重复加载
                if (((ExplorerItem)item.InsertNode.Content).Id != currentCid) continue;
                
                ShowNumTextBlock.Visibility = Visibility.Visible;
                ShowNumTip.Text = $"{item.ShowNum}/{item.LastFolderItem.Count}";
                _lastFolderItemList = item;
                isNeedLoad = false;
                break;
            }
        }

        if (isNeedLoad)
        {
            FillTreeNode(args.Node);
        }
    }

    private readonly List<LastUnAllShowFolderItem> _markShowPartFolderItemList = [];
    private LastUnAllShowFolderItem _lastFolderItemList;

    public Explorer(ObservableCollection<ExplorerItem> selectFolderName)
    {
        SelectFolderName = selectFolderName;
    }

    /// <summary>
    /// 填充之前TreeView未加载的子节点
    /// </summary>
    /// <param name="node"></param>
    /// <param name="maxNum"></param>
    /// <param name="isInsertLeft"></param>
    private void FillTreeNode(TreeViewNode node, int maxNum = 30, bool isInsertLeft = false)
    {
        if (node.Content is not ExplorerItem folder) return;

        List<FilesInfo> itemsList;

        if (isInsertLeft && _lastFolderItemList != null)
        {
            itemsList = _lastFolderItemList.LastFolderItem;
            itemsList = itemsList.GetRange(maxNum, maxNum);
            _markShowPartFolderItemList.Remove(_lastFolderItemList);
            _lastFolderItemList = null;
        }
        else
        {
            itemsList = GetFilesFromItems(folder.Id, FileType.Folder)?.ToList();
            //itemsList = DataAccessLocal.GetFolderListByPid(folder.Cid);
        }

        if (itemsList is not { Count: > 0 })
        {
            // The item is a folder, but it's empty. Leave HasUnrealizedChildren = true so
            // that the chevron appears, but don't try to process children that aren't there.
            return;
        }

        List<FilesInfo> itemsPartList;
        var hasUnrealizedChildren = false;

        // 显示部分
        if (itemsList.Count > maxNum)
        {
            ShowNumTextBlock.Visibility = Visibility.Visible;
            ShowNumTip.Text = $"{maxNum}/{itemsList.Count}";
            itemsPartList = itemsList.GetRange(0, maxNum);
            _lastFolderItemList = new LastUnAllShowFolderItem()
            {
                InsertNode = node,
                LastFolderItem = itemsList,
                ShowNum = maxNum
            };
            _markShowPartFolderItemList.Add(_lastFolderItemList);
            hasUnrealizedChildren = true;
        }
        // 数量较小，直接显示全部
        else
        {
            itemsPartList = itemsList;
            ShowNumTextBlock.Visibility = Visibility.Collapsed;
        }

        StartUpdateTreeView(node, itemsPartList);

        //标记 下一级是否有未加载的
        node.HasUnrealizedChildren = hasUnrealizedChildren;
    }

    private async void StartUpdateTreeView(TreeViewNode node, List<FilesInfo> itemsList)
    {
        ReadFileProgressBar.Maximum = itemsList.Count;
        ReadFileProgressBar.Value = 0;
        ReadFileProgressBar.Visibility = Visibility.Visible;

        var progress = new Progress<int>(progressPercent => ReadFileProgressBar.Value = progressPercent);

        ////排序
        //itemsList = itemsList.OrderBy(x => x.pid).ToList();

        //效率低
        var newNodeChildrenDict = await Task.Run(() => GetNewNode(itemsList, progress));

        foreach (var newNodeDict in newNodeChildrenDict)
        {
            var videoInfo = newNodeDict.Key;
            var newNode = new TreeViewNode
            {
                Content = new ExplorerItem
                {
                    Name = videoInfo.Name,
                    Type = FileType.Folder,
                    Id = videoInfo.CurrentId,
                    Datum = videoInfo
                },
                HasUnrealizedChildren = newNodeDict.Value,
            };
            node.Children.Add(newNode);
        }

        ReadFileProgressBar.Value = 0;
        ReadFileProgressBar.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// 放入 花时 较长的 查询目录是否有下级目录的操作
    /// </summary>
    /// <param name="itemsList"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    private Dictionary<FilesInfo, bool> GetNewNode(List<FilesInfo> itemsList, IProgress<int> progress)
    {
        var nodeHasUnrealizedChildrenDict = new Dictionary<FilesInfo, bool>();
        
        var i = 0;
        foreach (var folderInfo in itemsList)
        {
            i++;
            progress.Report(i);
            //检查下级是否还有文件夹
            var hasUnrealizedChildren = _filesInfoDao.GetPartFolderListByPid(folderInfo.CurrentId, 1).Count != 0;

            nodeHasUnrealizedChildrenDict.Add(folderInfo, hasUnrealizedChildren);

        }

        return nodeHasUnrealizedChildrenDict;
    }

    /// <summary>
    /// 全选
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
        FolderTreeView.SelectAll();
    }

    /// <summary>
    /// 清空选项
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
    {

        FolderTreeView.SelectedNodes.Clear();
    }

    /// <summary>
    /// 点击显示目录跳转指定位置
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    internal void FolderBreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        // Don't process last index (current location)
        if (args.Index >= SelectFolderName.Count - 1) return;
        
        // Home is special case.
        if (args.Index == 0)
        {
            TryUpdateFolderInfo(0);
        }
        // Go back to the clicked item.
        else
        {
            var item = (ExplorerItem)args.Item;

            //var data = DataAccessLocal.GetListByCid(item.Cid);
            //FileInSelectFolder.Clear();
            //foreach (var file_info in data)
            //{
            //    FileInSelectFolder.Add(file_info);
            //}

            // Remove breadcrumbs at the end until 
            // you get to the one that was clicked.
            while (SelectFolderName.Count > args.Index + 1)
            {
                SelectFolderName.RemoveAt(SelectFolderName.Count - 1);
                TryUpdateFolderInfo(item.Id);
            }
        }
    }

    private void Hyperlink_Click(Microsoft.UI.Xaml.Documents.Hyperlink sender, Microsoft.UI.Xaml.Documents.HyperlinkClickEventArgs args)
    {
        if (_lastFolderItemList == null) return;
        FillTreeNode(_lastFolderItemList.InsertNode, isInsertLeft: true);

        //成功
        ShowNumTextBlock.Visibility = Visibility.Collapsed;
    }

    public event ItemClickEventHandler ItemClick;

    //点击了详情页的列表
    private void FilesInfoListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not DetailFileInfo itemInfo) return;

        //文件夹
        if (itemInfo.Type == FileType.Folder)
        {
            if (itemInfo.Id == default) return;

            TryUpdateFolderInfo(itemInfo.Id);
        }

        ItemClick?.Invoke(sender, e);
    }

    //删除文件夹
    private async void DeletedCid_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: TreeViewNode { Content: ExplorerItem item } })
            return;


        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "确认",
            PrimaryButtonText = "删除",
            CloseButtonText = "返回",
            DefaultButton = ContentDialogButton.Close,
            Content = $"即将从本地数据库中删除 “{item.Name}”，确认删除？"
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await DeletedNodeAndDataAccessByCid(FolderTreeView.RootNodes, item.Id);

        }
    }

    private async Task DeletedNodeAndDataAccessByCid(IList<TreeViewNode> nodeChildren, long cid)
    {
        foreach (var node in nodeChildren)
        {
            if (((ExplorerItem)node.Content).Id == cid)
            {
                nodeChildren.Remove(node);
                
                await  _filesInfoDao.ExecuteRemoveAllByFolderIdAsync(cid);
                return;
            }
            var treeViewNodes = node.Children;
            if (treeViewNodes.Count != 0)
            {
                await DeletedNodeAndDataAccessByCid(treeViewNodes, cid);
            }
        }

    }

    public event RoutedEventHandler PlayVideoClick;
    private void PlayVideoButton_Click(object sender, RoutedEventArgs e)
    {
        PlayVideoClick?.Invoke(sender, e);
    }

    public event RoutedEventHandler PlayWithPlayerClick;
    private void PlayWithPlayerButtonClick(object sender, RoutedEventArgs e)
    {
        PlayWithPlayerClick?.Invoke(sender, e);
    }

}

public class LastUnAllShowFolderItem
{
    public TreeViewNode InsertNode { get; init; }
    public List<FilesInfo> LastFolderItem { get; init; }
    public int ShowNum { get; init; }
}

/// <summary>
/// TreeView样式选择
/// </summary>
public class ExplorerItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate FolderTemplate { get; set; }
    public DataTemplate FileTemplate { get; set; }

    //官方代码
    protected override DataTemplate SelectTemplateCore(object item)
    {
        var explorerItem = (ExplorerItem)((TreeViewNode)item).Content;
        return explorerItem.Type == FileType.Folder ? FolderTemplate : FileTemplate;
    }
}
