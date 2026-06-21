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
            {
                return new ApiResponse<T>
                {
                    Success = response.IsSuccessStatusCode,
                    Message = response.IsSuccessStatusCode ? null : BuildHttpErrorMessage(response)
                };
            }

            try
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                if (result == null)
                {
                    return new ApiResponse<T>
                    {
                        Success = false,
                        Message = BuildHttpErrorMessage(response)
                    };
                }

                if (!response.IsSuccessStatusCode)
                {
                    result.Success = false;
                    result.Message = FirstNonEmpty(result.Message, ExtractErrorMessage(content), BuildHttpErrorMessage(response));
                }

                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Unable to parse API response: {Method} {Endpoint}", method, endpoint);
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = FirstNonEmpty(ExtractErrorMessage(content), BuildHttpErrorMessage(response))
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API call failed: {Method} {Endpoint}", method, endpoint);
            return new ApiResponse<T> { Success = false, Message = "Unable to connect to the server." };
        }
    }

    private static string BuildHttpErrorMessage(HttpResponseMessage response)
        => $"Request failed with HTTP {(int)response.StatusCode} {response.ReasonPhrase}.";

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static string? ExtractErrorMessage(string content)
    {
        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (TryGetString(root, "message", out var message)) return message;

            if (root.TryGetProperty("errors", out var errors))
            {
                var messages = new List<string>();
                if (errors.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in errors.EnumerateObject())
                    {
                        if (property.Value.ValueKind == JsonValueKind.Array)
                        {
                            messages.AddRange(property.Value.EnumerateArray()
                                .Where(item => item.ValueKind == JsonValueKind.String)
                                .Select(item => item.GetString())
                                .Where(value => !string.IsNullOrWhiteSpace(value))!);
                        }
                        else if (property.Value.ValueKind == JsonValueKind.String)
                        {
                            messages.Add(property.Value.GetString()!);
                        }
                    }
                }
                else if (errors.ValueKind == JsonValueKind.Array)
                {
                    messages.AddRange(errors.EnumerateArray()
                        .Where(item => item.ValueKind == JsonValueKind.String)
                        .Select(item => item.GetString())
                        .Where(value => !string.IsNullOrWhiteSpace(value))!);
                }

                if (messages.Count > 0) return string.Join(" ", messages.Take(3));
            }

            if (TryGetString(root, "detail", out var detail)) return detail;
            if (TryGetString(root, "title", out var title)) return title;
        }
        catch (JsonException)
        {
            return content.Length <= 300 ? content : content[..300];
        }

        return null;
    }

    private static bool TryGetString(JsonElement root, string propertyName, out string? value)
    {
        value = null;
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString();
        return !string.IsNullOrWhiteSpace(value);
    }
}
