using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Display.Data;
using Display.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Display.ViewModels
{
    public partial class ImageViewModel:ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<SubImageViewModel> _photos = new();

        public readonly ObservableCollection<FilesInfo> Infos = new();

        [ObservableProperty]
        private bool _loading;

        [ObservableProperty]
        private int _progressValue;

        [ObservableProperty]
        private bool _isEnableDownButton;

        [ObservableProperty]
        private SubImageViewModel _currentPhotoViewModel;
        
        public async Task SetDataAndCurrentIndex(List<FilesInfo> files, int currentIndex)
        {
            _currentIndex = currentIndex;

            var subImageViewModels = new List<SubImageViewModel>();
            Infos.Clear();
            foreach (var info in files)
            {
                Infos.Add(info);
                var photoViewModel = new SubImageViewModel(info.PickCode);
                subImageViewModels.Add(photoViewModel);
            }

            Photos = new ObservableCollection<SubImageViewModel>(subImageViewModels);

            await LoadImage();
        }

        private int _currentIndex = -1;

        public ImageViewModel()
        {
        }   

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

            CurrentPhotoViewModel = Photos[_currentIndex];
            if (CurrentPhotoViewModel.IsDowning) return;

            Debug.WriteLine($"正在加载{_currentIndex}");

            var progress = new Progress<int>(p => ProgressValue = p);

            await CurrentPhotoViewModel.LoadThumbnailFromInternetAsync(progress);

            Loading = false;
            if (CurrentPhotoViewModel.Thumbnail is not null) IsEnableDownButton = true;
        }

        [RelayCommand]
        private async Task ExportCurrentImageAsync()
        {
            var fileName = CurrentPhotoViewModel.FileName;

            if (string.IsNullOrEmpty(fileName)) return;

            var saveFile = await SelectSaveImageAsync(fileName);
            if (saveFile is null) return;

            var isSuccess = await LocalCacheHelper.ExportFile(fileName, saveFile);

            if (!isSuccess)
            {
                IsEnableDownButton = false;
                await saveFile.DeleteAsync();
            }
        }

        private async Task<StorageFile> SelectSaveImageAsync(string name)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(name);
            var extension = Path.GetExtension(name);
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = fileNameWithoutExtension
            };

            savePicker.FileTypeChoices.Add("图片", new List<string> { extension });

            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(App.AppMainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, windowHandle);

            var result = await savePicker.PickSaveFileAsync();
            return result;
        }

    }
}
