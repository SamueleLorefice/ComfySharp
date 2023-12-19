using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Dynamic;
using System.Net;
using System.Runtime.CompilerServices;

namespace ComfySharp.Types;

public class NodeDBGenerator {
    private List<ExpandoObject> nodes;
    private ConversionSettings settings;
    
    private List<string> knownTypes = new();
    public List<string> GetKnownTypes() => knownTypes;
    
    private Dictionary<string, List<string>> knownEnums = new();
    public Dictionary<string, List<string>> GetKnownEnums() => knownEnums;
    
    private Dictionary<string, List<string>> knownStringLists = new();
    public Dictionary<string, List<string>> GetKnownStringLists() => knownStringLists;
    
    public int Count => nodes.Count;
    private int typeFields = 0;
    private int enumFields = 0;
    
    public NodeDBGenerator(ConversionSettings settings) {
        nodes = new();   
        this.settings = settings;
    }
    
    public void ResetDb() => nodes.Clear();
    
    /// <summary>
    /// Adds a new type to the list of known types, avoids duplicates
    /// </summary>
    /// <param name="type">type name as string</param>
    private void AddKnownType(string type) {
        if (!knownTypes.Contains(type)) {
            knownTypes.Add(type);
            Console.WriteLine("Added new known type: {0}", type);
        }
        typeFields++;
    }

    /// <summary>
    /// Adds or updates a known enum, avoids duplicates.
    /// </summary>
    /// <param name="type">enum type name as string</param>
    /// <param name="values">Collection of valid string values for this enum</param>
    private void AddKnownEnum(string type, ICollection<string> values) {
        if (!knownEnums.ContainsKey(type)) {
            knownEnums.Add(type, values.ToList());
            Console.WriteLine("Added new known enum: {0}", type);
        }
        else {
            values.ToList().ForEach(value => {
                if (!knownEnums[type].Contains(value)) {
                    knownEnums[type].Add(value);
                    Console.WriteLine("Added new value to already known enum: {0}", value);
                }
            });
        }
        enumFields++;
    }
    
    /// <summary>
    /// Adds or updates a known string list, avoids duplicates.
    /// </summary>
    /// <param name="type">stringList typename</param>
    /// <param name="values">Collection of string values for this StringList</param>
    private void AddKnownStringList(string type, ICollection<string> values) {
        if (!knownStringLists.ContainsKey(type)) {
            knownStringLists.Add(type, values.ToList());
            Console.WriteLine("Added new known stringList: {0}", type);
        }
        else {
            values.ToList().ForEach(value => {
                if (!knownStringLists[type].Contains(value)) {
                    knownStringLists[type].Add(value);
                    Console.WriteLine("Added new value to already known stringList: {0}", value);
                }
            });
        }
    }
    
    public void GenerateClasses(JsonDocument document) {
        foreach (var node in document.RootElement.EnumerateObject()) ScanNode(node);

        Console.WriteLine("List of recognized Types:");
        foreach (var knownType in knownTypes) {
            Console.WriteLine(knownType);
        }
        Console.WriteLine("\nTotal amount of types iterated: {0}\n", typeFields);
        Console.WriteLine("List of recognized Enums:");
        foreach (var knownEnum in knownEnums) {
            Console.WriteLine(knownEnum.Key);
            foreach (var value in knownEnum.Value) {
                Console.Write("\t{0}", value);
            }
            Console.WriteLine();
        }
        Console.WriteLine("\nTotal amount of enums iterated: {0}\n", enumFields);
    }

    /// <summary>
    /// executed for a single node, progresses through the properties of the node
    /// </summary>
    private void ScanNode(JsonProperty node) {
        // each of this are top level properties of the node
        foreach (var property in node.Value.EnumerateObject()) {
            //if this is a list of input properties:
            if (property.Name == "input" && property.Value.ValueKind == JsonValueKind.Object) ScanInputs(property);
            else if (property.Name == "output" && property.Value.ValueKind == JsonValueKind.Array) ScanOutputs(property);
        }
    }

    /// <summary>
    /// Executed on the input property inside a node
    /// </summary>
    private void ScanInputs(JsonProperty input) {
        foreach (var inputType in input.Value.EnumerateObject()) {
            //these are related to the nodes themselves and useless for us
            if (inputType.Name == "hidden") continue;
            // required and optionals have the same structure, so we can parse them the same way 
            if (inputType.Name == "required" || inputType.Name == "optional")
                foreach (var inputProperty in inputType.Value.EnumerateObject()) ScanInputField(inputProperty);
        }
    }
    
    /// <summary>
    /// Executed for each of the elements inside a required or optional input
    /// </summary>
    private void ScanInputField(JsonProperty inputProperty) {
        // if element 0 is a string, this is a type
        if (inputProperty.Value[0].ValueKind == JsonValueKind.String) 
            AddKnownType(inputProperty.Value[0].ToString());
        // else, if element 0 is an array, this is an enum or a list.
        else if (inputProperty.Value[0].ValueKind == JsonValueKind.Array) {
            //if the elements inside the array are not strings, this is a list of objects and might require special handling
            if (inputProperty.Value[0].EnumerateArray().Current.ValueKind != JsonValueKind.String) {
                Console.WriteLine("Encountered a special case: {0}", inputProperty.Name);
                return;
            }
            
            List<string> enumValues = new(); //holds all the values of the enum, valid for both following cases
            inputProperty.Value[0].EnumerateArray().ToList().ForEach(value => enumValues.Add(value.ToString()));
            
            // these are all lists of strings and not enums
            if (settings.EnumConvertAsString.Contains(inputProperty.Name)) {
                AddKnownStringList(inputProperty.Name, enumValues);
            } else {
                AddKnownEnum(inputProperty.Name, enumValues);
            }
        }
    }

    private void ScanOutputs(JsonProperty output) { }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GenerateNode(JsonElement data) {
        var node = new ExpandoObject();
        Console.WriteLine("Generating node: {0}", data.GetProperty("name").GetString() ?? "");
        foreach (var property in data.EnumerateObject()) {
            Console.WriteLine("Adding new property: {0}\nType: {2}\nValue: {1}\n", property.Name, property.Value, property.Value.GetType());
            node.TryAdd(property.Name, property.Value);
        }
        nodes.Add(node);
    }
    
    public void GenerateNode(JsonProperty node) {
        GenerateNode(node.Value);
    }
    
    public void GenerateNodes(JsonDocument document) {
        foreach (var node in document.RootElement.EnumerateObject()) {
            GenerateNode(node.Value);
        }
    }
    
    public void GenerateNodes(string json) {
        var document = JsonDocument.Parse(json);
        GenerateNodes(document);
    }
    
    public void GenerateNodes(ICollection<JsonElement> elements) {
        foreach (var element in elements) {
            GenerateNode(element);
        }
    }
    
    public void GenerateNodes(ICollection<JsonProperty> elements) {
        foreach (var element in elements) {
            GenerateNode(element);
        }
    }
    
    public List<ExpandoObject> GetNodes() => nodes;
    
    
}
