using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Display.Providers;
using Microsoft.UI.Xaml.Data;
using SharpCompress;
using DataAccess;

namespace Display.Models.Vo.IncrementalCollection;

public class IncrementalLoadSuccessInfoCollection : ObservableCollection<VideoInfoVo>, ISupportIncrementalLoading
{
    private double ImageWidth { get; set; }
    //private double ImageHeight { get; set; }
    private bool IsFuzzyQueryActor { get; set; }

    private bool IsContainFail => FilterConditionList != null && FilterConditionList.Contains("fail");

    private string OrderBy { get; set; }
    private bool IsDesc { get; set; }

    private Dictionary<string, string> Ranges { get; set; }

    private List<string> FilterConditionList { get; set; }

    private string FilterKeywords { get; set; }


    public int AllCount { get; private set; }

    public bool HasMoreItems { get; set; } = true;

    public IncrementalLoadSuccessInfoCollection()
    {

    }

    public IncrementalLoadSuccessInfoCollection(double imgWidth)
    {
        SetImageSize(imgWidth);
    }

    public async Task LoadData(int startShowCount = 20)
    {
        Clear();
        var newItems = await DataAccessLocal.Get.GetVideoInfoAsync(startShowCount, 0, OrderBy, IsDesc, FilterConditionList, FilterKeywords, Ranges, IsFuzzyQueryActor);

        var successCount = await DataAccessLocal.Get.GetCountOfVideoInfoAsync(FilterConditionList, FilterKeywords, Ranges);
        var failCount = 0;
        if (IsContainFail)
        {
            failCount = await DataAccessLocal.Get.GetCountOfFailFileInfoWithFilesInfoAsync(0, -1, FilterKeywords);
        }

        AllCount = successCount + failCount;

        newItems?.ForEach(item => Add(new VideoInfoVo(item, ImageWidth)));
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
        var lists = await DataAccessLocal.Get.GetVideoInfoAsync(count, Count, OrderBy, IsDesc, FilterConditionList, FilterKeywords, Ranges, IsFuzzyQueryActor);

        //在最后的时候加载匹配失败的
        //用于展示搜索结果
        if (lists == null || lists.Length < count)
        {
            HasMoreItems = false;

            //最后显示匹配失败，如果需要显示的话
            //无筛选功能
            if (IsContainFail)
            {
                var failList = await DataAccessLocal.Get.GetFailFileInfoWithFilesInfoAsync(0, -1, FilterKeywords);
                
                foreach (var filesInfo in failList)
                {
                    var failVideoInfo = new FailVideoInfo(filesInfo);
                    // var videoInfo = new FailVideoInfo(failVideoInfo, ImageWidth, ImageHeight);
                    Add(failVideoInfo);
                }
                
            }
        }

        lists?.ForEach(item => Add(new VideoInfoVo(item, ImageWidth)));

        var result = new LoadMoreItemsResult((uint)(lists?.Length ?? 0));

        return result;
    }
}
