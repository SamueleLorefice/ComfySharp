using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ComfySharp.Types;

[DataContract, JsonSerializable(typeof(Node))]
public class Node {
    [DataMember(Name = "name")]
    public string Name { get; set; }
    [DataMember(Name = "input")]
    public Inputs Inputs { get; set; }
    [DataMember(Name = "output")]
    public List<string> Outputs { get; set; }
    [DataMember(Name = "output_is_list")]
    public List<bool> OutputIsList { get; set; }
    [DataMember(Name = "output_name")]
    public List<string> OutputNames { get; set; }
    [DataMember(Name = "display_name")]
    public string DisplayName { get; set; }
    [DataMember(Name = "description")]
    public string Description { get; set; }
    [DataMember(Name = "category")]
    public string Category { get; set; }
    [DataMember(Name = "output_node")]
    public bool IsOutputNode { get; set; }
    
    public Node() {
        Name = "";
        Inputs = new();
        Outputs = new();
        OutputIsList = new();
        OutputNames = new();
        DisplayName = "";
        Description = "";
        Category = "";
        IsOutputNode = false;
    }
}