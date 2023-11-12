using System.Text.Json;
using ComfySharp.Types;

namespace ComfySharp; 

public static class ObjectInfoParser {
    public static void Parse(JsonDocument document, out List<Node> nodes) {
        nodes = new List<Node>();
        foreach (var node in document.RootElement.EnumerateObject()) {
            Node n = new();
            n.Name = node.Name;

            foreach (var prop in node.Value.EnumerateObject()) {
                switch (prop.Name) {
                    case "input":
                        n.Input = new();

                        foreach (var input in prop.Value.EnumerateObject()) {
                            switch (input.Name) {
                                case "required":
                                    foreach (var field in input.Value.EnumerateObject()) {
                                        InputField f = new();
                                        f.Name = field.Name;
                                        f.Type = Enum.Parse<PrimitiveType>(field.Value.GetString() ?? "");
                                        n.Input.Required.Add(f);
                                    }
                                    break;
                                case "optional":
                                    foreach (var field in input.Value.EnumerateObject()) {
                                        InputField f = new();
                                        f.Name = field.Name;
                                        f.Type = Enum.Parse<PrimitiveType>(field.Value.GetString() ?? "");
                                        n.Input.Optional.Add(f);
                                    }
                                    break;
                                case "hidden":
                                    foreach (var field in input.Value.EnumerateObject()) {
                                        InputField f = new();
                                        f.Name = field.Name;
                                        f.Type = Enum.Parse<PrimitiveType>(field.Value.GetString() ?? "");
                                        n.Input.Hidden.Add(f);
                                    }
                                    break;
                            }
                        }
                        break;
                    case "output":
                        foreach (var output in prop.Value.EnumerateObject()) {
                            n.Outputs.Add(Enum.Parse<PrimitiveType>(output.Value.GetString() ?? ""));
                            n.OutputIsList.Add(output.Value.GetBoolean());
                            n.OutputNames.Add(output.Name);
                        }
                        break;
                    case "display_name":
                        n.DisplayName = prop.Value.GetString() ?? "";
                        break;
                    case "description":
                        n.Description = prop.Value.GetString() ?? "";
                        break;
                    case "category":
                        n.Category = prop.Value.GetString() ?? "";
                        break;
                    case "output_node":
                        n.IsOutputNode = prop.Value.GetBoolean();
                        break;
                }
            }
        }
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
}