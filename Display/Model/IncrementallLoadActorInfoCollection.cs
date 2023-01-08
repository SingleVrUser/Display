using Data;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Display.Model;

public class IncrementallLoadActorInfoCollection : ObservableCollection<ActorInfo>, ISupportIncrementalLoading
{
    public bool HasMoreItems { get; set; } = true;

    public int AllCount { get; private set; }

    public Dictionary<string,bool> orderByList { get;private set; }

    public List<string> filterList { get; private set; }

    public bool isDsc { get;private set; }

    public IncrementallLoadActorInfoCollection(Dictionary<string, bool> orderByList)
    {
        SetOrder(orderByList);
    }

    public async Task<int> LoadData(int limit = 40,int offset=0)
    {
        var ActorInfos = await DataAccess.LoadActorInfo(limit, offset, orderByList, filterList);

        if (Count == 0)
            this.AllCount = DataAccess.CheckActorInfoCount(filterList);

        ActorInfos.ForEach(item => Add(item));

        if (AllCount <= Count)
        {
            HasMoreItems = false;
        }

        return ActorInfos.Count;
    }

    public void SetFilter(List<string> filterList)
    {
        this.filterList = filterList;
        Clear();
    }

    public void SetOrder(Dictionary<string, bool> orderBy)
    {
        this.orderByList = orderBy;
    }

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return InnerLoadMoreItemsAsync(count).AsAsyncOperation();
    }

    private int defaultCount = 40;
    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint count)
    {
        int getCount = await LoadData(defaultCount, Count);

        return new LoadMoreItemsResult
        {
            Count = (uint)getCount
        };
    }
}

