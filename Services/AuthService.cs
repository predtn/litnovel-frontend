using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace litnovel_frontend.Services;

public class AuthUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Avatar { get; set; }
    public string Role { get; set; } = "User";
    public string AccessToken { get; set; } = "";
    public string? RefreshToken { get; set; }
}

public class LoginRequest  { public string Identifier { get; set; } = ""; public string Password { get; set; } = ""; }
public class LoginResponse { public string AccessToken { get; set; } = ""; public string? RefreshToken { get; set; } public int ExpiresIn { get; set; } public AuthUser? User { get; set; } }
public class RegisterRequest { public string Username { get; set; } = ""; public string Email { get; set; } = ""; public string Password { get; set; } = ""; }

public interface IAuthService
{
    AuthUser? GetCurrentUser(HttpContext ctx);
    string? GetToken(HttpContext ctx);
    Task<(bool Success, string? Error)> LoginAsync(HttpContext ctx, string identifier, string password);
    Task<(bool Success, string? Error)> RegisterAsync(HttpContext ctx, string username, string email, string password);
    void Logout(HttpContext ctx);
    bool IsAuthenticated(HttpContext ctx);
    bool IsInRole(HttpContext ctx, string role);
}

public class AuthService : IAuthService
{
    private const string TokenCookie  = "litnovel_token";
    private const string RefreshCookie = "litnovel_refresh";
    private const string UserCookie   = "litnovel_user";
    private readonly IApiService _api;

    public AuthService(IApiService api) { _api = api; }

    public string? GetToken(HttpContext ctx) => ctx.Request.Cookies[TokenCookie];

    public AuthUser? GetCurrentUser(HttpContext ctx)
    {
        var token = GetToken(ctx);
        if (string.IsNullOrEmpty(token)) return null;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var id   = int.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "nameid")?.Value, out var i) ? i : 0;
            var name = jwt.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == "name")?.Value ?? "";
            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";
            var role = jwt.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role)?.Value ?? "User";
            var avatar = jwt.Claims.FirstOrDefault(c => c.Type == "avatar")?.Value;
            return new AuthUser { Id = id, Username = name, Email = email, Role = role, Avatar = avatar, AccessToken = token };
        }
        catch { return null; }
    }

    public bool IsAuthenticated(HttpContext ctx) => !string.IsNullOrEmpty(GetToken(ctx));

    public bool IsInRole(HttpContext ctx, string role)
    {
        var user = GetCurrentUser(ctx);
        if (user == null) return false;
        return role switch
        {
            "Admin" => user.Role == "Admin",
            "Staff" => user.Role is "Staff" or "Admin",
            "User"  => user.Role is "User" or "Staff" or "Admin",
            _ => false
        };
    }

    public async Task<(bool Success, string? Error)> LoginAsync(HttpContext ctx, string identifier, string password)
    {
        var result = await _api.PostAsync<LoginResponse>("/api/auth/login", new LoginRequest { Identifier = identifier, Password = password });
        if (result?.Success == true && result.Data != null)
        {
            SetTokenCookies(ctx, result.Data.AccessToken, result.Data.RefreshToken, result.Data.ExpiresIn);
            return (true, null);
        }
        return (false, result?.Message ?? "Thông tin đăng nhập không hợp lệ.");
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(HttpContext ctx, string username, string email, string password)
    {
        var result = await _api.PostAsync<object>("/api/auth/register", new RegisterRequest { Username = username, Email = email, Password = password });
        if (result?.Success == true) return (true, null);
        return (false, result?.Message ?? "Đăng ký thất bại.");
    }

    public void Logout(HttpContext ctx)
    {
        ctx.Response.Cookies.Delete(TokenCookie);
        ctx.Response.Cookies.Delete(RefreshCookie);
    }

    private static void SetTokenCookies(HttpContext ctx, string accessToken, string? refreshToken, int expiresIn)
    {
        var opts = new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax };
        ctx.Response.Cookies.Append(TokenCookie, accessToken, new CookieOptions
        {
            HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddSeconds(expiresIn > 0 ? expiresIn : 900)
        });
        if (!string.IsNullOrEmpty(refreshToken))
            ctx.Response.Cookies.Append(RefreshCookie, refreshToken, new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
    }
}
