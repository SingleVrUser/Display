using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;
using DataAccess.Models.Entity;
using Display.Models.Enums.OneOneFive;
using Display.Providers;
using Microsoft.UI.Xaml.Data;
using SharpCompress;

namespace Display.Models.Vo.IncrementalCollection;

public class IncrementalLoadFailDatumInfoCollection : ObservableCollection<FilesInfo>, ISupportIncrementalLoading
{

    public void SetShowType(FailType showType)
    {
        ShowType = showType;
    }

    public FailType ShowType { get; set; } = FailType.All;

    public string FilterName { get; set; } = string.Empty;

    public int AllCount { get; private set; }

    public bool HasMoreItems { get; set; } = true;

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return InnerLoadMoreItemsAsync(count).AsAsyncOperation();
    }
    public void SetFilter(string filterKeywords)
    {
        FilterName = filterKeywords;
    }
    public void SetOrder(string orderBy, bool isDesc)
    {
        OrderBy = orderBy;
        IsDesc = isDesc;
    }

    public string OrderBy { get; set; }
    public bool IsDesc { get; set; }

    public async Task LoadData(int startShowCount = 20)
    {
        var newItems = DataAccessLocal.Get.GetFailFileInfoWithFilesInfo(0, startShowCount, FilterName, OrderBy, IsDesc, ShowType);
        if (Count == 0)
        {
            AllCount = DataAccessLocal.Get.GetCountOfFailFilesInfoFiles(FilterName, ShowType);
        }
        else
            Clear();

        newItems?.ForEach(Add);
        HasMoreItems = true;
    }

    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint count)
    {
        var failLists = DataAccessLocal.Get.GetFailFileInfoWithFilesInfo(Items.Count, (int)count, FilterName, OrderBy, IsDesc, ShowType);

        if (failLists == null)
        {
            HasMoreItems = false;
            return new LoadMoreItemsResult
            {
                Count = 0
            };
        }

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
