using System.Runtime.Serialization;

namespace ComfySharp.Types;

[DataContract]
public enum DirType {
    [DataMember(Name = "input")] Input,
    [DataMember(Name = "temp")] Temp,
    [DataMember(Name = "output")] Output
}