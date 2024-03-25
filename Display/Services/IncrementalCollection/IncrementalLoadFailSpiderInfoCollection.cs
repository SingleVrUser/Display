using Display.Models.Data;
using Microsoft.UI.Xaml.Data;
using SharpCompress;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Display.Services.IncrementalCollection;

public class IncrementalLoadFailSpiderInfoCollection : ObservableCollection<FailDatum>, ISupportIncrementalLoading
{
    public async Task LoadData(int startShowCount = 20)
    {
        var newItems = await DataAccess.Get.GetFailFileInfoWithDatum(0, startShowCount, showType: ShowType);

        if (Count == 0)
        {
            HasMoreItems = true;
            AllCount = DataAccess.Get.GetCountOfFailDatumFiles(showType: ShowType);
        }
        else
            Clear();

        newItems?.ForEach(item => Add(new FailDatum(item)));
    }

    public void SetShowType(FailType showType)
    {
        this.ShowType = showType;
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
        var failLists = await DataAccess.Get.GetFailFileInfoWithDatum(Items.Count, (int)count, showType: ShowType);
        if (failLists is null)
        {
            return new LoadMoreItemsResult(0);
        }

        if (failLists.Length < count)
        {
            HasMoreItems = false;
        }

        failLists.ForEach(item => Add(new FailDatum(item)));

        return new LoadMoreItemsResult
        {
            Count = (uint)failLists.Length
        };
    }
}
