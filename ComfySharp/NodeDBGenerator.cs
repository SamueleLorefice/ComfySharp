using System.Diagnostics;
using System.Text.Json;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Microsoft.CSharp;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;


namespace ComfySharp;

public class NodeDBGenerator {
    readonly private List<ExpandoObject> nodes;
#region Conversion
    readonly private ConversionSettings settings;

    readonly private List<string> knownTypes = new();
    public List<string> GetKnownTypes() => knownTypes;

    readonly private Dictionary<string, List<string>> knownEnums = new();
    public Dictionary<string, List<string>> GetKnownEnums() => knownEnums;

    readonly private Dictionary<string, List<string>> knownStringLists = new();
    public Dictionary<string, List<string>> GetKnownStringLists() => knownStringLists;
    
    public int Count => nodes.Count;
    private int typeFields;
    private int enumFields;
    private int stringListFields;
#endregion

#region CodeGeneration
    readonly private CSharpCodeProvider codeProvider = new();
    readonly private CodeCompileUnit compileUnit = new();
    readonly private CodeNamespace codeNamespace = new("ComfySharp.Types.Generated");
    readonly private List<CodeTypeDeclaration> codeTypes = new();
#endregion

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
            Logger.Info($"Added new known type: {type}");
        } else {
          Logger.Trace($"Skipped already known type: {type}");  
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
            Logger.Info($"Added new known enum: {type}");
            Logger.Trace($"\tAdded values: {string.Join(", ", values)}");
        } else {
            values.ToList().ForEach(value => {
                if (!knownEnums[type].Contains(value)) {
                    knownEnums[type].Add(value);
                    Logger.Info($"Added new value to already known enum: {value}");
                } else {
                    Logger.Trace($"Skipped already known enum value: {type}.{value}");
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
            Logger.Info($"Added new known stringList: {type}");
            Logger.Trace($"\tAdded values: {string.Join(", ", values)}");
        } else {
            values.ToList().ForEach(value => {
                if (!knownStringLists[type].Contains(value)) {
                    knownStringLists[type].Add(value);
                    Logger.Info($"Added new value to already known stringList: {type}");
                } else {
                    Logger.Trace($"Skipped already known stringList value: {type}.{value}");
                }
            });
        }
        stringListFields++;
    }
    
    /// <summary>
    /// Generates C# classes for all known types, enums and stringLists, then also for every node
    /// </summary>
    /// <property name="document">JsonDocument containing the nodes off an objectInfo api call</property>
    public void GenerateClasses(JsonDocument document) {
        Logger.Info("NodeDB Scan phase 1: building Types, Enum and StringLists DataBases");
        var timer = new Stopwatch();
            timer.Start();
        foreach (var node in document.RootElement.EnumerateObject()) ScanNode(node);
        timer.Stop();
        Logger.Info($"NodeDB Scan phase 1: finished in {timer.ElapsedMilliseconds} ms");
        string types = "";
        foreach (var knownType in knownTypes)
            types += $"\t{knownType}";
        Logger.Trace($"List of recognized Types:\n{types}");
        Logger.Info($"Total amount of detected types\\iterated:\t\t {knownTypes.Count}\t\\{typeFields}");
        
        string enums = "";
        foreach (var knownEnum in knownEnums) {
            enums += $"{knownEnum.Key}\n";
            foreach (var value in knownEnum.Value)
                enums += $"\t{value}";
            enums += "\n";
        }
        Logger.Trace($"List of recognized Enums: {enums}");
        Logger.Info($"Total amount of enums detected\\iterated:\t\t {knownEnums.Count}\t\\{enumFields}");
        Logger.Info($"Total amount of stringLists detected\\iterated:\t {knownStringLists.Count}\t\\{knownStringLists.Count}");
        
        Logger.Info("NodeDB Scan phase 2: generating types");
        timer.Restart();
        Logger.Info("Enum Generation starting...");
        CodeNamespace enumNs = new("ComfySharp.Types.Generated.Enums");
        compileUnit.Namespaces.Add(enumNs);
        foreach (var knownEnum in knownEnums) {
            GenerateEnum(knownEnum.Key, knownEnum.Value, enumNs);
        }
        GenerateEnum("BaseTypes", knownTypes, enumNs);//TODO: make a proper method that handles the edge cases, like * and comma separated multi instances
        Logger.Info($"Enum Generation finished. Took {timer.ElapsedMilliseconds} ms");
        timer.Restart();
        //compileUnit.Namespaces.Add(codeNamespace);
        //ns.Imports.AddRange(new CodeNamespaceImport[] { new ("System"), new ("System.Collections.Generics"), new ("ComfySharp") });
    }

    private void GenerateEnum(string name, List<string> enumValues, CodeNamespace ns) {
        string path = Path.Combine(Environment.CurrentDirectory, settings.CodeOutputFolderName, $"{name}.cs");
        Logger.Debug($"Generating enum {name} w\\ {enumValues.Count} values.\n" +
                     $"\t\tPath: {path}");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        IndentedTextWriter writer = new(File.CreateText(path));
        CodeTypeDeclaration enumType = new() {
            Name = name,
            IsEnum = true,
            IsPartial = false,
            Attributes = MemberAttributes.Public
        };
        
        foreach (var value in enumValues) 
            enumType.Members.Add(new CodeMemberField(name, value));
        
        ns.Types.Add(enumType); //add the enum to the namespace
        //generate the code
        codeProvider.GenerateCodeFromCompileUnit(compileUnit, writer, new() {
            BlankLinesBetweenMembers = false,
            BracingStyle = "Block",
            VerbatimOrder = true,
            ElseOnClosing = false,
            IndentString = "\t"
        });
        ns.Types.Remove(enumType);//remove the enum from the namespace, so it doesn't get generated twice
        codeTypes.Add(enumType);//add the enum to the list of types, so we can generate the nodes later
        writer.InnerWriter.Flush();//flush the writer
        writer.Close();//close the writer and release the file lock
    }

    /// <summary>
    /// executed for a single node, progresses through the properties of the node
    /// </summary>
    private void ScanNode(JsonProperty node) {
        Logger.Debug($"Scanning node: {node.Name}"); 
        // if this node is blacklisted, skip it
        if (settings.BlacklistedNodes.Contains(node.Name)) {
            Logger.Debug($"Skipping blacklisted node: {node.Name}");
            return;
        }
        
        // each of this are top level properties of the node
        foreach (var property in node.Value.EnumerateObject()) {
            //if this is a list of input properties:
            if (property is { Name: "input", Value.ValueKind: JsonValueKind.Object }) ScanInputs(property);
            else if (property is { Name: "output", Value.ValueKind: JsonValueKind.Array }) ScanOutputs(property);
        }
    }

    /// <summary>
    /// Executed on the input property inside a node
    /// </summary>
    private void ScanInputs(JsonProperty input) {
        Logger.Trace($"Scanning inputs of node: {input.Name}");
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
        Logger.Trace($"Scanning input field: {inputProperty.Name}");
        
        // if element 0 is a string, this is a type
        if (inputProperty.Value[0].ValueKind == JsonValueKind.String) {
            AddKnownType(inputProperty.Value[0].ToString());
            return;
        }
        
        // if element 0 is an array, this is an enum or a list.
        if (inputProperty.Value[0].ValueKind == JsonValueKind.Array) {
            
            var firstElement = inputProperty.Value.EnumerateArray().ToArray()[0];
            
            if (firstElement.GetArrayLength() > 0) {
                //if the elements inside the array are not strings, this is a list of objects and might require special handling
                if (firstElement[0].ValueKind != JsonValueKind.String) {
                    Logger.Info($"Encountered a special case: {inputProperty.Name}" +
                                $"\nFirst element of array is not a string, but a {firstElement[0].ValueKind}");
                    return;
                }

                List<string> enumValues = new(); //holds all the values of the enum, valid for both following cases
                firstElement.EnumerateArray().ToList().ForEach(value => enumValues.Add(value.ToString()));

                // these are all lists of strings and not enums
                if (settings.EnumConvertAsString.Contains(inputProperty.Name)) {
                    AddKnownStringList(inputProperty.Name, enumValues);
                }
                else {
                    AddKnownEnum(inputProperty.Name, enumValues);
                }
            } else {
                //if the array is empty, this is a list of unknown and might require special handling later on, for now, skip it
                Logger.Info($"Encountered a special case: {inputProperty.Name}" +
                            "\nFirst element of array is empty"+
                            "\nAdding to known types as empty list...");
                AddKnownStringList(inputProperty.Name, new List<string>());
            }
        }
    }

    /// <summary>
    /// Executed for each output array of every node.
    /// </summary>
    private void ScanOutputs(JsonProperty output) {
        Logger.Trace($"Scanning outputs of node: {output.Name}");
        foreach (var outputType in output.Value.EnumerateArray()) AddKnownType(outputType.GetString()!);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GenerateNode(JsonElement data) {
        var node = new ExpandoObject();
        Logger.Info($"Generating node: {data.GetProperty("name").GetString() ?? ""}" );
        foreach (var property in data.EnumerateObject()) {
            Logger.Info($"Adding new property: {property.Name}\nType: {property.Value}\nValue: {property.Value.GetType()}\n");
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
