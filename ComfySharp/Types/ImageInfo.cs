using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ComfySharp.Types;

[DataContract, JsonSerializable(typeof(ImageInfo))]
public class ImageInfo {
    [DataMember(Name = "name")]
    public string Name { get; set; }
    [DataMember(Name = "subfolder")]
    public string Subfolder { get; set; }
    [DataMember(Name = "type")]
    public DirType Type { get; set; }
}