using System.Net.Http.Json;
using DMsite.Models;

namespace DMsite.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public SupabaseAuthResponse? Session { get; private set; }
    public bool IsLoggedIn => Session != null;

    public AuthService(IHttpClientFactory factory, IConfiguration config)
    {
        _http = factory.CreateClient();
        _config = config;
    }

    // Login
    public async Task<bool> Login(string email, string password)
    {
        try
        {
            var url = $"{_config["Supabase:Url"]}/auth/v1/token?grant_type=password";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("apikey", _config["Supabase:AnonKey"]);
            request.Headers.Add("Authorization", $"Bearer {_config["Supabase:AnonKey"]}");
            request.Content = JsonContent.Create(new { email, password });

            var response = await _http.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Supabase login failed: {content}");
                return false;
            }

            var authResponse = await response.Content.ReadFromJsonAsync<SupabaseAuthResponse>();

            if (authResponse?.AccessToken == null)
            {
                Console.WriteLine($"Supabase login returned no access token: {content}");
                return false;
            }

            Session = authResponse;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in Login: {ex.Message}");
            return false;
        }
    }


    // Register + create customer profile
    public async Task<bool> Register(string email, string password, string fullName)
    {
        try
        {
            var url = $"{_config["Supabase:Url"]}/auth/v1/signup";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("apikey", _config["Supabase:AnonKey"]);
            request.Content = JsonContent.Create(new { email, password });

            var response = await _http.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Supabase signup failed: {content}");
                return false;
            }

            var authResponse = await response.Content.ReadFromJsonAsync<SupabaseAuthResponse>();

            if (authResponse?.User == null)
            {
                Console.WriteLine($"Supabase signup returned no user. Response: {content}");
                return false;
            }

            // Store session in memory (optional)
            Session = authResponse;

            // Insert profile with default role = customer
            var profileUrl = $"{_config["Supabase:Url"]}/rest/v1/profiles";
            var profileReq = new HttpRequestMessage(HttpMethod.Post, profileUrl);
            profileReq.Headers.Add("apikey", _config["Supabase:AnonKey"]);
            profileReq.Headers.Add("Authorization", $"Bearer {_config["Supabase:AnonKey"]}");
            profileReq.Content = JsonContent.Create(new
            {
                id = authResponse.User.Id,
                full_name = fullName,
                role = "customer"
            });

            var profileResp = await _http.SendAsync(profileReq);
            if (!profileResp.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to create profile: {await profileResp.Content.ReadAsStringAsync()}");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in Register: {ex.Message}");
            return false;
        }
    }

    public async Task<string?> GetUserRole()
    {
        if (!IsLoggedIn) return null;

        var userId = Session!.User.Id;
        var url = $"{_config["Supabase:Url"]}/rest/v1/profiles?id=eq.{userId}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("apikey", _config["Supabase:AnonKey"]);
        request.Headers.Add("Authorization", $"Bearer {_config["Supabase:AnonKey"]}");

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var profiles = await response.Content.ReadFromJsonAsync<List<Profile>>();
        return profiles?.FirstOrDefault()?.Role;
    }

    public class Profile
    {
        public string Id { get; set; } = "";
        public string Full_Name { get; set; } = "";
        public string Role { get; set; } = "";
    }

    public void Logout() => Session = null;
}
