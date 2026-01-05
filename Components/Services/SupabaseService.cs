using System.Net.Http.Json;
using DMsite.Models;

namespace DMsite.Services;

public class SupabaseService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public SupabaseService(IHttpClientFactory factory, AuthService auth)
    {
        _http = factory.CreateClient("Supabase");
        _auth = auth;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string uri, object? content = null)
    {
        var request = new HttpRequestMessage(method, uri);

        if (content != null)
        {
            request.Content = JsonContent.Create(content);
        }

        if (_auth.Session?.AccessToken != null)
        {
            request.Headers.Add("Authorization", $"Bearer {_auth.Session.AccessToken}");
        }

        return request;
    }

    public async Task<List<Service>> GetServicesAsync()
    {
        return await _http.GetFromJsonAsync<List<Service>>("services?order=id")
               ?? new List<Service>();
    }

    public async Task CreateServiceAsync(Service s)
    {
        var request = CreateRequest(HttpMethod.Post, "services", s);
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateServiceAsync(Service s)
    {
        var request = CreateRequest(HttpMethod.Patch, $"services?id=eq.{s.Id}", s);
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteServiceAsync(long id)
    {
        var request = CreateRequest(HttpMethod.Delete, $"services?id=eq.{id}");
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<Announcement>> GetAnnouncementsAsync()
    {
        return await _http.GetFromJsonAsync<List<Announcement>>("announcements?order=CreatedAt.desc")
               ?? new List<Announcement>();
    }

    public async Task CreateAnnouncementAsync(Announcement a)
    {
        var request = CreateRequest(HttpMethod.Post, "announcements", a);
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAnnouncementAsync(long id)
    {
        var request = CreateRequest(HttpMethod.Delete, $"announcements?id=eq.{id}");
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}