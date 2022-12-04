using Data;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Display.Model;

public class IncrementalLoadingdVideoFileCollection : ObservableCollection<VideoCoverDisplayClass>, ISupportIncrementalLoading
{
    public IncrementalLoadingdVideoFileCollection(List<VideoCoverDisplayClass> AllItems)
    {
        this.AllItems = AllItems;
    }

    public string filterName { get; set; } = string.Empty;

    public bool HasMoreItems { get; set; } = true;

    public List<VideoCoverDisplayClass> AllItems { get; set; }

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return InnerLoadMoreItemsAsync(count).AsAsyncOperation();
    }

    public int defaultCount = 30;

    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint count)
    {
        var Lists = AllItems.Skip(Count).Take(defaultCount).ToList();

        if (Lists.Count < defaultCount)
        {
            HasMoreItems = false;
        }

        Lists.ForEach(item => Add(item));

        return new LoadMoreItemsResult
        {
            Count = (uint)Lists.Count
        };
    }
}
