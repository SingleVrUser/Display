using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Display.Models.Enums;
using Display.Models.Enums.OneOneFive;
using Newtonsoft.Json;

namespace Display.Models.Dto.OneOneFive;

public class ActorInfo : INotifyPropertyChanged
{
    [JsonProperty(propertyName: "id")]
    public int Id { get; set; }

    [JsonProperty(propertyName: "name")]
    public string Name { set; get; }

    public List<string> OtherNames { get; set; }

    [JsonProperty(propertyName: "is_woman")]
    public int IsWoman { set; get; } = 1;

    [JsonProperty(propertyName: "birthday")]
    public string Birthday { set; get; } = string.Empty;

    [JsonProperty(propertyName: "bwh")]
    public string Bwh { set; get; } = string.Empty;

    [JsonProperty(propertyName: "bust")]
    public int Bust { set; get; }

    [JsonProperty(propertyName: "waist")]
    public int Waist { set; get; }

    [JsonProperty(propertyName: "hips")]
    public int Hips { set; get; }

    [JsonProperty(propertyName: "height")]
    public int Height { set; get; }

    [JsonProperty(propertyName: "works_count")]
    public int WorksCount { set; get; }

    [JsonProperty(propertyName: "work_time")]
    public string WorkTime { set; get; } = string.Empty;

    private string _profilePath { set; get; } = string.Empty;

    [JsonProperty(propertyName: "prifile_path")]
    public string ProfilePath
    {
        get => _profilePath;
        set
        {
            var path = !string.IsNullOrEmpty(value) ? value : Constants.FileType.NoPicturePath;
            if (_profilePath == path) return;

            _profilePath = path;

            OnPropertyChanged();
        }
    }

    [JsonProperty(propertyName: "blog_url")]
    public string BlogUrl { set; get; } = string.Empty;

    [JsonProperty(propertyName: "info_url")]
    public string InfoUrl { get; set; } = string.Empty;

    [JsonProperty(propertyName: "is_like")]
    public int IsLike { set; get; } = 0;

    [JsonProperty(propertyName: "addtime")]
    public long AddTime { set; get; } = DateTimeOffset.Now.ToUnixTimeSeconds();

    [JsonProperty(propertyName: "video_count")]
    public int VideoCount { get; set; }

    public string ImageUrl { get; set; }

    private Status _status = Status.BeforeStart;

    public Status Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    private string _genderInfo;

    public string GenderInfo
    {
        get => _genderInfo;
        set
        {
            _genderInfo = value;
            OnPropertyChanged();
        }
    }


    private string _ageInfo;

    public string AgeInfo
    {
        get
        {
            return _ageInfo;
        }
        set
        {
            _ageInfo = value;
            OnPropertyChanged();
        }
    }

    public string Initials => string.Empty + Name.FirstOrDefault();

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}