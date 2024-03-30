using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage.Pickers;
using Display.Helper.FileProperties.Name;
using Display.Models.Dto.Settings.Options;
using Microsoft.UI.Xaml.Controls;

namespace Display.Models.Vo.Down;

internal abstract class BaseDownloader : INotifyPropertyChanged
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string ApiPlaceholder { get; }
    public abstract string ApiText { get; set; }
    public abstract string SavePath { get; set; }

    public void SetApiSettingClick()
    {
        var apiSetting = MatchApiSetting();
        if (apiSetting == null)
        {
            ShowWarn("Api设置", "请检查格式是否正确");
            return;
        }
        SetApiSetting(apiSetting);
    }

    public void OpenSavePath()
        => FileMatch.OpenFolderWithSystemExplorer(SavePath);

    public async void SelectSavePath()
    {
        var folder = await FileMatch.OpenFolder(App.AppMainWindow, PickerLocationId.PicturesLibrary);
        if (folder is null) return;

        if (folder.Path == SavePath)
        {
            ShowInfo("选择目录与原目录相同，未修改");
        }
        else
        {
            SavePath = folder.Path;
            OnPropertyChanged(nameof(SavePath));
            SetSavePath(folder.Path);
        }
    }

    public void ClearSavePath()
    {
        SetSavePath(null);
        SavePath = string.Empty;
        OnPropertyChanged(nameof(SavePath));
    }

    public void CheckSettingOk()
    {
        if (string.IsNullOrWhiteSpace(ApiText))
        {
            ShowWarn("Api设置", "不能为空");
            return;
        }

        var curSetting = MatchApiSetting();

        if (curSetting is null)
        {
            ShowWarn("Api设置", "请检查格式是否正确");
            return;
        }

        CheckApiOk(curSetting);
    }

    public Action<string, string, InfoBarSeverity> ShowMessageAction;
    protected static string ReadApiSetting(DownApiSettings setting)
        => setting is null ? string.Empty : $"{setting.ApiUrl} (部分隐藏)";
    protected abstract DownApiSettings MatchApiSetting();
    protected abstract void SetSavePath(string newPath);
    protected abstract void SetApiSetting(DownApiSettings settings);
    protected abstract void CheckApiOk(DownApiSettings curSetting);

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

    private void ShowInfo(string message)
        => ShowMessageAction?.Invoke(message, null, InfoBarSeverity.Informational);

    protected void ShowError(string title, string message)
        => ShowMessageAction?.Invoke(title, message, InfoBarSeverity.Error);

    protected void ShowWarn(string title, string message)
        => ShowMessageAction?.Invoke(title, message, InfoBarSeverity.Warning);

    protected void ShowSuccess(string message)
        => ShowMessageAction?.Invoke(message, null, InfoBarSeverity.Success);

}