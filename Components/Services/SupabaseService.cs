using System.Net.Http.Json;
using DMsite.Models;

namespace DMsite.Services;

public class SupabaseService
{
    private readonly HttpClient _http;

    public SupabaseService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("Supabase");
    }
    public async Task CreateServiceAsync(Service s)
    {
        var response = await _http.PostAsJsonAsync("services", s);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateServiceAsync(Service s)
    {
        var response = await _http.PatchAsJsonAsync($"services?id=eq.{s.Id}", s);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteServiceAsync(long id)
    {
        var response = await _http.DeleteAsync($"services?id=eq.{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<Service>> GetServicesAsync()
    {
        return await _http.GetFromJsonAsync<List<Service>>("services")
               ?? new List<Service>();
    }
}
