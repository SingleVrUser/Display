
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Display.Data;

namespace Display.Models;

public class IncrementalLoadFailDatumInfoCollection : ObservableCollection<Datum>, ISupportIncrementalLoading
{
    public IncrementalLoadFailDatumInfoCollection()
    {

    }

    public async Task LoadData(int startShowCount = 20)
    {
        var newItems = await DataAccess.LoadFailFileInfoWithDatum(0, startShowCount, filterName, OrderBy, IsDesc, ShowType);

        if (Count == 0)
        {
            HasMoreItems = true;
            this.AllCount = DataAccess.CheckFailDatumFilesCount(filterName, ShowType);
        }
        else
            Clear();


        newItems.ForEach(item => Add(item));
    }

    public void SetShowType(FailType ShowType)
    {
        this.ShowType = ShowType;
    }

    public FailType ShowType { get; set; } = FailType.All;

    public string filterName { get; set; } = string.Empty;

    public int AllCount { get; private set; }

    public bool HasMoreItems { get; set; } = true;

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return InnerLoadMoreItemsAsync(count).AsAsyncOperation();
    }
    public void SetFilter(string filterKeywords)
    {
        this.filterName = filterKeywords;
    }
    public void SetOrder(string orderBy, bool isDesc)
    {
        this.OrderBy = orderBy;
        this.IsDesc = isDesc;
    }

    public string OrderBy { get; set; }
    public bool IsDesc { get; set; }

    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint count)
    {
        var failLists = await DataAccess.LoadFailFileInfoWithDatum(Items.Count, (int)count, filterName, OrderBy, IsDesc, ShowType);

        if (failLists.Count < count)
        {
            HasMoreItems = false;
        }

        failLists.ForEach(item => Add(item));

        return new LoadMoreItemsResult
        {
            Count = (uint)failLists.Count
        };
    }
}
