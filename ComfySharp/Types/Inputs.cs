using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ComfySharp.Types;

[DataContract, JsonSerializable(typeof(Inputs))]
public class Inputs {
    [DataMember(Name = "required")]
    public List<IInput> Required { get; set; } = new();
    [DataMember(Name = "optional", IsRequired = false)]
    public List<IInput>? Optional { get; set; }
    [DataMember(Name = "hidden", IsRequired = false)]
    public List<IInput>? Hidden { get; set; }
}