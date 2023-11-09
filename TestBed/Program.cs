using System.Threading.Channels;
using ComfySharp;

Console.WriteLine("Setting Up testing for default comfyUI server running on localhost:8188");
var client = new ComfyClient("http://localhost:8188");

var info = await client.GetObjectInfo();

Console.WriteLine("Testing GetEmbeddings");
var embeddings = await client.GetEmbeddings();

for (int i = 0; i < embeddings.Length; i++) {
    Console.WriteLine($"Embedding {i}: {embeddings[i]}");
}

Console.WriteLine("Testing UploadImage");
Console.ReadLine();