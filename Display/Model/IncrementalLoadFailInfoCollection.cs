using Data;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;

namespace Display.Model;

public class IncrementalLoadFailInfoCollection : ObservableCollection<Datum>, ISupportIncrementalLoading
{
    public IncrementalLoadFailInfoCollection()
    {

    }

    public async Task LoadData(int startShowCount = 20)
    {
        var newItems = await DataAccess.LoadFailFileInfo(0, startShowCount, filterName, OrderBy, IsDesc, ShowType);

        if (Count == 0)
        {
            HasMoreItems = true;
            this.AllCount = DataAccess.CheckFailFilesCount(filterName, ShowType);
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

    public int AllCount { get;private set; }

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
        var failLists = await DataAccess.LoadFailFileInfo(Items.Count, (int)count, filterName, OrderBy, IsDesc, ShowType);
        
        if (failLists.Count < count)
        {
            HasMoreItems= false;
        }

        failLists.ForEach(item => Add(item));

        return new LoadMoreItemsResult
        {
            Count = (uint)failLists.Count
        };
    }
}
