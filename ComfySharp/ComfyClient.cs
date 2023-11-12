using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ComfySharp;

public class ComfyClient {
    private HttpClient client;
    private List<Node> nodes;
    
    public string BaseUrl { get; set; }
    
    public ComfyClient(string baseUrl) {
        BaseUrl = baseUrl;
        client = new HttpClient {
            BaseAddress = new Uri(baseUrl),
            DefaultRequestHeaders = { { "User-Agent", "ComfySharp" } }
        };
        nodes= new List<Node>();
    }

    public async Task<string[]?> GetEmbeddings() {
        string[]? embeddings = null;

        var req = await client.GetAsync("/embeddings");    

        if (req is { IsSuccessStatusCode: true, Content: not null }) 
            embeddings = await req.Content.ReadFromJsonAsync<string[]>();
        
        return embeddings;
    }

    //???
    public async Task<ImageInfo?> UploadImage() {
        var req = await client.PostAsync("/upload/image", null );
        
        if (req is { IsSuccessStatusCode: true, Content: not null }) 
            return await req.Content.ReadFromJsonAsync<ImageInfo>();
        return null;
    }

    //public async Task<>GetHistory

    public async Task<List<Node>?> GetObjectInfo() {
        var req = await client.GetAsync("/object_info");

        if (req is { IsSuccessStatusCode: true, Content: not null }) {
            var doc = await req.Content.ReadFromJsonAsync<JsonDocument>();

            foreach (var node in doc.RootElement.EnumerateObject()) {
                Node n;
                
                
            }
        }
            
        return null;
    }
    
    public async Task<byte[]?> GetImage(string filename) {
        var req = await client.GetAsync($"/image?{filename}");
        
        if (req is { IsSuccessStatusCode: true, Content: not null }) 
            return await req.Content.ReadFromJsonAsync<byte[]>();
        return null;
    }
}

[DataContract, JsonSerializable(typeof(Node))]
public class Node {
    [DataMember(Name = "name")]
    public string Name { get; set; }
    [DataMember(Name = "input")]
    public Input Input { get; set; }
    [DataMember(Name = "output")]
    public List<PrimitiveType> Outputs { get; set; }
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
}

[DataContract]
public struct Input {
    [DataMember]
    public List<InputField> Required { get; set; }
    [DataMember]
    public List<InputField> Optional { get; set; }
    [DataMember]
    public List<InputField> Hidden { get; set; }

    
    
}

public struct InputField {
    [DataMember(Name = "name")]
    public string Name { get; set; }
    
    public PrimitiveType Type { get; set; }
}

public enum PrimitiveType {
    ANY,
    CLIP,
    CLIP_VISION,
    CLIP_VISION_OUTPUT,
    CONDITIONING,
    CONTROL_NET,
    EXTRA_PNGINFO,
    FLOAT,
    GLIGEN,
    IMAGE,
    INT,
    LATENT,
    MASK,
    MODEL,
    PROMPT,
    SAMPLER,
    SIGMAS,
    STRING,
    STYLE_MODEL,
    UNIQUE_ID,
    UPSCALE_MODEL,
    VAE,
}

[DataContract, JsonSerializable(typeof(ImageInfo))]
public class ImageInfo {
    [DataMember(Name = "name")]
    public string Name { get; set; }
    [DataMember(Name = "subfolder")]
    public string Subfolder { get; set; }
    [DataMember(Name = "type")]
    public DirType Type { get; set; }
}

[DataContract]
public enum DirType {
    [DataMember(Name = "input")] Input,
    [DataMember(Name = "temp")] Temp,
    [DataMember(Name = "output")] Output
}