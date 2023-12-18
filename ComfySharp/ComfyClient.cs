using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ComfySharp.Types;

namespace ComfySharp;

public class ComfyClient {
    private HttpClient client;
    private List<ExpandoObject> nodes;
    
    public string BaseUrl { get; set; }
    
    public ComfyClient(string baseUrl) {
        BaseUrl = baseUrl;
        client = new HttpClient {
            BaseAddress = new Uri(baseUrl),
            DefaultRequestHeaders = { { "User-Agent", "ComfySharp" } }
        };
        nodes= new();
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
            ObjectInfoParser.Parse(doc, out nodes);
        }

        return null;
        //throw new NotImplementedException();
    }
    
    public async Task<byte[]?> GetImage(string filename) {
        var req = await client.GetAsync($"/image?{filename}");
        
        if (req is { IsSuccessStatusCode: true, Content: not null }) 
            return await req.Content.ReadFromJsonAsync<byte[]>();
        return null;
    }
}