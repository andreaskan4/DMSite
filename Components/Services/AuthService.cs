using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using DMsite.Models;

namespace DMsite.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly IJSRuntime _js;

    public SupabaseAuthResponse? Session { get; private set; }
    public bool IsLoggedIn => Session != null;
    private string? _cachedRole;

    public AuthService(IHttpClientFactory factory, IConfiguration config, IJSRuntime js)
    {
        _http = factory.CreateClient("Supabase");
        _config = config;
        _js = js;
    }

    public async Task InitializeAsync()
    {
        if (Session != null) return;
        try
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", "supabase.session");
            if (!string.IsNullOrEmpty(json))
                Session = JsonSerializer.Deserialize<SupabaseAuthResponse>(json);
        }
        catch { }
    }

    public async Task<(bool IsSuccess, string? Error)> Login(string email, string password)
    {
        try
        {
            _cachedRole = null;
            var url = $"{_config["Supabase:Url"]}/auth/v1/token?grant_type=password";
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            // Προσθήκη Anon Key για να μας επιτρέψει το Login
            request.Headers.Add("Authorization", $"Bearer {_config["Supabase:AnonKey"]}");
            request.Content = JsonContent.Create(new { email, password });

            var response = await _http.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return (false, ExtractError(content) ?? "Αποτυχία σύνδεσης.");

            var authResponse = await response.Content.ReadFromJsonAsync<SupabaseAuthResponse>();
            if (authResponse?.AccessToken == null) return (false, "Δεν ελήφθη token.");

            Session = authResponse;
            await SaveSession();
            return (true, null);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool IsSuccess, string? Error)> Register(string email, string password, string fullName)
    {
        try
        {
            _cachedRole = null;
            var url = $"{_config["Supabase:Url"]}/auth/v1/signup";
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Add("Authorization", $"Bearer {_config["Supabase:AnonKey"]}");
            var payload = new { email, password, data = new { full_name = fullName } };
            request.Content = JsonContent.Create(payload);

            var response = await _http.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return (false, ExtractError(content) ?? "Αποτυχία εγγραφής.");

            var authResponse = await response.Content.ReadFromJsonAsync<SupabaseAuthResponse>();
            if (authResponse?.User != null)
            {
                if (authResponse.AccessToken != null) { Session = authResponse; await SaveSession(); }
                return (true, null);
            }
            return (false, "Σφάλμα εγγραφής.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task Logout()
    {
        Session = null;
        _cachedRole = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", "supabase.session");
    }

    public async Task<string?> GetUserRole()
    {
        if (_cachedRole != null) return _cachedRole;
        if (!IsLoggedIn) await InitializeAsync();
        if (!IsLoggedIn) return null;

        try
        {
            var userId = Session!.User.Id;
            var url = $"profiles?id=eq.{userId}&select=role";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            // Εδώ χρησιμοποιούμε το Token του Χρήστη
            request.Headers.Add("Authorization", $"Bearer {Session.AccessToken}");

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var profiles = await response.Content.ReadFromJsonAsync<List<Profile>>();
            var role = profiles?.FirstOrDefault()?.Role;
            if (!string.IsNullOrEmpty(role)) _cachedRole = role;
            return role;
        }
        catch { return null; }
    }

    private async Task SaveSession()
    {
        var json = JsonSerializer.Serialize(Session);
        await _js.InvokeVoidAsync("localStorage.setItem", "supabase.session", json);
    }

    private string? ExtractError(string jsonContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            if (doc.RootElement.TryGetProperty("msg", out var msg)) return msg.GetString();
            if (doc.RootElement.TryGetProperty("error_description", out var desc)) return desc.GetString();
        }
        catch { }
        return jsonContent;
    }

    public class Profile { public string Role { get; set; } = ""; }
}