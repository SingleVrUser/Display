
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.WinUI.Collections;
using CommunityToolkit.WinUI.Controls;
using DataAccess.Models.Entity;
using Display.Models.Entities.Details;
using Display.Models.Records;
using Display.Models.Vo;
using Display.Models.Vo.IncrementalCollection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages.DetailInfo;

public sealed partial class EditInfo
{
    private readonly List<object> _editOptions;

    private readonly AdvancedCollectionView _categorySuggestion;
    private readonly ObservableCollection<TokenData> _categoryItems;
    private readonly List<TokenData> _categories;

    private readonly IncrementalLoadActorInfoCollection _actorsSuggestion;
    private readonly ObservableCollection<ActorInfo> _actorItems;

    private readonly VideoCoverVo _videoCover;

    public EditInfo(VideoCoverVo videoCover)
    {
        InitializeComponent();

        _videoCover = videoCover;

        _categoryItems = [];
        _categories = videoCover.Category.Split(",").Where(item => !string.IsNullOrEmpty(item)).Select(x => new TokenData(x)).ToList();
        _categories.ForEach(item => _categoryItems.Add(item));

        _actorItems = [];
        var oldActors = videoCover.ActorName.Split(",")
            .Where(item => !string.IsNullOrEmpty(item))
            .Select(x => new ActorInfo(x)).ToList();
        
        oldActors.ForEach(item => _actorItems.Add(item));

        _categorySuggestion = new AdvancedCollectionView(_categories);
        _categorySuggestion.SortDescriptions.Add(new SortDescription(string.Empty, SortDirection.Ascending));

        _actorsSuggestion = new IncrementalLoadActorInfoCollection(
            new Dictionary<string, bool> { { "is_like", true }, { "prifile_path", true } }, 20);

        //SuggestedItems显示在上将无法使用动态加载
        //所以暂时将演员选项放在靠上位置

        _editOptions =
        [
            new TokenizingEditOption
            {
                Header = "类别", ItemAdding = Category_TokenItemAdding, TextChanged = Category_TextChanged,
                SuggestedItemsSource = _categorySuggestion, ItemTemplate = Resources["TokenTemplate"] as DataTemplate,
                ItemsSource = _categoryItems,
                SymbolIconSource = new SymbolIconSource { Symbol = Symbol.Edit }
            },


            new TokenizingEditOption
            {
                Header = "演员", ItemAdding = Actor_TokenItemAdding, TextChanged = Actor_TextChanged,
                SuggestedItemsSource = _actorsSuggestion,
                ItemTemplate = Resources["ActorTemplate"] as DataTemplate, ItemsSource = _actorItems,
                SymbolIconSource = new SymbolIconSource { Symbol = Symbol.AddFriend }
            },

            new CommonEditOption { Header = "标题", MinWidth = 480, Text = videoCover.Title },
            new CommonEditOption { Header = "发布时间", Text = videoCover.ReleaseTime },
            new CommonEditOption { Header = "视频长度", Text = videoCover.LengthTime },
            new CommonEditOption { Header = "导演", Text = videoCover.Director },
            new CommonEditOption { Header = "制作商", Text = videoCover.Producer },
            new CommonEditOption { Header = "发行商", Text = videoCover.Publisher },
            new CommonEditOption { Header = "系列", Text = videoCover.Series }

        ];
    }

    private void Category_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.CheckCurrent() && args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            _categorySuggestion.Filter = item => !sender.Items.Contains(item) && ((TokenData)item).Name.Contains(sender.Text, StringComparison.CurrentCultureIgnoreCase);
            //_acv.RefreshFilter();
        }
    }

    private void Category_TokenItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs args)
    {
        // Take the user's text and convert it to our data type (if we have a matching one).
        args.Item = _categories.FirstOrDefault(item => item.Name.Contains(args.TokenText, StringComparison.CurrentCultureIgnoreCase)) ??
                    // Otherwise, create a new version of our data type
                    new TokenData(args.TokenText);
    }


    private void Actor_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (!args.CheckCurrent() || args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
        
        //含有',可能是输入法输入的过程
        //'查询会报错，所以跳过
        if (sender.Text.Contains('\'')) return;

        if (string.IsNullOrEmpty(sender.Text))
        {
            _actorsSuggestion.SetFilter(null);
        }
        else
        {
            _actorsSuggestion.SetFilter([$"name LIKE '%{sender.Text}%'"]);
        }

        if (_actorsSuggestion.Count == 0) _actorsSuggestion.LoadDataAsync(20);
        //_acv.RefreshFilter();
    }

    private void Actor_TokenItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs args)
    {
        // Take the user's text and convert it to our data type (if we have a matching one).
        args.Item = _actorsSuggestion.FirstOrDefault(item => item.Name.Contains(args.TokenText, StringComparison.CurrentCultureIgnoreCase)) ??
                    // Otherwise, create a new version of our data type
                    new ActorInfo(args.TokenText);
    }

    public VideoCoverVo GetInfoAfterEdit()
    {
        foreach (var option in _editOptions)
        {
            switch (option)
            {
                case CommonEditOption commonEditOption:
                    switch (commonEditOption.Header)
                    {
                        case "标题":
                            _videoCover.Title = commonEditOption.Text;
                            break;
                        case "发布时间":
                            _videoCover.ReleaseTime = commonEditOption.Text;
                            break;
                        case "视频长度":
                            _videoCover.LengthTime = commonEditOption.Text;
                            break;
                        case "导演":
                            _videoCover.Director = commonEditOption.Text;
                            break;
                        case "制作商":
                            _videoCover.Producer = commonEditOption.Text;
                            break;
                        case "发行商":
                            _videoCover.Publisher = commonEditOption.Text;
                            break;
                        case "系列":
                            _videoCover.Series = commonEditOption.Text;
                            break;
                    }

                    break;
                case TokenizingEditOption tokenizingEditOption:
                    switch (tokenizingEditOption.Header)
                    {
                        case "类别":
                            _videoCover.Category = string.Join(",", _categoryItems.Select(item => item.Name));
                            break;
                        case "演员":
                            _videoCover.Actor = string.Join(",", _actorItems.Select(item => item.Name));
                            break;
                    }

                    break;
            }
        }

        return _videoCover;
    }


}


public class CommonOrTokenizingTemplateSelector : DataTemplateSelector
{
    public DataTemplate CommonTemplate { get; set; }

    public DataTemplate TokenizingTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return item switch
        {
            CommonEditOption => CommonTemplate,
            TokenizingEditOption => TokenizingTemplate,
            _ => null
        };
    }
}