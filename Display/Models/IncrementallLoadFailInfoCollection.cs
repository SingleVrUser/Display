﻿
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Display.Data;

namespace Display.Models;

public class IncrementallLoadFailInfoCollection : ObservableCollection<FailInfo>, ISupportIncrementalLoading
{
    public bool HasMoreItems { get; set; } = true;

    public int AllCount { get; private set; }

    public FailInfoShowType ShowType { get; private set; }

    public IncrementallLoadFailInfoCollection(FailInfoShowType showType)
    {
        ShowType = showType;
    }

    public async void SetShowType(FailInfoShowType showType)
    {
        ShowType = showType;

        if (!HasMoreItems) HasMoreItems = true;

        Clear();

        await LoadData();
    }

    public async Task<int> LoadData(int limit = 20, int offset = 0)
    {
        var Infos = await DataAccess.LoadFailFileInfoWithFailInfo(offset, limit, ShowType);

        if (Count == 0)
            AllCount = DataAccess.CheckFailInfosCount(ShowType);

        Infos.ForEach(item => Add(item));

        var getCount = Infos.Count;

        if (AllCount <= Count || getCount == 0)
        {
            HasMoreItems = false;
        }

        return getCount;
    }

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return InnerLoadMoreItemsAsync(count).AsAsyncOperation();
    }

    private int defaultCount = 20;
    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint count)
    {
        int getCount = await LoadData(defaultCount, Count);

        return new LoadMoreItemsResult
        {
            Count = (uint)getCount
        };
    }
}