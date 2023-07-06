// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Display.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Windows.Foundation;
using Display.Models.IncrementalCollection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.DetailInfo;

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

            new CommonEditOption(){Header= "标题",MinWidth=480,Text=videoInfo.Title},
            new CommonEditOption (){Header= "发布时间",Text=videoInfo.ReleaseTime},
            new CommonEditOption (){Header= "视频长度",Text=videoInfo.Lengthtime},
            new CommonEditOption() { Header = "导演",Text=videoInfo.Director },
            new CommonEditOption() { Header = "制作商" ,Text=videoInfo.Producer},
            new CommonEditOption() { Header = "发行商" ,Text=videoInfo.Publisher},
            new CommonEditOption() { Header = "系列" ,Text=videoInfo.Series},
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
                actors_suggestion.SetFilter(new () { $"name LIKE '%{sender.Text}%'" });
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
        VideoCoverDisplayClass info = this.videoInfo;

        foreach (var option in EditOptions)
        {
            if (option is CommonEditOption commonEditOption)
            {
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
            }
            else if (option is TokenizingEditOption tokenizingEditOption)
            {
                switch (tokenizingEditOption.Header)
                {
                    case "类别":
                        info.Category = string.Join(",", category_items.Select(item => item.Name));
                        break;
                    case "演员":
                        info.Actor = string.Join(",", actor_items.Select(item => item.Name));
                        break;
                }
            }
        }

        return info;
    }

}


public class CommonOrTokenizingTemplateSelector : DataTemplateSelector
{
    // Define the (currently empty) data templates to return
    // These will be "filled-in" in the XAML code.
    public DataTemplate CommonTemplate { get; set; }

    public DataTemplate TokenizingTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        // Return the correct data template based on the item's type.
        if (item is CommonEditOption)
        {
            return CommonTemplate;
        }
        else if (item is TokenizingEditOption)
        {
            return TokenizingTemplate;
        }
        else
        {
            return null;
        }
    }
}

public class CommonEditOption
{
    public string Header;
    public string PlaceholderText => $"请输入{Header}";
    public double MinWidth { get; set; } = 150;

    public string Text;
}

public class TokenData
{
    public string Initials => string.Empty + Name.FirstOrDefault();

    public string Name { get; set; }

    public TokenData(string name)
    {
        this.Name = name;
    }

}

public class TokenizingEditOption
{
    public string Header;
    public string PlaceholderText => $"添加{Header}";

    public string Text;

    public SymbolIconSource SymbolIconSource;

    public TypedEventHandler<TokenizingTextBox, TokenItemAddingEventArgs> ItemAdding;

    public TypedEventHandler<AutoSuggestBox, AutoSuggestBoxTextChangedEventArgs> TextChanged;

    public object SuggestedItemsSource;

    public object ItemsSource { get; set; }

    public DataTemplate ItemTemplate;

}
