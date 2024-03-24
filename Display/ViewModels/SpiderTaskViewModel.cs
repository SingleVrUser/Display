using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Display.Managers;

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
    public void Start()
    {
        InitView();

        //string[] names = ["abw-123", "abp-235", "eyan-200"];
        //MaxProgressValue += names.Length;
        //_manager.AddTask(names);
    }

    private void InitView()
    {
        IsTaskRunning = true;
        ShowPause = false;
    }

    [RelayCommand]
    public void Cancel()
    {
        ShowPause = true;
        _manager.CancelTask();
    }
}