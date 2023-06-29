
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Display.Data;
using Display.Helper;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Input;

namespace Display.Controls
{
    public sealed partial class Explorer : UserControl
    {
        ObservableCollection<ExplorerItem> TreeViewDataSource;
        ObservableCollection<ExplorerItem> SelectFolderName;
        ObservableCollection<FilesInfo> FileInSelectFolder;

        public static readonly DependencyProperty FileMenuFlyoutProperty =
            DependencyProperty.Register(nameof(FileMenuFlyout), typeof(MenuFlyout), typeof(Explorer), null);

        public MenuFlyout FileMenuFlyout
        {
            get => (MenuFlyout)GetValue(FileMenuFlyoutProperty);
            set => SetValue(FileMenuFlyoutProperty, value);
        }

        //存储获取过的Datum，避免重复获取
        List<StoreDatum> StoreDataList = new();

        public Explorer()
        {
            InitializeComponent();

            TreeViewDataSource = GetRootFolder();
            foreach (var item in TreeViewDataSource)
            {
                var node = new TreeViewNode()
                {
                    Content = item,
                    HasUnrealizedChildren = item.HasUnrealizedChildren,
                    IsExpanded = item.IsExpanded
                };
                FolderTreeView.RootNodes.Add(node);
            }

            FileInSelectFolder = new ObservableCollection<FilesInfo>();
            SelectFolderName = new ObservableCollection<ExplorerItem>();

            TryUpdateFolderInfo(0);
        }


        /// <summary>
        /// 获取当前Cid下的文件（文件和文件夹，一层）
        /// </summary>
        /// <param name="folderCid"></param>
        /// <param name="outType"></param>
        /// <returns></returns>
        public Datum[] GetFilesFromItems(long folderCid, FilesInfo.FileType outType)
        {
            Datum[] items;

            //先从存储的List中获取
            var item = StoreDataList.FirstOrDefault(x => x.Cid == folderCid);
            if (item == null)
            {
                items = DataAccess.Get.GetListByCid(folderCid);

                //排序
                items = items.OrderByDescending(x => x.TimeEdit).ToArray();

                StoreDataList.Add(new StoreDatum()
                {
                    Cid = folderCid,
                    DatumList = items
                });
            }
            else
            {
                items = item.DatumList;
            }

            if (outType == FilesInfo.FileType.Folder)
            {
                items = items.Where(x => x.Fid == null).ToArray();
            }

            return items;
        }

