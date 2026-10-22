using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using shared.Model;

namespace WebApp.Data;

public class ApiService
{
    private readonly HttpClient http;
    private readonly IConfiguration configuration;
    private readonly string baseAPI;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiService(HttpClient http, IConfiguration configuration)
    {
        this.http = http;
        this.configuration = configuration;
        baseAPI = this.configuration["base_api"]
            ?? throw new InvalidOperationException("Configuration value 'base_api' is missing.");
    }

    public async Task<Post[]> GetPosts()
    {
        string url = $"{baseAPI}posts/";
        return await http.GetFromJsonAsync<Post[]>(url) ?? [];
    }

    public async Task<Post?> GetPost(int id)
    {
        string url = $"{baseAPI}posts/{id}/";
        return await http.GetFromJsonAsync<Post>(url);
    }

    public async Task<Post> CreatePost(string title, string content, string authorName, bool isUrl)
    {
        string url = $"{baseAPI}posts/";
        HttpResponseMessage msg = await http.PostAsJsonAsync(url, new { title, content, authorName, isUrl });

        msg.EnsureSuccessStatusCode();
        return await ReadResponse<Post>(msg);
    }

    public async Task<Comment> CreateComment(string content, string authorName, int postId)
    {
        string url = $"{baseAPI}posts/{postId}/comments";
        HttpResponseMessage msg = await http.PostAsJsonAsync(url, new { content, authorName });

        msg.EnsureSuccessStatusCode();
        return await ReadResponse<Comment>(msg);
    }

    public async Task<Post> UpvotePost(int id)
    {
        string url = $"{baseAPI}posts/{id}/upvote/";
        HttpResponseMessage msg = await http.PutAsync(url, content: null);

        msg.EnsureSuccessStatusCode();
        return await ReadResponse<Post>(msg);
    }

    public async Task<Post> DownvotePost(int id)
    {
        string url = $"{baseAPI}posts/{id}/downvote/";
        HttpResponseMessage msg = await http.PutAsync(url, content: null);

        msg.EnsureSuccessStatusCode();
        return await ReadResponse<Post>(msg);
    }

    public async Task<Comment> UpvoteComment(int postId, int commentId)
    {
        string url = $"{baseAPI}posts/{postId}/comments/{commentId}/upvote";
        HttpResponseMessage msg = await http.PutAsync(url, content: null);

        msg.EnsureSuccessStatusCode();
        return await ReadResponse<Comment>(msg);
    }

    public async Task<Comment> DownvoteComment(int postId, int commentId)
    {
        string url = $"{baseAPI}posts/{postId}/comments/{commentId}/downvote";
        HttpResponseMessage msg = await http.PutAsync(url, content: null);

        msg.EnsureSuccessStatusCode();
        return await ReadResponse<Comment>(msg);
    }

    private static async Task<T> ReadResponse<T>(HttpResponseMessage msg)
    {
        string json = await msg.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
            ?? throw new InvalidDataException($"The API returned an empty {typeof(T).Name} response.");
    }
}
