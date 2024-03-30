using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage.Pickers;
using Display.Helper.FileProperties.Name;
using Display.Models.Enums;
using Microsoft.UI.Xaml;

namespace Display.Models.Dto.Settings.Options;

internal class SavePath : INotifyPropertyChanged
{
    public string Name { get; set; }

    public SavePathEnum Own { get; set; }

    private string _path;
    public string Path
    {
        get => _path;
        set
        {
            if (_path == value) return;
            _path = value;
            OnPropertyChanged();
        }
    }

    public string OriginPath { get; }

    public PickerLocationId SuggestedStartLocation { get; set; }

    public Action<string> SaveAction { get; set; }

    public Action SaveSamePathAction { get; set; }

    public SavePath(string originPath)
    {
        OriginPath = originPath;
        Path = originPath;
    }

    public void OpenPathButtonClick(object sender, RoutedEventArgs args)
    {
        FileMatch.LaunchFolder(Path);
    }

    public async void UpdatePathButtonClick(object sender, RoutedEventArgs args)
    {
        var folder = await FileMatch.OpenFolder(App.AppMainWindow, SuggestedStartLocation);
        if (folder == null) return;

        if (folder.Path == OriginPath)
        {
            SaveSamePathAction?.Invoke();
        }
        else
        {
            Path = folder.Path;
            SaveAction?.Invoke(Path);
        }
    }


    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}