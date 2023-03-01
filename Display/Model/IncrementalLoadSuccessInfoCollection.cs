using Data;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Display.Model;

public class IncrementalLoadSuccessInfoCollection : ObservableCollection<VideoCoverDisplayClass>, ISupportIncrementalLoading
{
    public int AllCount { get; private set; }

    public bool HasMoreItems { get; set; } = true;

    private double imageWidth { get; set; }
    private double imageHeight { get; set; }

    private bool isFuzzyQueryActor { get; set; }

    private string orderBy { get; set; }
    private bool isDesc { get; set; }

    private Dictionary<string, string> ranges { get; set; }

    private List<string> filterConditionList { get; set; }

    private string filterKeywords { get; set; }

    public IncrementalLoadSuccessInfoCollection(double imgwidth,double imgheight)
    {
        SetImageSize(imgwidth, imgheight);
    }

    public async Task LoadData(int startShowCount = 20)
    {
        var newItems = await DataAccess.LoadVideoInfo(startShowCount, 0,orderBy,isDesc, filterConditionList, filterKeywords, ranges, isFuzzyQueryActor);

        if (Count == 0)
            this.AllCount = DataAccess.CheckVideoInfoCount(orderBy, isDesc, filterConditionList, filterKeywords,ranges);
        else
            Clear();

        newItems.ForEach(item => Add(new(item, imageWidth, imageHeight)));
    }

    public void SetImageSize(double imgwidth, double imgheight)
    {
        this.imageWidth = imgwidth;
        this.imageHeight = imgheight;
    }

    public void SetFilter(List<string> filterConditionList, string filterKeywords, bool isFuzzyQueryActor)
    {
        this.filterConditionList = filterConditionList;
        this.filterKeywords = filterKeywords;
        this.isFuzzyQueryActor = isFuzzyQueryActor;
    }

    public void SetRange(Dictionary<string, string> ranges)
    {
        this.ranges = ranges;
    }

    public void SetOrder(string orderBy, bool isDesc)
    {
        this.orderBy = orderBy;
        this.isDesc = isDesc;
    }

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return InnerLoadMoreItemsAsync(count).AsAsyncOperation();
    }

    public int defaultCount = 30;

    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint count)
    {
        var lists = await DataAccess.LoadVideoInfo(defaultCount, Count, orderBy, isDesc, filterConditionList, filterKeywords, ranges, isFuzzyQueryActor);

        //在最后的时候加载匹配失败的
        //用于展示搜索结果
        if (lists.Count < defaultCount)
        {
            HasMoreItems = false;

            //最后显示匹配失败，如果需要显示的话
            //无筛选功能
            if (filterConditionList!=null && filterConditionList.Contains("fail"))
            {
                var failList = await DataAccess.LoadFailFileInfoWithDatum(0,-1,filterKeywords);
                failList.ForEach(item => Add(new(new(item), imageWidth, imageHeight)));
            }
        }

        lists.ForEach(item => Add(new(item,imageWidth,imageHeight)));

        return new LoadMoreItemsResult
        {
            Count = (uint)lists.Count
        };
    }
}
