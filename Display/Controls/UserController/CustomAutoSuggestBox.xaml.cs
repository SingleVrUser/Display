using Display.Helper.FileProperties.Name;
using Display.Providers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using DataAccess.Dao.Interface;
using DataAccess.Models.Entity;
using Display.Models.Dto;
using Display.Models.Vo;

namespace Display.Controls.UserController;

public sealed partial class CustomAutoSuggestBox
{
    //提交请求（按下Enter）
    public event TypedEventHandler<AutoSuggestBox, AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;
    public event EventHandler<object> OpenAutoSuggestionBoxCompleted;
    public event EventHandler<object> CloseAutoSuggestionBoxCompleted;

    private readonly ISearchHistoryDao _searchHistoryDao = App.GetService<ISearchHistoryDao>();

    private readonly List<SearchHistory> _historySearchItemList;

    public CustomAutoSuggestBox()
    {
        InitializeComponent();
            
        _historySearchItemList = _searchHistoryDao.List().OrderByDescending(i=>i.Id).ToList();
    }
    
    //输入的Text改变
    private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        var searchText = sender.Text;
        if (args.Reason is not (AutoSuggestionBoxTextChangeReason.UserInput or AutoSuggestionBoxTextChangeReason.ProgrammaticChange))
            return;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            ShowHistorySearch();
            return;
        }

        //过滤非法元素
        searchText = searchText.Replace("'", "");

        var selectedTypes = GetSelectedTypes();

        var resultList = new List<object>();
        var item = await FileMatch.GetVideoInfoFromType(selectedTypes, searchText, 50);

        if (item.Count == 0)
        {
            var content = "未找到";
            if (AppSettings.IsUseX1080X)
            {
                content += "，点击搜索资源";
            }
            resultList.Add(content);
        }
        else
        {
            if (AppSettings.IsUseX1080X)
            {
                resultList.Add("点击搜索资源");
            }

            item.ForEach(i=>resultList.Add(new SearchHistoryVo()
            {
                Name = i.Name,
                ActorName = string.Join(",",i.ActorInfoList),
                Category = i.CategoryList != null
                    ? string.Join(",", i.CategoryList)
                    : string.Empty,
                ImagePath = i.ImagePath
            }));
        }
        
        sender.ItemsSource = resultList;

        _isBusy = false;
    }

    private List<string> GetSelectedTypes()
    {
        var allSelectedButtons = GetAllSelectedMethodButton();
        var selectedTypes = allSelectedButtons.Where(item => item.IsChecked).ToList().Select(item => item.Tag.ToString()).ToList();

        return selectedTypes;
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.QueryText != typeof(SearchHistory).FullName)
        {
            sender.DataContext = GetSelectedTypes();

            //只有查询到才跳转
            if (sender.ItemsSource is List<object> { Count: > 0 })
            {
                QuerySubmitted?.Invoke(sender, args);
            }

            TrySaveSearchHistory(args.QueryText);
        }

        //初始化搜索框
        NavViewSearchBox.Text = "";
        sender.ItemsSource = null;
    }

    private void TrySaveSearchHistory(string keyword)
    {
        if (string.IsNullOrEmpty(keyword) ||
            _historySearchItemList.FirstOrDefault(i => i.Keyword == keyword) != null) return;
        
        var searchHistory = new SearchHistory {Keyword = keyword};
        _historySearchItemList.Insert(0,searchHistory);
            
        //保存到数据库
        _searchHistoryDao.ExecuteAdd(searchHistory);
    }

    private void NavViewSearchBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (selectFoundMethodButton.Visibility != Visibility.Collapsed) return;

        OpenAutoSuggestionBoxStoryboard.Begin();

        ShowHistorySearch();
    }

    private void ShowHistorySearch()
    {
        var searchHistories = _historySearchItemList.ToArray();
        NavViewSearchBox.ItemsSource = new List<HistorySearchItem>
        {
            new(searchHistories)
        };
    }

    private void NavViewSearchBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_isBusy) return;
        if (sender is not AutoSuggestBox autoSuggestBox) return;

        if (selectFoundMethodButton.FocusState == FocusState.Pointer) return;

        CloseAutoSuggestionBoxStoryboard.Begin();

        //清空输入
        NavViewSearchBox.Text = "";
        autoSuggestBox.ItemsSource = null;
    }

    /// <summary>
    /// 获取搜索选择方式的按钮
    /// </summary>
    /// <returns></returns>
    private List<ToggleMenuFlyoutItem> GetAllSelectedMethodButton()
    {
        return
        [
            SelectedCid_Toggle, SelectedActor_Toggle, SelectedCategory_Toggle, SelectedTitle_Toggle,
            SelectedProducter_Toggle, SelectedDirector_Toggle, SelectedFail_Toggle
        ];
    }

    /// <summary>
    /// 切换搜索范围
    /// 左键单选
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ChangedFindMethod_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleMenuFlyoutItem item) return;

        var items = GetAllSelectedMethodButton();

        //单选肯定要被选中
        item.IsChecked = true;

        //单选肯定不是全选
        SelectedAll_Toggle.IsChecked = false;

        //单选，只保留一个为选中状态
        foreach (var button in items.Where(button => button.IsChecked && button.Text != item.Text))
        {
            button.IsChecked = false;
        }

        selectFoundMethodButton.Content = item.Text;
    }

    /// <summary>
    /// 搜索方式全选或重设
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SelectedAllFindMethod_Clicked(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleMenuFlyoutItem item) return;

        var items = GetAllSelectedMethodButton();

        //全选
        if (item.IsChecked)
        {
            items.ForEach(flyoutItem => flyoutItem.IsChecked = true);
            selectFoundMethodButton.Content = "全部";
        }
        //全不选（除了第一个）
        else
        {
            items.Skip(1).ToList().ForEach(flyoutItem => flyoutItem.IsChecked = false);
            selectFoundMethodButton.Content = "番号";
        }

    }

    /// <summary>
    /// 切换搜索范围
    /// 右键多选
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ChangedFindMethod_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {   
        if (sender is not ToggleMenuFlyoutItem item) return;

        var items = GetAllSelectedMethodButton();

        var selectedList = items.Where(flyoutItem => flyoutItem.IsChecked).ToList();

        //加上或减去当前选中或取消项
        var selectedCount = item.IsChecked ? selectedList.Count - 1 : selectedList.Count + 1;

        //System.Diagnostics.Debug.WriteLine($"当前选中的数量：{selectedCount}");

        //不能不选，不做任何改变
        if (selectedCount == 0)
        {
        }
        else
        {
            item.IsChecked = !item.IsChecked;

            if (selectedCount == 1)
            {
                //该项被取消
                if (!item.IsChecked)
                {
                    selectFoundMethodButton.Content = selectedList.IndexOf(item) == 0 ? selectedList[1].Text : selectedList[0].Text;
                }
                else
                    selectFoundMethodButton.Content = item.Text;

                SelectedAll_Toggle.IsChecked = false;
            }
            else if (selectedCount == items.Count)
            {
                selectFoundMethodButton.Content = "全部";
                SelectedAll_Toggle.IsChecked = true;
            }
            //多选
            else
            {
                selectFoundMethodButton.Content = "多个";
                SelectedAll_Toggle.IsChecked = false;
            }
        }
    }

    private void OpenAutoSuggestionBoxStoryboard_OnCompleted(object sender, object e)
    {
        OpenAutoSuggestionBoxCompleted?.Invoke(sender, e);
    }

    private void CloseAutoSuggestionBoxStoryboard_OnCompleted(object sender, object e)
    {
        CloseAutoSuggestionBoxCompleted?.Invoke(sender, e);
    }


    public event TypedEventHandler<object, string> SuggestionItemTapped;
    private void ItemTapped(object sender, TappedRoutedEventArgs e)
    {
        //准备动画
        ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", NavViewSearchBox);

        var keyword = NavViewSearchBox.Text;
        SuggestionItemTapped?.Invoke(sender, keyword);

        TrySaveSearchHistory(keyword);
    }

    private bool _isBusy;

    private void SearchHistoryItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not GridViewItem { Content: string keyword }) return;

        _isBusy = true;
        NavViewSearchBox.IsSuggestionListOpen = true;
        NavViewSearchBox.Text = keyword;
    }
    
    private void ClearSearchHistoryClick(object sender, RoutedEventArgs e)
    {
        _historySearchItemList.Clear();
        _searchHistoryDao.ExecuteDeleteAll();
        NavViewSearchBox.ItemsSource = null;
    }
}

public class SuggestionBoxItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate NotFoundDataTemplate { get; set; }
    public DataTemplate FoundListDataTemplate { get; set; }

    public DataTemplate HistoryDataTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return item switch
        {
            string => NotFoundDataTemplate,
            VideoInfo => FoundListDataTemplate,
            HistorySearchItem => HistoryDataTemplate,
            _ => base.SelectTemplateCore(item)
        };
    }
}


public class ItemsContainerStyleSelector : StyleSelector
{
    public Style NoPointerOverStyle { get; set; }

    protected override Style SelectStyleCore(object item, DependencyObject container)
    {
        return item is HistorySearchItem ? NoPointerOverStyle : base.SelectStyleCore(item, container);
    }
}

