using Data;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Display.Model;

public class IncrementallLoadDatumCollection : ObservableCollection<FilesInfo>, ISupportIncrementalLoading
{
    private WebApi webApi { get; set; }

    public bool HasMoreItems { get; private set; } = true;

    public int AllCount { get;private set; }

    public WebApi.OrderBy orderby { get;private set; } = WebApi.OrderBy.user_ptime;

    public int asc { get;private set; }

    private string cid { get; set; }

    public IncrementallLoadDatumCollection(string cid)
    {
        this.cid = cid;
        webApi= new WebApi();

    }

    public async Task<WebFileInfo> GetFilesInfoAsync(int limit, int offset)
    {
        var FilesInfo = await webApi.GetFileAsync(cid, limit, offset, orderBy: orderby, asc: asc);

        this.asc = FilesInfo.is_asc;

        WebApi.OrderBy order;
        Enum.TryParse(FilesInfo.order, out order);
        if (this.orderby != order) this.orderby = order;

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

    
    public void SetCid(string cid)
    {
        this.cid = cid;
        Clear();
        HasMoreItems = true;
    }

    public async Task SetOrder(WebApi.OrderBy orderBy,int asc)
    {
        this.orderby= orderBy;
        this.asc = asc;
        await webApi.ChangedShowType(cid,orderBy, asc);
        SetCid(cid);
    }

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return InnerLoadMoreItemsAsync(count).AsAsyncOperation();
    }

    private int defaultCount = 40;
    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint count)
    {
        int getCount = await LoadData(defaultCount, Count);

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
