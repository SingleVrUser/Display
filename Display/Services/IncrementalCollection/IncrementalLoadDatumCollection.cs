﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Display.Extensions;
using Display.Models.Data;
using Microsoft.UI.Xaml.Data;
using SharpCompress;

namespace Display.Services.IncrementalCollection;

public class IncrementalLoadDatumCollection : ObservableCollection<FilesInfo>, ISupportIncrementalLoading
{
    private WebApi WebApi { get; }

    private bool _isBusy;

    public bool HasMoreItems { get; private set; } = true;

    private CancellationToken _token = CancellationToken.None;

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

    private bool _isNull;

    public bool IsNull
    {
        get => _isNull;
        set
        {
            if (_isNull == value) return;

            _isNull = value;

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsNull)));
        }
    }

    public new void Insert(int index, FilesInfo item)
    {
        base.Insert(index, item);
        AllCount++;

        if (IsNull) IsNull = false;
    }

    public void AddArray(FilesInfo[] items)
    {
        items.ForEach(Add);
        AllCount += items.Length;

        if (IsNull) IsNull = false;
    }

    public new void Remove(FilesInfo item)
    {
        base.Remove(item);
        AllCount--;

        if (AllCount == 0) IsNull = true;
    }

    public WebApi.OrderBy OrderBy { get; private set; } = WebApi.OrderBy.UserProduceTime;

    public int Asc { get; private set; }

    public long Cid { get; set; }

    private bool IsOnlyFolder { get; set; }

    public IncrementalLoadDatumCollection(long cid, bool isOnlyFolder = false)
    {
        Cid = cid;
        WebApi = WebApi.GlobalWebApi;
        IsOnlyFolder = isOnlyFolder;
    }


    private WebPath[] _webPaths;

    public WebPath[] WebPaths
    {
        get => _webPaths;
        set
        {
            if(_webPaths == value) return;

            _webPaths = value;

            WebPathChanged?.Invoke(_webPaths);
        }
    }

    public async Task<WebFileInfo> GetFilesInfoAsync(int limit, int offset)
    {
        Debug.WriteLine($"从网络中获取文件列表:{offset}-{limit + offset}");
        var filesInfo = await WebApi.GetFileAsync(Cid, limit, offset, orderBy: OrderBy.GetDescription(), asc: Asc, isOnlyFolder: IsOnlyFolder);
        if(filesInfo == null) return null;

        WebPaths = filesInfo.path;
        AllCount = filesInfo.count;

        //查询该目录的排列方式
        var order = filesInfo.order.ParseEnumByDescription<WebApi.OrderBy>();
        if (OrderBy != order) OrderBy = order;
        Asc = filesInfo.is_asc;

        //汇报事件
        GetFileInfoCompletedEventArgs args = new() { Orderby = OrderBy, Asc = Asc, TimeReached = DateTime.Now };
        GetFileInfoCompleted?.Invoke(this, args);

        return filesInfo;
    }

    public async Task<int> LoadData(int limit = 40, int offset = 0)
    {
        IsNull = false;

        var filesInfo = await GetFilesInfoAsync(limit, offset);

        // 访问失败，没有登陆
        // 或者没有数据
        if (filesInfo?.data == null || AllCount == 0)
        {
            IsNull = true;
            HasMoreItems = false;
            return 0;
        }

        filesInfo.data.ForEach(i=>Add(new FilesInfo(i)));
        
        HasMoreItems = AllCount > Count;
        Debug.WriteLine($"[{Count}/{AllCount}] HasMoreItems = {HasMoreItems}");

        return filesInfo.data.Length;
    }

    public async Task SetCid(long cid)
    {
        if (_isBusy) return;

        // 两个进程同时执行Clear操作会闪退
        _isBusy = true;

        Cid = cid;

        // 避免使用LoadMoreItemsAsync重复加载数据，LoadData后会重新设置为正确的值
        HasMoreItems = false;

        Clear();

        //当目录为空时，HasMoreItems==True无法激活查询服务，需要手动LoadData
        //var isNeedLoad = Count == 0;
        //HasMoreItems = true;
        //HasMoreItems = !isNeedLoad;

        await LoadData();

        _isBusy = false;
    }

    public async Task SetOrder(WebApi.OrderBy orderBy, int asc)
    {
        OrderBy = orderBy;
        Asc = asc;
        await WebApi.ChangedShowType(Cid, orderBy, asc);
        await SetCid(Cid);
    }

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint _)
    {
        return InnerLoadMoreItemsAsync().AsAsyncOperation();
    }

    private readonly int _defaultCount = 30;
    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync()
    {
        var getCount = HasMoreItems ? await LoadData(_defaultCount, Count) : 0;

        return new LoadMoreItemsResult
        {
            Count = (uint)getCount
        };
    }

    public event EventHandler<GetFileInfoCompletedEventArgs> GetFileInfoCompleted;
    public event Action<WebPath[]> WebPathChanged;
}

public class GetFileInfoCompletedEventArgs : EventArgs
{
    public WebApi.OrderBy Orderby { get; set; }

    public int Asc { get; set; }

    public DateTime TimeReached { get; set; }
}
