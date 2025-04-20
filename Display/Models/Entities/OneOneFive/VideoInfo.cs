using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

//namespace Display.Models.Entities.OneOneFive;

///// <summary>
///// 视频详细信息
///// </summary>
//public class VideoInfo : INotifyPropertyChanged
//{
//    private string _trueName;

//    [JsonProperty(propertyName: "truename")]
//    public string TrueName
//    {
//        get => _trueName;
//        set
//        {
//            _trueName = value;
//            OnPropertyChanged();
//        }
//    }

//    [JsonProperty(propertyName: "title")]
//    public string Title { get; set; }

//    private string _releaseTime;

//    [JsonProperty(propertyName: "releasetime")]
//    public string ReleaseTime
//    {
//        get => _releaseTime;
//        set
//        {
//            _releaseTime = value;
//            OnPropertyChanged();
//        }
//    }

//    private string _lengthTime;

//    [JsonProperty(propertyName: "lengthtime")]
//    public string Lengthtime
//    {
//        get => _lengthTime;
//        set
//        {
//            _lengthTime = value;
//            OnPropertyChanged();
//        }
//    }

//    private string _director;

//    [JsonProperty(propertyName: "director")]
//    public string Director
//    {
//        get => _director;
//        set
//        {
//            _director = value;
//            OnPropertyChanged();
//        }
//    }

//    private string _producer;

//    [JsonProperty(propertyName: "producer")]
//    public string Producer
//    {
//        get => _producer;
//        set
//        {
//            _producer = value;
//            OnPropertyChanged();
//        }
//    }

//    private string _publisher;

//    [JsonProperty(propertyName: "publisher")]
//    public string Publisher
//    {
//        get => _publisher;
//        set
//        {
//            _publisher = value;
//            OnPropertyChanged();
//        }
//    }

//    private string _series;

//    [JsonProperty(propertyName: "series")]
//    public string Series
//    {
//        get => _series;
//        set
//        {
//            _series = value;
//            OnPropertyChanged();
//        }
//    }

//    private string _category;

//    [JsonProperty(propertyName: "category")]
//    public string Category
//    {
//        get => _category;
//        set
//        {
//            _category = value;
//            OnPropertyChanged();
//        }
//    }

//    private string _actor { get; set; } = string.Empty;

//    [JsonProperty(propertyName: "actor")]
//    public string Actor
//    {
//        get => _actor;
//        set
//        {
//            _actor = value;
//            OnPropertyChanged();
//        }

//    }

//    private string _imageUrl;

//    [JsonProperty(propertyName: "imageurl")]
//    public string ImageUrl
//    {
//        get => _imageUrl;
//        set
//        {
//            if (_imageUrl == value) return;
//            _imageUrl = value;
//            OnPropertyChanged();
//        }
//    }


//    [JsonProperty(propertyName: "sampleImageList")]
//    public string SampleImageList { get; set; }

//    private string _imagePath;

//    [JsonProperty(propertyName: "imagepath")]
//    public string ImagePath
//    {
//        get => _imagePath;
//        set
//        {
//            if (_imagePath == value) return;

//            var path = value;
//            _imagePath = !string.IsNullOrEmpty(path) ? path : Constants.FileType.NoPicturePath;
//            OnPropertyChanged();
//        }
//    }


//    [JsonProperty(propertyName: "busurl")]
//    public string busUrl { get; set; }

//    private long _lookLater = 0;

//    [JsonProperty(propertyName: "look_later")]
//    public long LookLater
//    {
//        get => _lookLater;
//        set
//        {
//            _lookLater = value;
//            OnPropertyChanged();
//        }
//    }

//    private double _score = -1;

//    [JsonProperty(propertyName: "score")]
//    public double Score
//    {
//        get => _score;
//        set
//        {
//            _score = value;
//            OnPropertyChanged();
//        }
//    }


//    private int _isLike = 0;

//    [JsonProperty(propertyName: "is_like")]
//    public int IsLike
//    {
//        get => _isLike;
//        set
//        {
//            _isLike = value;
//            OnPropertyChanged();
//        }
//    }

//    [JsonProperty(propertyName: "addtime")]
//    public long AddTime { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
//    public int IsWm { get; set; } = -1;


//    public event PropertyChangedEventHandler PropertyChanged;
//    public void OnPropertyChanged([CallerMemberName] string propertyName = "")
//    {
//        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//    }

//}