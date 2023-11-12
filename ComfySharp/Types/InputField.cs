using System.Runtime.Serialization;

namespace ComfySharp.Types;

public struct InputField {
    [DataMember(Name = "name")]
    public string Name { get; set; }
    
    public PrimitiveType Type { get; set; }
}