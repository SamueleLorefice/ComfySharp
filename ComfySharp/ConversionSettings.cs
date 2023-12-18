using System.Text.Json;

namespace ComfySharp;

[Serializable]
public class ConversionSettings {
    public List<string> EnumConvertAsString = new();
    public List<string> EnumConvertAsBool = new();
    
    public static ConversionSettings FromFile(string path) {
        if (!File.Exists(path)) throw new FileNotFoundException("Could not find settings file", path);
        string json = File.ReadAllText(path);
        ConversionSettings? settings = FromJson(json);
        return settings;
    }
    
    public static ConversionSettings FromJson(string json) {
        ConversionSettings? settings = JsonSerializer.Deserialize<ConversionSettings>(json);
        if(settings is null) throw new NullReferenceException("Could not deserialize settings file");
        return settings;
    }
    
    public static string ToJson(ConversionSettings settings) => JsonSerializer.Serialize(settings);
    
    public void Save(string path) => File.WriteAllText(path, ToJson());
    
    public string ToJson() => JsonSerializer.Serialize(this);
    
}