        //获取根目录下的文件夹
        public ObservableCollection<ExplorerItem> GetRootFolder()
        {
            var list = new ObservableCollection<ExplorerItem>();

            //位于根目录下的文件夹
            var data = GetFilesFromItems(0, FilesInfo.FileType.Folder);

            foreach (var item in data)
            {
                var hasUnrealizedChildren = DataAccess.Get.GetFolderListByPid(item.Cid, 1).Length != 0;

                var folders = new ExplorerItem()
                {
                    Name = item.Name,
                    Type = FilesInfo.FileType.Folder,
                    Id = item.Cid,
                    HasUnrealizedChildren = hasUnrealizedChildren,
                    datum = item
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
            var content = ((args.InvokedItem as TreeViewNode).Content as ExplorerItem);
            var cid = content.Id;

            //避免重复点击
            if (cid == _lastInvokedCid) return;

            _lastInvokedCid = cid;
            TryUpdateFolderInfo(cid);

            ItemInvoked?.Invoke(sender, args);

        }

        /// <summary>
        /// 根据cid更新右侧的信息（显示目录 和 文件详情）
        /// </summary>
        /// <param name="folderCid"></param>
        private void TryUpdateFolderInfo(long folderCid)
        {
            var items = GetFilesFromItems(folderCid, FilesInfo.FileType.File);

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
                SelectFolderName.Add(new ExplorerItem()
                {
                    Name = "根目录",
                    Id = 0
                });
            }
            else
            {
                //从数据库中获取根目录信息
                List<Datum> folderToRootList = DataAccess.Get.GetRootByCid(folderCid);
                foreach (var info in folderToRootList)
                {
                    SelectFolderName.Add(new ExplorerItem()
                    {
                        Name = info.Name,
                        Id = info.Cid
                    });
                }
            }

        }

        /// <summary>
        /// 通过文件Cid获取根目录的Node
        /// </summary>
        /// <param Name="rootNodes"></param>
        /// <param Name="folderCid"></param>
        /// <returns></returns>
        private TreeViewNode getNodeByRootNodeWithCid(List<TreeViewNode> rootNodes, long folderCid)
        {
            //不存在Cid未零的Node
            if (folderCid == 0) return null;

            TreeViewNode targertNode = null;

            var childrenNode = rootNodes;

            while (targertNode == null && childrenNode.Count != 0)
            {
                List<TreeViewNode> tmpChildrenNode = new();
                foreach (TreeViewNode node in childrenNode)
                {
                    //Content
                    if (((ExplorerItem)node.Content).Id == folderCid)
                    {
                        targertNode = node;
                        break;
                    }
                    //Children

                    foreach (var cnode in node.Children)
                    {
                        tmpChildrenNode.Add(cnode);
                    }
                }
                childrenNode = tmpChildrenNode;
            }

            return targertNode;
        }

        /// <summary>
        /// 更新所选文件夹的文件列表
        /// </summary>
        /// <param name="items"></param>
        private void TryUpdateFileInSelectFolder(Datum[] items)
        {
            FileInSelectFolder.Clear();

            //排序
            items = items.OrderByDescending(x => x.Pid).ToArray();

            foreach (var file in items)
            {
                FileInSelectFolder.Add(new FilesInfo(file));
            }
        }

        /// <summary>
        /// TreeView 展开
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="args"></param>
        private void TreeView_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
        {
            //标记为 内含未加载项
            if (args.Node.HasUnrealizedChildren)
            {
                bool isNeedLoad = true;

                if (_markShowPartFolderItemList.Count > 0)
                {
                    var currentCid = (args.Node.Content as ExplorerItem).Id;
                    foreach (var item in _markShowPartFolderItemList)
                    {
                        //找到之前未加载完成的记录，
                        //即之前加载过了，无需重复加载
                        if ((item.InsertNode.Content as ExplorerItem).Id == currentCid)
                        {
                            ShowNumTextBlock.Visibility = Visibility.Visible;
                            ShowNumTip.Text = $"{item.ShowNum}/{item.LastFolderItem.Count}";
                            _lastFolderItemList = item;
                            isNeedLoad = false;
                            break;
                        }
                    }
                }

                if (isNeedLoad)
                {
                    FillTreeNode(args.Node);
                }
            }
        }

        private List<lastUnAllShowFolderItem> _markShowPartFolderItemList = new();
        private lastUnAllShowFolderItem _lastFolderItemList;


        /// <summary>
        /// 填充之前TreeView未加载的子节点
        /// </summary>
        /// <param Name="node"></param>
        /// <param Name="MaxNum"></param>
        private void FillTreeNode(TreeViewNode node, int MaxNum = 30, bool isInsertLeft = false)
        {
            if (node.Content is not ExplorerItem folder) return;

            List<Datum> itemsList;

            if (isInsertLeft && _lastFolderItemList != null)
            {
                itemsList = _lastFolderItemList.LastFolderItem;
                itemsList = itemsList.GetRange(MaxNum, MaxNum);
                _markShowPartFolderItemList.Remove(_lastFolderItemList);
                _lastFolderItemList = null;
            }
            else
            {
                itemsList = GetFilesFromItems(folder.Id, FilesInfo.FileType.Folder).ToList();
                //itemsList = DataAccess.GetFolderListByPid(folder.Cid);
            }

            if (itemsList.Count == 0)
            {
                // The item is a folder, but it's empty. Leave HasUnrealizedChildren = true so
                // that the chevron appears, but don't try to process children that aren't there.
                return;
            }

            List<Datum> itemspartList;
            bool hasUnrealizedChildren = false;

            // 显示部分
            if (itemsList.Count > MaxNum)
            {
                ShowNumTextBlock.Visibility = Visibility.Visible;
                ShowNumTip.Text = $"{MaxNum}/{itemsList.Count}";
                itemspartList = itemsList.GetRange(0, MaxNum);
                _lastFolderItemList = new lastUnAllShowFolderItem()
                {
                    InsertNode = node,
                    LastFolderItem = itemsList,
                    Count = itemsList.Count,
                    ShowNum = MaxNum
                };
                _markShowPartFolderItemList.Add(_lastFolderItemList);
                hasUnrealizedChildren = true;
            }
            // 数量较小，直接显示全部
            else
            {
                itemspartList = itemsList;
                ShowNumTextBlock.Visibility = Visibility.Collapsed;
            }

            startUpdateTreeView(node, itemspartList);

            //标记 下一级是否有未加载的
            node.HasUnrealizedChildren = hasUnrealizedChildren;
        }

        private async void startUpdateTreeView(TreeViewNode node, List<Datum> itemsList)
        {
            readFileProgressBar.Maximum = itemsList.Count;
            readFileProgressBar.Value = 0;
            readFileProgressBar.Visibility = Visibility.Visible;

            var progress = new Progress<int>(progressPercent => readFileProgressBar.Value = progressPercent);

            ////排序
            //itemsList = itemsList.OrderBy(x => x.pid).ToList();

            //效率低
            Dictionary<Datum, bool> newNode_Children_Dict = await Task.Run(() => getNewNode(itemsList, progress));

            foreach (var newNodeDict in newNode_Children_Dict)
            {
                var videoInfo = newNodeDict.Key;
                var newNode = new TreeViewNode()
                {
                    Content = new ExplorerItem()
                    {
                        Name = videoInfo.Name,
                        Type = FilesInfo.FileType.Folder,
                        Id = videoInfo.Cid,
                        datum = videoInfo
                    },
                    HasUnrealizedChildren = newNodeDict.Value,
                };
                node.Children.Add(newNode);
            }

            //await Task.Run(() => {
            //    var newdfNode = new TreeViewNode();
            //});


            readFileProgressBar.Value = 0;
            readFileProgressBar.Visibility = Visibility.Collapsed;
        }

        //private List<TreeViewNode> createTreeViewNode()
        //{
        //    var result = new List<TreeViewNode>();
        //    for (int i = 0; i < 10; i++)
        //    {
        //        var node = new TreeViewNode();
        //        result.Add(node);
        //    }

        //    return result;
        //}

        /// <summary>
        /// 放入 花时 较长的 查询目录是否有下级目录的操作
        /// </summary>
        /// <param Name="itemsList"></param>
        /// <param Name="progress"></param>
        /// <returns></returns>
        private Dictionary<Datum, bool> getNewNode(List<Datum> itemsList, IProgress<int> progress)
        {
            var Node_HasUnrealizedChildren_Dict = new Dictionary<Datum, bool>();
            //var NodeList = new List<TreeViewNode>();
            int i = 0;
            foreach (var folderInfo in itemsList)
            {
                i++;
                progress.Report(i);
                //检查下级是否还有文件夹
                bool hasUnrealizedChildren = DataAccess.Get.GetFolderListByPid(folderInfo.Cid, 1).Length != 0;

                Node_HasUnrealizedChildren_Dict.Add(folderInfo, hasUnrealizedChildren);

                //var newNode = new TreeViewNode()
                //{
                //    Content = new ExplorerItem()
                //    {
                //        Name = folderInfo.n,
                //        Type = ExplorerItem.ExplorerItemType.Folder,
                //        Cid = folderInfo.cid,
                //    },
                //    HasUnrealizedChildren = hasUnrealizedChildren,
                //};

                //newNode.HasUnrealizedChildren = hasUnrealizedChildren;

                //NodeList.Add(newNode);
            }

            return Node_HasUnrealizedChildren_Dict;
        }

        /// <summary>
        /// 全选
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FolderTreeView.SelectAll();
        }

