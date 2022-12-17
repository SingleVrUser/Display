using Data;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Display.Model;

public class IncrementalLoadingdFileCollection : ObservableCollection<Datum>, ISupportIncrementalLoading
{
    public string filterName { get; set; } = string.Empty;

    public int AllCount { get; set; }

    public bool HasMoreItems { get; set; } = true;

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return InnerLoadMoreItemsAsync(count).AsAsyncOperation();
    }

    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint count)
    {
        var failLists = DataAccess.LoadFailFileInfo(Items.Count, (int)count, filterName);
        
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
