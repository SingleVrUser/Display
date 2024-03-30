using Windows.Foundation;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Models.Entities.Details;

public class TokenizingEditOption
{
    public string Header;
    public string PlaceholderText => $"添加{Header}";

    public SymbolIconSource SymbolIconSource;

    public TypedEventHandler<AutoSuggestBox, AutoSuggestBoxTextChangedEventArgs> TextChanged;
    
    public TypedEventHandler<TokenizingTextBox, TokenItemAddingEventArgs> ItemAdding;

    public object SuggestedItemsSource;

    public object ItemsSource { get; init; }

    public DataTemplate ItemTemplate;
    
    
    public void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        TextChanged?.Invoke(sender,args);
    }
    
    public void OnItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs args)
    {
        ItemAdding?.Invoke(sender,args);
    }

}