        /// <summary>
        /// 清空选项
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

            FolderTreeView.SelectedNodes.Clear();
        }

        /// <summary>
        /// 点击显示目录跳转指定位置
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="args"></param>
        private void FolderBreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            // Don't process last index (current location)
            if (args.Index < SelectFolderName.Count - 1)
            {
                // Home is special case.
                if (args.Index == 0)
                {
                    TryUpdateFolderInfo(0);
                }
                // Go back to the clicked item.
                else
                {
                    var item = (ExplorerItem)args.Item;

                    //var data = DataAccess.GetListByCid(item.Cid);
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
            if (e.ClickedItem is not FilesInfo itemInfo) return;

            //文件夹
            if (itemInfo.Type == FilesInfo.FileType.Folder)
            {
                if (itemInfo.Id == null) return;

                TryUpdateFolderInfo((long)itemInfo.Id);
            }

            ItemClick?.Invoke(sender, e);
        }

        //删除文件夹
        private async void DeletedCid_Click(object sender, RoutedEventArgs e)
        {
            var item = ((sender as MenuFlyoutItem).DataContext as TreeViewNode).Content as ExplorerItem;

            var dialog = new ContentDialog()
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
                DeletedNodeAndDataAccessByCid(FolderTreeView.RootNodes, item.Id);

            }
        }

