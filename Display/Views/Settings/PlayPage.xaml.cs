using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Display.Models.Data;
using Display.Models.Data.Enums;
using Display.Models.Settings.Options;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.Settings;

public sealed partial class PlayPage : INotifyPropertyChanged
{
    private Player[] _players;
    private Player _currentPlayer;
    private Player CurrentPlayer
    {
        get => _currentPlayer;
        set
        {
            if (_currentPlayer == value) return;
            _currentPlayer = value;
            AppSettings.PlayerSelection = _currentPlayer.Own;
            OnPropertyChanged();
        }
    }

    public PlayPage()
    {
        this.InitializeComponent();

        InitOption();
    }

    private void InitOption()
    {
        // ������
        _players =
        [
            new Player
            {
                Own = PlayerType.WebView,
                Name = "WebView",
                IsNeedPath = false,
                IsLoadSubOptionOn = false,
                IsChangeQualityEnable = false
            },
            new Player
            {
                Own = PlayerType.PotPlayer,
                Name = "PotPlayer",
                Path = AppSettings.PotPlayerExePath,
                SavePathAction = path=>AppSettings.PotPlayerExePath = path,
                ResetPathFunc = () => Constant.DefaultSettings.Player.ExePath.PotPlayer
            },
            new Player
            {
                Own = PlayerType.Vlc,
                Name = "VLC",
                Path = AppSettings.VlcExePath,
                SavePathAction = path=>AppSettings.VlcExePath = path,
                ResetPathFunc = () => Constant.DefaultSettings.Player.ExePath.Vlc
            },
            new Player
            {
                Own = PlayerType.Mpv,
                Name = "MPV",
                Path = AppSettings.MpvExePath,
                SavePathAction = path=>AppSettings.MpvExePath = path,
                ResetPathFunc = () => Constant.DefaultSettings.Player.ExePath.Mpv
            },
            new Player
            {
                Own = PlayerType.MediaElement,
                Name = "MediaElement",
                IsNeedPath = false,
            }
        ];

        // ��ǰѡ��
        CurrentPlayer = _players.FirstOrDefault(player => player.Own == AppSettings.PlayerSelection);

    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}