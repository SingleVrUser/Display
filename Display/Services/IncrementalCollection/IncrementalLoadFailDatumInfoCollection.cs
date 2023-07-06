
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Display.Data;
using Display.Models;
using SharpCompress;

namespace Display.Services.IncrementalCollection;

public class IncrementalLoadFailDatumInfoCollection : ObservableCollection<Datum>, ISupportIncrementalLoading
{
    public IncrementalLoadFailDatumInfoCollection()
    {

    }

    public async Task LoadData(int startShowCount = 20)
    {
        var newItems = await DataAccess.Get.GetFailFileInfoWithDatum(0, startShowCount, filterName, OrderBy, IsDesc, ShowType);

        if (Count == 0)
        {
            HasMoreItems = true;
            AllCount = DataAccess.Get.GetCountOfFailDatumFiles(filterName, ShowType);
        }
        else
            Clear();


        newItems.ForEach(Add);
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
        filterName = filterKeywords;
    }
    public void SetOrder(string orderBy, bool isDesc)
    {
        OrderBy = orderBy;
        IsDesc = isDesc;
    }

    public string OrderBy { get; set; }
    public bool IsDesc { get; set; }

    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint count)
    {
        var failLists = await DataAccess.Get.GetFailFileInfoWithDatum(Items.Count, (int)count, filterName, OrderBy, IsDesc, ShowType);

        if (failLists.Length < count)
        {
            HasMoreItems = false;
        }

        failLists.ForEach(Add);

        return new LoadMoreItemsResult
        {
            Count = (uint)failLists.Length
        };
    }
}
