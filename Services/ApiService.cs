using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace litnovel_frontend.Services;

public interface IApiService
{
    Task<ApiResponse<T>?> GetAsync<T>(string endpoint, string? token = null);
    Task<ApiResponse<T>?> PostAsync<T>(string endpoint, object? body, string? token = null);
    Task<ApiResponse<T>?> PutAsync<T>(string endpoint, object? body, string? token = null);
    Task<ApiResponse<T>?> DeleteAsync<T>(string endpoint, string? token = null);
}

public class ApiService : IApiService
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ApiService(HttpClient http, ILogger<ApiService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ApiResponse<T>?> GetAsync<T>(string endpoint, string? token = null)
        => await SendAsync<T>(HttpMethod.Get, endpoint, null, token);

    public async Task<ApiResponse<T>?> PostAsync<T>(string endpoint, object? body, string? token = null)
        => await SendAsync<T>(HttpMethod.Post, endpoint, body, token);

    public async Task<ApiResponse<T>?> PutAsync<T>(string endpoint, object? body, string? token = null)
        => await SendAsync<T>(HttpMethod.Put, endpoint, body, token);

    public async Task<ApiResponse<T>?> DeleteAsync<T>(string endpoint, string? token = null)
        => await SendAsync<T>(HttpMethod.Delete, endpoint, null, token);

    private async Task<ApiResponse<T>?> SendAsync<T>(HttpMethod method, string endpoint, object? body, string? token)
    {
        try
        {
            var request = new HttpRequestMessage(method, endpoint);
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _http.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return new ApiResponse<T> { Success = response.IsSuccessStatusCode };

            return JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API call failed: {Method} {Endpoint}", method, endpoint);
            return new ApiResponse<T> { Success = false, Message = "Không thể kết nối đến máy chủ." };
        }
    }
}
