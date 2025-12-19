using System.Text.Json.Serialization;

namespace DMsite.Models;

public class SupabaseAuthResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("user")]
    public SupabaseUser User { get; set; } = new();
}

public class SupabaseUser
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
}
