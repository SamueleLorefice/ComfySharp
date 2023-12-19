using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ComfySharp;

[DataContract, JsonSerializable(typeof(ConversionSettings))]
public class ConversionSettings {
    [DataMember(IsRequired = true)] public List<string> EnumConvertAsString = new();
    [DataMember(IsRequired = true)] public List<string> EnumConvertAsBool = new();
    static JsonSerializerOptions jsonOpt = new() { WriteIndented = true, IncludeFields = true };
    public static ConversionSettings FromFile(string path) {
        if (!File.Exists(path)) throw new FileNotFoundException("Could not find settings file", path);
        string json = File.ReadAllText(path);
        ConversionSettings? settings = FromJson(json);
        return settings;
    }

    public static ConversionSettings FromJson(string json) {
        ConversionSettings? settings = JsonSerializer.Deserialize<ConversionSettings>(json, jsonOpt);
        if (settings is null) throw new NullReferenceException("Could not deserialize settings file");
        return settings;
    }

    public static string ToJson(ConversionSettings settings) => JsonSerializer.Serialize(settings);

    public void Save(string path) => File.WriteAllText(path, ToJson());

    public string ToJson() => JsonSerializer.Serialize(this, jsonOpt);

}