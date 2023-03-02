using Data;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Display.Models;

public class IncrementallLoadDatumCollection : ObservableCollection<FilesInfo>, ISupportIncrementalLoading
{
    private WebApi webApi { get; set; }

    public bool HasMoreItems { get; private set; } = true;

    private int _allCount;
    public int AllCount
    {
        get
        {
            return this._allCount;
        }
        private set
        {
            if (this._allCount == value) return;

            this._allCount = value;

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(AllCount)));
        }    
    }

    public WebApi.OrderBy orderby { get;private set; } = WebApi.OrderBy.user_ptime;

    public int asc { get;private set; }

    public string cid { get; set; }

    public IncrementallLoadDatumCollection(string cid)
    {
        this.cid = cid;
        webApi= new WebApi();

    }

    public async Task<WebFileInfo> GetFilesInfoAsync(int limit, int offset)
    {
        //查询该目录的排列方式
        var filesShowMethod = await webApi.GetFilesShowInfo(cid);

        WebApi.OrderBy order;
        Enum.TryParse(filesShowMethod.order, out order);
        if (this.orderby != order) this.orderby = order;

        this.asc = filesShowMethod.is_asc;

        var FilesInfo = await webApi.GetFileAsync(cid, limit, offset, orderBy: orderby, asc: asc);

        //汇报事件
        GetFileInfoCompletedEventArgs args = new() { orderby = this.orderby , asc = this.asc, TimeReached = DateTime.Now };
        EventHandler<GetFileInfoCompletedEventArgs> handler = GetFileInfoCompleted;
        if (handler != null)
        {
            handler(this, args);
        }

        return FilesInfo;
    }

    public async Task<int> LoadData(int limit = 40,int offset=0)
    {
        var FilesInfo = await GetFilesInfoAsync(limit,offset);
        if (AllCount!= FilesInfo.count) AllCount = FilesInfo.count;

        if (FilesInfo.data == null)
        {
            HasMoreItems = false;
            return 0;
        }

        foreach (var datum in FilesInfo.data)
        {
            Add(new(datum));
        }

        if (AllCount <= Count)
        {
            HasMoreItems = false;
        }

        return FilesInfo.data.Length;
    }

    
    public async Task SetCid(string cid)
    {
        this.cid = cid;
        Clear();

        //当目录为空时，HasMoreItems==True无法激活查询服务，需要手动LoadData
        bool isNeedLoad = Count == 0;
        HasMoreItems = !isNeedLoad;

        if (isNeedLoad) await LoadData();
    }

    public async Task SetOrder(WebApi.OrderBy orderBy,int asc)
    {
        this.orderby= orderBy;
        this.asc = asc;
        await webApi.ChangedShowType(cid,orderBy, asc);
        await SetCid(cid);
    }

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return InnerLoadMoreItemsAsync(count).AsAsyncOperation();
    }

    private int defaultCount = 40;
    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint count)
    {
        int getCount = HasMoreItems ? await LoadData(defaultCount, Count) : 0;

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
