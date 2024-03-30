using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Display.Models.Dto.Settings.Options;
using Display.Models.Enums;
using Display.Providers;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages.Settings;

public sealed partial class PlayPage
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
        }
    }

    public PlayPage()
    {
        InitOption();

        this.InitializeComponent();
    }

    private void InitOption()
    {
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
                Own = PlayerType.MediaElement,
                Name = "MediaElement",
                IsNeedPath = false,
            }
            ,
            new Player
            {
                Own = PlayerType.PotPlayer,
                Name = "PotPlayer",
                Icon = new BitmapIcon
                {
                    ShowAsMonochrome = false,
                    UriSource = new Uri("ms-appx:///Assets/Logo/potplayer-logo.png")
                },
                Path = AppSettings.PotPlayerExePath,
                SavePathAction = path=>AppSettings.PotPlayerExePath = path,
                ResetPathFunc = () => Constants.DefaultSettings.Player.ExePath.PotPlayer
            },
            new Player
            {
                Own = PlayerType.Vlc,
                Name = "VLC",
                Icon = new BitmapIcon
                {
                    ShowAsMonochrome = false,
                    UriSource = new Uri("ms-appx:///Assets/Logo/vlc-logo.png")
                },
                Path = AppSettings.VlcExePath,
                SavePathAction = path=>AppSettings.VlcExePath = path,
                ResetPathFunc = () => Constants.DefaultSettings.Player.ExePath.Vlc
            },
            new Player
            {
                Own = PlayerType.Mpv,
                Name = "mpv",
                Icon = new BitmapIcon
                {
                    ShowAsMonochrome = false,
                    UriSource = new Uri("ms-appx:///Assets/Logo/mpv-logo.png")
                },
                Path = AppSettings.MpvExePath,
                SavePathAction = path=>AppSettings.MpvExePath = path,
                ResetPathFunc = () => Constants.DefaultSettings.Player.ExePath.Mpv
            }
        ];

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