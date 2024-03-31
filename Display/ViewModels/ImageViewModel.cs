using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Display.Helper.Data;
using Display.Helper.UI;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Display.Models.Api.OneOneFive.File;
using Display.Models.Dto.Media;
using Display.Models.Dto.OneOneFive;
using Display.Models.Vo.OneOneFive;

namespace Display.ViewModels;

internal partial class ImageViewModel : ObservableObject
{
    [ObservableProperty]
    private List<SubImageModel> _photos = [];

    public readonly ObservableCollection<FilesInfo> Infos = [];

    [ObservableProperty]
    private bool _loading;

    [ObservableProperty]
    private int _progressValue;

    [ObservableProperty]
    private bool _isEnableDownButton;

    [ObservableProperty]
    private SubImageModel _currentPhotoModel;

    public async Task SetDataAndCurrentIndex(List<FilesInfo> files, int currentIndex)
    {
        _currentIndex = currentIndex;

        var subImageViewModels = new List<SubImageModel>();
        Infos.Clear();
        foreach (var info in files)
        {
            Infos.Add(info);
            var photoViewModel = new SubImageModel(info);
            subImageViewModels.Add(photoViewModel);
        }

        Photos = subImageViewModels;

        await LoadImage();
    }

    private int _currentIndex = -1;

    [RelayCommand]
    private void ClearAllPhotos()
    {
        Photos.Clear();
        Infos.Clear();
        _currentIndex = -1;
        IsEnableDownButton = false;
    }

    [RelayCommand]
    private async Task PreparePhotoAsync(int photoIndex)
    {
        if (photoIndex < 0 || Photos.Count <= photoIndex) return;

        if (_currentIndex == photoIndex) return;

        _currentIndex = photoIndex;

        await LoadImage();
    }

    private async Task LoadImage()
    {
        ProgressValue = 0;
        Loading = true;
        IsEnableDownButton = false;

        CurrentPhotoModel = Photos[_currentIndex];
        if (CurrentPhotoModel.IsDowning) return;

        Debug.WriteLine($"正在加载{_currentIndex}");

        var progress = new Progress<int>(p => ProgressValue = p);

        await CurrentPhotoModel.LoadThumbnailFromInternetAsync(progress);

        Loading = false;
        if (CurrentPhotoModel.Thumbnail is not null) IsEnableDownButton = true;
    }

    [RelayCommand]
    private async Task ExportCurrentImageAsync(UIElement element)
    {
        var fileName = CurrentPhotoModel.FileInfo.Name;

        if (string.IsNullOrEmpty(fileName)) return;


        var window = WindowHelper.GetWindowForElement(element);
        if (window == null) return;

        var saveFile = await SelectSaveImageAsync(fileName, window);
        if (saveFile is null) return;

        var isSuccess = await LocalCacheHelper.ExportFile(fileName, saveFile);

        if (!isSuccess)
        {
            IsEnableDownButton = false;
            await saveFile.DeleteAsync();
        }
    }

    [RelayCommand]
    private async Task OpenWithOtherApplicationAsync()
    {
        var filePath = LocalCacheHelper.GetCacheFilePath(CurrentPhotoModel.FileInfo.Name);
        if (filePath == null) return;
        await Windows.System.Launcher.LaunchUriAsync(new Uri(filePath));
    }

    private async Task<StorageFile> SelectSaveImageAsync(string name, Window window)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(name);
        var extension = Path.GetExtension(name);
        var savePicker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            SuggestedFileName = fileNameWithoutExtension
        };

        savePicker.FileTypeChoices.Add("图片", new List<string> { extension });


        var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, windowHandle);

        var result = await savePicker.PickSaveFileAsync();
        return result;
    }

}