using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ComfySharp.Types;

[DataContract, JsonSerializable(typeof(InputField))]
public class InputField : IInput {
    [DataMember(Name = "name", IsRequired = false)]
    public string Name { get; set; }
    [DataMember(Name = "type", IsRequired = false)]
    public string Type { get; set; }

    public InputField(string name, string type) {
        Name = name;
        Type = type;
    }
}