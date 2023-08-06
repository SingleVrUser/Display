using Display.Data;
using Microsoft.UI.Xaml.Data;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Display.Models.IncrementalCollection;

public class IncrementalLoadSuccessInfoCollection : ObservableCollection<VideoCoverDisplayClass>, ISupportIncrementalLoading
{
    private const int DefaultCount = 30;
    private double ImageWidth { get; set; }
    private double ImageHeight { get; set; }
    private bool IsFuzzyQueryActor { get; set; }

    private bool IsContainFail => FilterConditionList != null && FilterConditionList.Contains("fail");

    private string OrderBy { get; set; }
    private bool IsDesc { get; set; }

    private Dictionary<string, string> Ranges { get; set; }

    private List<string> FilterConditionList { get; set; }

    private string FilterKeywords { get; set; }


    public int AllCount { get; private set; }

    public bool HasMoreItems { get; set; } = true;

    public IncrementalLoadSuccessInfoCollection(double imgWidth, double imgHeight)
    {
        SetImageSize(imgWidth, imgHeight);
    }

    public async Task LoadData(int startShowCount = 20)
    {
        var newItems = await DataAccess.Get.GetVideoInfo(startShowCount, 0, OrderBy, IsDesc, FilterConditionList, FilterKeywords, Ranges, IsFuzzyQueryActor);

        if (Count == 0)
        {
            var successCount = DataAccess.Get.GetCountOfVideoInfo(FilterConditionList, FilterKeywords, Ranges);
            var failCount = 0;
            if (IsContainFail)
            {
                failCount = DataAccess.Get.GetCountOfFailFileInfoWithDatum(0, -1, FilterKeywords);
            }

            AllCount = successCount + failCount;
        }

        else
            Clear();

        newItems.ForEach(item => Add(new VideoCoverDisplayClass(item, ImageWidth, ImageHeight)));

    }

    public void SetImageSize(double imgWidth, double imgHeight)
    {
        ImageWidth = imgWidth;
        ImageHeight = imgHeight;
    }

    public void SetFilter(List<string> filterConditionList, string filterKeywords, bool isFuzzyQueryActor)
    {
        this.FilterConditionList = filterConditionList;
        this.FilterKeywords = filterKeywords;
        this.IsFuzzyQueryActor = isFuzzyQueryActor;
    }

    public void SetRange(Dictionary<string, string> ranges)
    {
        this.Ranges = ranges;
    }

    public void SetOrder(string orderBy, bool isDesc)
    {
        this.OrderBy = orderBy;
        this.IsDesc = isDesc;
    }

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return InnerLoadMoreItemsAsync(count).AsAsyncOperation();
    }


    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint count)
    {
        var lists = await DataAccess.Get.GetVideoInfo(DefaultCount, Count, OrderBy, IsDesc, FilterConditionList, FilterKeywords, Ranges, IsFuzzyQueryActor);

        //在最后的时候加载匹配失败的
        //用于展示搜索结果
        if (lists==null || lists.Length < DefaultCount)
        {
            HasMoreItems = false;

            //最后显示匹配失败，如果需要显示的话
            //无筛选功能
            if (IsContainFail)
            {
                var failList = await DataAccess.Get.GetFailFileInfoWithDatum(0, -1, FilterKeywords);
                failList?.ForEach(item => Add(new VideoCoverDisplayClass(new VideoInfo(item), ImageWidth, ImageHeight)));
            }
        }

        lists?.ForEach(item => Add(new VideoCoverDisplayClass(item, ImageWidth, ImageHeight)));

        return new LoadMoreItemsResult
        {
            Count = (uint)(lists?.Length ?? 0)
        };
    }
}
