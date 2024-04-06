using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Display.Models.Enums.OneOneFive;
using Display.Providers;
using Microsoft.UI.Xaml.Data;
using SharpCompress;

namespace Display.Models.Vo.IncrementalCollection;

public class IncrementalLoadFailSpiderInfoCollection : ObservableCollection<FailDatum>, ISupportIncrementalLoading
{
    public async void LoadData(int startShowCount = 20)
    {
        var newItems = await DataAccessLocal.Get.GetFailFileInfoWithFilesInfoAsync(0, startShowCount, showType: ShowType);

        if (Count == 0)
        {
            HasMoreItems = true;
            AllCount = await DataAccessLocal.Get.GetCountOfFailFilesInfoFilesAsync(showType: ShowType);
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
        var failLists = await DataAccessLocal.Get.GetFailFileInfoWithFilesInfoAsync(Items.Count, (int)count, showType: ShowType);
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
