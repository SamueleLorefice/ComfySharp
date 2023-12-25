using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ComfySharp.Types;

public interface INode {
    //Inputs
    [DataMember(Name = "input")]
    public Inputs Inputs { get; }
    //Outputs
    [DataMember(Name = "output")]
    public string[] Outputs { get; }
    [DataMember(Name = "output_is_list")]
    public bool[] OutputIsList { get; }
    [DataMember(Name = "output_name")]
    public string[] OutputNames { get; }
    //Metadata
    [DataMember(Name = "name")]
    public string Name { get; }
    [DataMember(Name = "display_name")]
    public string DisplayName { get; }
    [DataMember(Name = "description")]
    public string Description { get; }
    [DataMember(Name = "category")]
    public string Category { get; }
    [DataMember(Name = "output_node")]
    public bool IsOutputNode { get; }
    
    
}