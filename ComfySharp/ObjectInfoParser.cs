using System.Dynamic;
using System.Text.Json;
using ComfySharp.Types;

namespace ComfySharp; 
[Obsolete("This class is obsolete, use NodeDBGenerator instead",false)]
public static class ObjectInfoParser {
    
    public static void Parse(JsonDocument document, out List<ExpandoObject> nodes) {
        NodeDBGenerator dbGenerator = new(new ConversionSettings());
        dbGenerator.GenerateNodes(document);
        nodes = dbGenerator.GetNodes();
        dbGenerator.GenerateClasses(document);
    }
    
    private static void ParseNode(JsonElement node, out Node n) {
        n = new();
        
        n.Name = node.GetProperty("name").GetString() ?? "";
        n.Input = ParseInput(node.GetProperty("input"));
        n.Outputs = ParseOutputs(node.GetProperty("output"));
        n.OutputIsList = ParseOutputIsList(node.GetProperty("output"));
        n.OutputNames = ParseOutputNames(node.GetProperty("output"));
        n.DisplayName = node.GetProperty("display_name").GetString() ?? "";
        n.Description = node.GetProperty("description").GetString() ?? "";
        n.Category = node.GetProperty("category").GetString() ?? "";
        n.IsOutputNode = node.GetProperty("output_node").GetBoolean();
    }
    static private List<string> ParseOutputNames(JsonElement getProperty) {
        List<string> outputNames = new();
        foreach (var output in getProperty.EnumerateArray()) {
            outputNames.Add(output.GetProperty("name").GetString() ?? "");
        }
        return outputNames;
    }
    static private List<bool> ParseOutputIsList(JsonElement getProperty) {
        throw new NotImplementedException();
    }
    static private List<PrimitiveType> ParseOutputs(JsonElement getProperty) {
        throw new NotImplementedException();
    }
    static private Input ParseInput(JsonElement getProperty) {
        throw new NotImplementedException();
    }
}