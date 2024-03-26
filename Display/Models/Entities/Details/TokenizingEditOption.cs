using Windows.Foundation;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Models.Entities.Details;

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