using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataAccess.Models.Entity;
using Display.Models.Api.OneOneFive.File;
using Newtonsoft.Json;

namespace Display.Models.Entities.OneOneFive;

public class FailInfo : INotifyPropertyChanged
{
    [JsonProperty(propertyName: "pc")]
    public string PickCode { get; set; }

    [JsonProperty(propertyName: "is_like")]
    public int IsLike { get; set; }

    private double _score;

    [JsonProperty(propertyName: "score")]
    public double Score
    {
        get => _score;
        set
        {
            _score = value;
            OnPropertyChanged();
        }
    }

    [JsonProperty(propertyName: "look_later")]
    public long LookLater { get; set; } = 0;

    [JsonProperty(propertyName: "image_path")]
    public string ImagePath { get; set; } = Constants.FileType.NoPicturePath;

    [JsonProperty(propertyName: nameof(Datum))]
    public FilesInfo Datum { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}