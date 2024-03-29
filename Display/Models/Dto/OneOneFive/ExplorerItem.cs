using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Display.Models.Dto.OneOneFive;

/// <summary>
/// 目录展示
/// </summary>
public class ExplorerItem : INotifyPropertyChanged
{
    public string Name { get; set; }
    public long Id { get; set; }
    public bool HasUnrealizedChildren { get; set; }
    public FilesInfo.FileType Type { get; set; }
    private ObservableCollection<ExplorerItem> m_children;
    public ObservableCollection<ExplorerItem> Children
    {
        get => m_children ??= new ObservableCollection<ExplorerItem>();
        set
        {
            m_children = value;
        }
    }

    public Datum datum;

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

    private bool m_isExpanded;
    public bool IsExpanded
    {
        get => m_isExpanded;
        set
        {
            if (m_isExpanded != value)
            {
                m_isExpanded = value;
                NotifyPropertyChanged("IsExpanded");
            }
        }
    }

    private bool m_isSelected;
    public bool IsSelected
    {
        get => m_isSelected;

        set
        {
            if (m_isSelected == value) return;
            m_isSelected = value;
            NotifyPropertyChanged(nameof(IsSelected));
        }

    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}