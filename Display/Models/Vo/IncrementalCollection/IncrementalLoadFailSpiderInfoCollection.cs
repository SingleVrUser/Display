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
    public void LoadData(int startShowCount = 20)
    {
        var newItems = DataAccessLocal.Get.GetFailFileInfoWithFilesInfo(0, startShowCount, showType: ShowType);

        if (Count == 0)
        {
            HasMoreItems = true;
            AllCount = DataAccessLocal.Get.GetCountOfFailFilesInfoFiles(showType: ShowType);
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

    private Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint count)
    {
        var failLists = DataAccessLocal.Get.GetFailFileInfoWithFilesInfo(Items.Count, (int)count, showType: ShowType);
        if (failLists is null)
        {
            return Task.FromResult(new LoadMoreItemsResult(0));
        }

        if (failLists.Length < count)
        {
            HasMoreItems = false;
        }

        failLists.ForEach(item => Add(new FailDatum(item)));

        return Task.FromResult(new LoadMoreItemsResult
        {
            Count = (uint)failLists.Length
        });
    }
}
