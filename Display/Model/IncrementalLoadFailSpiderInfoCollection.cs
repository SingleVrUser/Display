using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using Data;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Display.Model;

public class IncrementalLoadFailSpiderInfoCollection : ObservableCollection<FailDatum>, ISupportIncrementalLoading
{
    public async Task LoadData(int startShowCount = 20)
    {
        var newItems = await DataAccess.LoadFailFileInfo(0, startShowCount,showType: ShowType);

        if (Count == 0)
        {
            HasMoreItems = true;
            this.AllCount = DataAccess.CheckFailFilesCount(showType:  ShowType);
        }
        else
            Clear();

        newItems.ForEach(item => Add(new(item)));
    }

    public void SetShowType(FailType ShowType)
    {
        this.ShowType = ShowType;
    }

    public FailType ShowType { get; set; } = FailType.All;

    public int AllCount { get; private set; }

    public bool HasMoreItems { get; set; } = true;

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return InnerLoadMoreItemsAsync(count).AsAsyncOperation();
    }

    public string OrderBy { get; set; }
    public bool IsDesc { get; set; }

    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint count)
    {
        var failLists = await DataAccess.LoadFailFileInfo(Items.Count, (int)count, showType: ShowType);

        if (failLists.Count < count)
        {
            HasMoreItems = false;
        }

        failLists.ForEach(item => Add(new(item)));

        return new LoadMoreItemsResult
        {
            Count = (uint)failLists.Count
        };
    }
}
