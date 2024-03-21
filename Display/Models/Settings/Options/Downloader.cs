using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Display.Helper.FileProperties.Name;
using Display.Models.Data.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Models.Settings.Options;

internal class Downloader(string name) : INotifyPropertyChanged
{
    public string Name { get; set; } = name;
    public string Description { get; set; }
    public string ApiPlaceholder { get; set; }
    public string ApiText { get; set; }






    public Action<string> SavePathAction { get; set; }
    public Func<string> ResetPathFunc { get; set; }


    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
