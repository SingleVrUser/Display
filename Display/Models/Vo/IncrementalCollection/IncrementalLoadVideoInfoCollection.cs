using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using DataAccess.Dao.Interface;
using Microsoft.UI.Xaml.Data;

namespace Display.Models.Vo.IncrementalCollection;

public class IncrementalLoadVideoInfoCollection : ObservableCollection<Video.VideoCoverVo>, ISupportIncrementalLoading
{
    private readonly IVideoInfoDao _videoInfoDao = App.GetService<IVideoInfoDao>();
    
    private double ImageWidth { get; set; }
    private bool IsFuzzyQueryActor { get; set; }

    private string OrderBy { get; set; }
    private bool IsDesc { get; set; }

    private Dictionary<string, string> Ranges { get; set; }

    private List<string> FilterConditionList { get; set; }

    private string FilterKeywords { get; set; }

    public int AllCount { get; private set; }

    public bool HasMoreItems { get; set; } = true;

    public IncrementalLoadVideoInfoCollection()
    {

    }

    public IncrementalLoadVideoInfoCollection(double imgWidth)
    {
        SetImageSize(imgWidth);
    }

    public async Task LoadData(int startShowCount = 20)
    {
        Clear();

        var newItems = _videoInfoDao.GetRandomList(0, startShowCount);
        //
        // var newItems = await DataAccessLocal.Get.GetVideoInfoAsync(startShowCount, 0, OrderBy, IsDesc, FilterConditionList, FilterKeywords, Ranges, IsFuzzyQueryActor);

        // var successCount = await DataAccessLocal.Get.GetCountOfVideoInfoAsync(FilterConditionList, FilterKeywords, Ranges);

        var successCount = _videoInfoDao.TotalCount();

        AllCount = successCount;

        newItems.ForEach(item => Add(new Video.VideoCoverVo(item, ImageWidth)));
    }

    public void SetImageSize(double imgWidth)
    {
        ImageWidth = imgWidth;
    }

    public void SetFilter(List<string> filterConditionList, string filterKeywords, bool isFuzzyQueryActor)
    {
        this.FilterConditionList = filterConditionList;
        this.FilterKeywords = filterKeywords;
        this.IsFuzzyQueryActor = isFuzzyQueryActor;
        HasMoreItems = true;
    }

    public void SetRange(Dictionary<string, string> ranges)
    {
        Ranges = ranges;
    }

    public void SetOrder(string orderBy, bool isDesc)
    {
        OrderBy = orderBy;
        IsDesc = isDesc;
    }

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        Debug.WriteLine("请求加载:"+count);

        return InnerLoadMoreItemsAsync((int)count).AsAsyncOperation();
    }

    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(int count)
    {
        var lists = _videoInfoDao.List(Count, count);
        // var lists = await DataAccessLocal.Get.GetVideoInfoAsync(count, Count, OrderBy, IsDesc, FilterConditionList, FilterKeywords, Ranges, IsFuzzyQueryActor);

        //在最后的时候加载匹配失败的
        //用于展示搜索结果
        if (lists.Count < count)
        {
            HasMoreItems = false;
        }

        lists.ForEach(item => Add(new Video.VideoCoverVo(item, ImageWidth)));

        var result = new LoadMoreItemsResult((uint)lists.Count);

        return result;
    }
}
