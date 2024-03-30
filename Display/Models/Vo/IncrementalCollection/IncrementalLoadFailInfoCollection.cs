using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Display.Models.Dto.OneOneFive;
using Display.Models.Entities;
using Display.Models.Entities.OneOneFive;
using Display.Models.Enums.OneOneFive;
using Display.Models.Vo;
using Display.Models.Vo.OneOneFive;
using Display.Providers;
using Microsoft.UI.Xaml.Data;
using SharpCompress;

namespace Display.Models.Data.IncrementalCollection;

public class IncrementalLoadFailInfoCollection(FailInfoShowType showType)
    : ObservableCollection<FailInfo>, ISupportIncrementalLoading
{
    public bool HasMoreItems { get; set; } = true;

    public int AllCount { get; private set; }

    public FailInfoShowType ShowType { get; private set; } = showType;

    public async void SetShowType(FailInfoShowType showType)
    {
        ShowType = showType;

        if (!HasMoreItems) HasMoreItems = true;

        Clear();

        await LoadData();
    }

    public async Task<int> LoadData(int limit = 20, int offset = 0)
    {
        var infos = await DataAccess.Get.GetFailFileInfoWithFailInfo(offset, limit, ShowType);

        infos?.ForEach(Add);

        var getCount = infos?.Length ?? 0;

        if (Count == 0)
            AllCount = DataAccess.Get.GetCountOfFailInfos(ShowType);

        if (AllCount <= Count || getCount == 0)
        {
            HasMoreItems = false;
        }

        return getCount;
    }

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return InnerLoadMoreItemsAsync().AsAsyncOperation();
    }

    private readonly int _defaultCount = 20;
    private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync()
    {
        var getCount = await LoadData(_defaultCount, Count);

        return new LoadMoreItemsResult
        {
            Count = (uint)getCount
        };
    }
}
