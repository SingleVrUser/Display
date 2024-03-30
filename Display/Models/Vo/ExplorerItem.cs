using System.Collections.ObjectModel;
using System.ComponentModel;
using Display.Models.Api.OneOneFive.File;
using Display.Models.Vo.OneOneFive;

namespace Display.Models.Vo;

/// <summary>
/// 目录展示
/// </summary>
public class ExplorerItem : INotifyPropertyChanged
{
    public string Name { get; init; }
    public long Id { get; init; }
    public bool HasUnrealizedChildren { get; init; }
    public FilesInfo.FileType Type { get; init; }
    private ObservableCollection<ExplorerItem> _mChildren;
    public ObservableCollection<ExplorerItem> Children
    {
        get => _mChildren ??= [];
        set => _mChildren = value;
    }

    public Datum Datum;

    private string _iconPath;
    public string IconPath
    {
        get
        {
            if (_iconPath != null) return _iconPath;
            _iconPath = FilesInfo.GetFileIconFromType(Type);

            return _iconPath;
        }
        set => _iconPath = value;
    }

    private bool _mIsExpanded;
    public bool IsExpanded
    {
        get => _mIsExpanded;
        set
        {
            if (_mIsExpanded == value) return;
            
            _mIsExpanded = value;
            NotifyPropertyChanged(nameof(IsExpanded));
        }
    }

    private bool _mIsSelected;
    public bool IsSelected
    {
        get => _mIsSelected;

        set
        {
            if (_mIsSelected == value) return;
            _mIsSelected = value;
            NotifyPropertyChanged(nameof(IsSelected));
        }

    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}