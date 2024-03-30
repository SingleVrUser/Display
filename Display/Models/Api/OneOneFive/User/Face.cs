using Newtonsoft.Json;

namespace Display.Models.Api.OneOneFive.User;

public class Face
{
    [JsonProperty("face_l")]
    public string FaceL { get; set; }
    
    [JsonProperty("face_m")]
    public string FaceM { get; set; }
    
    [JsonProperty("face_s")]
    public string FaceS { get; set; }
}