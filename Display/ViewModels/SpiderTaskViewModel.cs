using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Display.Managers;
using System.Collections.ObjectModel;

namespace Display.ViewModels;

internal partial class SpiderTaskViewModel : ObservableObject
{
    private readonly SpiderManager _manager = App.GetService<SpiderManager>();

    [ObservableProperty]
    private int _maxProgressValue;

    [ObservableProperty]
    private int _currentProgressValue;

    [ObservableProperty]
    private bool _showPause;

    [ObservableProperty]
    private bool _isTaskRunning;

    [ObservableProperty]
    private string[] _spiderNames;

    public ObservableCollection<string> FailNameCollection;

    public SpiderTaskViewModel()
    {
        InitData();
    }

    private void InitData()
    {
        MaxProgressValue = _manager.LeftNameCount;
        IsTaskRunning = MaxProgressValue > 0;
        _manager.ItemTaskSuccessAction += _ => CurrentProgressValue++;
        _manager.TaskCompletedAction += () =>
        {
            if (ShowPause) return;

            IsTaskRunning = false;
            //MaxProgressValue = 0;
            //CurrentProgressValue = 0;
        };
        SpiderNames = _manager.SpiderNames;
        FailNameCollection = new ObservableCollection<string>(_manager.GetFailNames());
    }


    [RelayCommand]
    private void Cancel()
    {
        ShowPause = true;
        _manager.CancelTask();
    }
}