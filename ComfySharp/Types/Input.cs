using System.Runtime.Serialization;

namespace ComfySharp.Types;

[DataContract]
public struct Input {
    [DataMember]
    public List<InputField> Required { get; set; }
    [DataMember]
    public List<InputField> Optional { get; set; }
    [DataMember]
    public List<InputField> Hidden { get; set; }

    public Input() {
        Required = new();
        Optional = new();
        Hidden = new();
    }
}