
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Display.Data;
using OpenCvSharp.Flann;

namespace Display.Models;

public class IncrementalLoadDatumCollection : ObservableCollection<FilesInfo>, ISupportIncrementalLoading
{
    private WebApi webApi { get; set; }

    public bool HasMoreItems { get; private set; } = true;

    private int _allCount;
    public int AllCount
    {
        get => _allCount;
        private set
        {
            if (_allCount == value) return;

            _allCount = value;

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(AllCount)));
        }
    }

    public new void Insert(int index, FilesInfo item)
    {
        //base.InsertItem(index, item);
        base.Insert(index, item);
        AllCount++;
    }

    public new void Remove(FilesInfo item)
    {
        //base.RemoveItem(item);
        base.Remove(item);
        AllCount--;
    }


    public WebApi.OrderBy orderby { get; private set; } = WebApi.OrderBy.UserPtime;

    public int asc { get; private set; }

    public long Cid { get; set; }

    private bool IsOnlyFolder { get; set; }

    public IncrementalLoadDatumCollection(long cid,bool isOnlyFolder = false)
    {
        Cid = cid;
        webApi = WebApi.GlobalWebApi;
        IsOnlyFolder = isOnlyFolder;
    }

    public WebPath[] WebPaths;

    public async Task<WebFileInfo> GetFilesInfoAsync(int limit, int offset)
    {
        //查询该目录的排列方式
        var filesShowMethod = await webApi.GetFilesShowInfo(Cid);

        WebApi.OrderBy order;
        Enum.TryParse(filesShowMethod.order, out order);
        if (this.orderby != order) this.orderby = order;

        this.asc = filesShowMethod.is_asc;

        var filesInfo = await webApi.GetFileAsync(Cid, limit, offset, orderBy: orderby, asc: asc,isOnlyFolder:IsOnlyFolder);
        WebPaths = filesInfo.path;

        //汇报事件
        GetFileInfoCompletedEventArgs args = new() { orderby = this.orderby, asc = this.asc, TimeReached = DateTime.Now };
        GetFileInfoCompleted?.Invoke(this, args);

        return filesInfo;
    }

    public async Task<int> LoadData(int limit = 40, int offset = 0)
    {
        var filesInfo = await GetFilesInfoAsync(limit, offset);

        AllCount = filesInfo.count;

        if (filesInfo.data == null)
        {
            HasMoreItems = false;
            return 0;
        }

        foreach (var datum in filesInfo.data)
        {
            Add(new FilesInfo(datum));
        }

        HasMoreItems = AllCount > Count;

        return filesInfo.data.Length;
    }


    public async Task SetCid(long cid)
    {
        Cid = cid;
        Clear();

        //当目录为空时，HasMoreItems==True无法激活查询服务，需要手动LoadData
        bool isNeedLoad = Count == 0;
        HasMoreItems = !isNeedLoad;

        if (isNeedLoad) await LoadData();
    }

    public async Task SetOrder(WebApi.OrderBy orderBy, int asc)
    {
        this.orderby = orderBy;
        this.asc = asc;
        await webApi.ChangedShowType(Cid, orderBy, asc);
        await SetCid(Cid);
    }

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return InnerLoadMoreItemsAsync(count).AsAsyncOperation();
    }

    private readonly int _defaultCount = 30;
    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint count)
    {
        int getCount = HasMoreItems ? await LoadData(_defaultCount, Count) : 0;

        return new LoadMoreItemsResult
        {
            Count = (uint)getCount
        };
    }

    public event EventHandler<GetFileInfoCompletedEventArgs> GetFileInfoCompleted;
}

public class GetFileInfoCompletedEventArgs : EventArgs
{
    public WebApi.OrderBy orderby { get; set; }

    public int asc { get; set; }

    public DateTime TimeReached { get; set; }
}