        private static void DeletedNodeAndDataAccessByCid(IList<TreeViewNode> nodeChildren, long cid, SqliteConnection connection = null)
        {
            var isNeedCloseConnection = connection == null;
            if (connection == null)
            {
                connection = new SqliteConnection(DataAccess.ConnectionString);
                connection.Open();
            }   

            foreach (var node in nodeChildren)
            {
                if (((ExplorerItem)node.Content).Id == cid)
                {
                    nodeChildren.Remove(node);
                    DataAccess.Delete.DeleteAllDirectoryAndFiles_InFilesInfoTable(cid, connection);
                    return;
                }
                var treeViewNodes = node.Children;
                if (treeViewNodes.Count != 0)
                {
                    DeletedNodeAndDataAccessByCid(treeViewNodes, cid ,connection);
                }
            }

            if (!isNeedCloseConnection) return;

            connection.Close();
            connection.Dispose();
        }

        public event RoutedEventHandler PlayVideoClick;
        private void PlayVideoButton_Click(object sender, RoutedEventArgs e)
        {

            PlayVideoClick?.Invoke(sender, e);
        }

        public event RoutedEventHandler PlayWithPlayerClick;
        private void PlayWithPlayerButtonClick(object sender, RoutedEventArgs e)
        {
            PlayWithPlayerClick?.Invoke(sender,e);
        }
        
    }

    public class lastUnAllShowFolderItem
    {
        public TreeViewNode InsertNode { get; set; }
        public List<Datum> LastFolderItem { get; set; }
        public int Count { get; set; }
        public int ShowNum { get; set; }
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
            var explorerItem = (ExplorerItem)(item as TreeViewNode).Content;
            if (explorerItem.Type == FilesInfo.FileType.Folder) return FolderTemplate;

            return FileTemplate;
        }
    }

    /// <summary>
    /// ListView样式选择
    /// </summary>
    public class FileInfoItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FolderTemplate { get; set; }
        public DataTemplate FileTemplate { get; set; }


        //参照微软TreeView代码编写，实际使用中样式选择有误
        //protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        //{
        //    var explorerItem = (FilesInfo)item;
        //    if (explorerItem.Type == FilesInfo.FileType.Folder) return FolderTemplate;

        //    return FileTemplate;
        //}


        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var explorerItem = (FilesInfo)item;
            if (explorerItem.Type == FilesInfo.FileType.Folder) return FolderTemplate;

            return FileTemplate;
        }
    }

}
