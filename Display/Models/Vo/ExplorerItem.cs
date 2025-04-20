using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DataAccess.Models.Entity;
using Display.Helper.UI;
using Display.Models.Enums;

namespace Display.Models.Vo;

/// <summary>
/// 目录展示
/// </summary>
public partial class ExplorerItem : ObservableObject
{
    public string Name { get; init; }
    public long Id { get; init; }
    public bool HasUnrealizedChildren { get; init; }
    public FileType Type { get; init; }
    
    public ObservableCollection<ExplorerItem> Children = [];

    public FileInfo Datum;

    public string IconPath => ResourceHelper.GetFileIconFromType(Type);

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isSelected;
}