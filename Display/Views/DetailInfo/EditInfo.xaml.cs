// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.WinUI.Collections;
using CommunityToolkit.WinUI.Controls;
using Display.Services.IncrementalCollection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Display.Models.Dto.OneOneFive;
using Display.Models.Entities.Details;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.DetailInfo;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class EditInfo : Page
{
    private List<object> EditOptions;

    private AdvancedCollectionView category_suggestion;
    private ObservableCollection<TokenData> category_items;
    private List<TokenData> categories;

    //private AdvancedCollectionView actor_acv;
    private IncrementalLoadActorInfoCollection actors_suggestion;
    private ObservableCollection<ActorInfo> actor_items;

    VideoCoverDisplayClass videoInfo;


    public EditInfo(VideoCoverDisplayClass videoInfo)
    {
        this.InitializeComponent();

        this.videoInfo = videoInfo;

        category_items = new();
        categories = videoInfo.Category.Split(",").Where(item => !string.IsNullOrEmpty(item)).Select(x => new TokenData(x)).ToList();
        categories.ForEach(item => category_items.Add(item));

        actor_items = new();
        var old_actors = videoInfo.Actor.Split(",").Where(item => !string.IsNullOrEmpty(item)).Select(x => new ActorInfo() { Name = x }).ToList();
        old_actors.ForEach(item => actor_items.Add(item));

        category_suggestion = new(categories, false);
        category_suggestion.SortDescriptions.Add(new SortDescription(string.Empty, SortDirection.Ascending));

        actors_suggestion = new(new() { { "is_like", true }, { "prifile_path", true } }, 20);

        //SuggestedItems显示在上将无法使用动态加载
        //所以暂时将演员选项放在靠上位置

        EditOptions = new()
        {
            new TokenizingEditOption() { Header = "类别" ,ItemAdding = this.Category_TokenItemAdding,TextChanged = this.Category_TextChanged,
                                        SuggestedItemsSource = category_suggestion, ItemTemplate= Resources["TokenTemplate"] as DataTemplate,ItemsSource=category_items,
                                        SymbolIconSource= new(){Symbol=Symbol.Edit}},

            new TokenizingEditOption() { Header = "演员" ,ItemAdding = this.Actor_TokenItemAdding,TextChanged = this.Actor_TextChanged ,
                                        SuggestedItemsSource = actors_suggestion, ItemTemplate= this.Resources["ActorTemplate"] as DataTemplate,ItemsSource=actor_items,
                                        SymbolIconSource=new(){Symbol=Symbol.AddFriend} },

            new CommonEditOption {Header= "标题",MinWidth=480,Text=videoInfo.Title},
            new CommonEditOption {Header= "发布时间",Text=videoInfo.ReleaseTime},
            new CommonEditOption {Header= "视频长度",Text=videoInfo.Lengthtime},
            new CommonEditOption { Header = "导演",Text=videoInfo.Director },
            new CommonEditOption { Header = "制作商" ,Text=videoInfo.Producer},
            new CommonEditOption { Header = "发行商" ,Text=videoInfo.Publisher},
            new CommonEditOption { Header = "系列" ,Text=videoInfo.Series},
        };
    }

    private void Category_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.CheckCurrent() && args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            category_suggestion.Filter = item => !sender.Items.Contains(item) && (item as TokenData).Name.Contains(sender.Text, System.StringComparison.CurrentCultureIgnoreCase);
            //_acv.RefreshFilter();
        }
    }

    private void Category_TokenItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs args)
    {
        // Take the user's text and convert it to our data type (if we have a matching one).
        args.Item = categories.FirstOrDefault((item) => item.Name.Contains(args.TokenText, System.StringComparison.CurrentCultureIgnoreCase));

        // Otherwise, create a new version of our data type
        if (args.Item == null)
        {
            args.Item = new TokenData(args.TokenText);
        }
    }


    private async void Actor_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.CheckCurrent() && args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            //含有',可能是输入法输入的过程
            //'查询会报错，所以跳过
            if (sender.Text.Contains("'")) return;

            if (string.IsNullOrEmpty(sender.Text))
            {
                actors_suggestion.SetFilter(null);
            }
            else
            {
                actors_suggestion.SetFilter(new() { $"name LIKE '%{sender.Text}%'" });
            }

            if (actors_suggestion.Count == 0) await actors_suggestion.LoadData(20);
            //_acv.RefreshFilter();
        }
    }

    private void Actor_TokenItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs args)
    {
        // Take the user's text and convert it to our data type (if we have a matching one).
        args.Item = actors_suggestion.FirstOrDefault((item) => item.Name.Contains(args.TokenText, System.StringComparison.CurrentCultureIgnoreCase));

        // Otherwise, create a new version of our data type
        if (args.Item == null)
        {
            args.Item = new ActorInfo() { Name = args.TokenText };
        }
    }

    public VideoCoverDisplayClass GetInfoAfterEdit()
    {
        var info = videoInfo;

        foreach (var option in EditOptions)
        {
            switch (option)
            {
                case CommonEditOption commonEditOption:
                    switch (commonEditOption.Header)
                    {
                        case "标题":
                            info.Title = commonEditOption.Text;
                            break;
                        case "发布时间":
                            info.ReleaseTime = commonEditOption.Text;
                            break;
                        case "视频长度":
                            info.Lengthtime = commonEditOption.Text;
                            break;
                        case "导演":
                            info.Director = commonEditOption.Text;
                            break;
                        case "制作商":
                            info.Producer = commonEditOption.Text;
                            break;
                        case "发行商":
                            info.Publisher = commonEditOption.Text;
                            break;
                        case "系列":
                            info.Series = commonEditOption.Text;
                            break;
                    }

                    break;
                case TokenizingEditOption tokenizingEditOption:
                    switch (tokenizingEditOption.Header)
                    {
                        case "类别":
                            info.Category = string.Join(",", category_items.Select(item => item.Name));
                            break;
                        case "演员":
                            info.Actor = string.Join(",", actor_items.Select(item => item.Name));
                            break;
                    }

                    break;
            }
        }

        return info;
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