using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

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

public class LoginRequest { public string Identifier { get; set; } = ""; public string Password { get; set; } = ""; }
public class LoginResponse { public string AccessToken { get; set; } = ""; public string? RefreshToken { get; set; } public int ExpiresIn { get; set; } public AuthUser? User { get; set; } }
public class RegisterRequest { public string Username { get; set; } = ""; public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
public class ForgotPasswordRequest { public string Email { get; set; } = ""; }
public class ResetPasswordRequest { public string Token { get; set; } = ""; public string NewPassword { get; set; } = ""; public string ConfirmPassword { get; set; } = ""; }
public class ChangePasswordRequest { public string CurrentPassword { get; set; } = ""; public string NewPassword { get; set; } = ""; public string ConfirmPassword { get; set; } = ""; }
public class UpdateProfileRequest { public string? Avatar { get; set; } public string? Bio { get; set; } }
public class LogoutRequest { public string RefreshToken { get; set; } = ""; }

public interface IAuthService
{
    AuthUser? GetCurrentUser(HttpContext ctx);
    string? GetToken(HttpContext ctx);
    string? GetRefreshToken(HttpContext ctx);
    Task<(bool Success, string? Error)> LoginAsync(HttpContext ctx, string identifier, string password);
    Task<(bool Success, string? Error)> RegisterAsync(HttpContext ctx, string username, string email, string password);
    Task<(bool Success, string? Error)> ForgotPasswordAsync(string email);
    Task<(bool Success, string? Error)> ResetPasswordAsync(string token, string newPassword, string confirmPassword);
    Task LogoutAsync(HttpContext ctx);
    bool IsAuthenticated(HttpContext ctx);
    bool IsInRole(HttpContext ctx, string role);
}

public class AuthService : IAuthService
{
    private const string TokenCookie = "litnovel_token";
    private const string RefreshCookie = "litnovel_refresh";
    private readonly IApiService _api;

    public AuthService(IApiService api)
    {
        _api = api;
    }

    public string? GetToken(HttpContext ctx) => ctx.Request.Cookies[TokenCookie];
    public string? GetRefreshToken(HttpContext ctx) => ctx.Request.Cookies[RefreshCookie];

    public AuthUser? GetCurrentUser(HttpContext ctx)
    {
        var token = GetToken(ctx);
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var id = int.TryParse(jwt.Claims.FirstOrDefault(c => c.Type is "sub" or "nameid" || c.Type == ClaimTypes.NameIdentifier)?.Value, out var parsedId) ? parsedId : 0;
            var name = jwt.Claims.FirstOrDefault(c => c.Type is "unique_name" or "name" || c.Type == ClaimTypes.Name)?.Value ?? "";
            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email)?.Value ?? "";
            var role = jwt.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role)?.Value ?? "User";
            var avatar = jwt.Claims.FirstOrDefault(c => c.Type == "avatar")?.Value;

            return new AuthUser { Id = id, Username = name, Email = email, Role = role, Avatar = avatar, AccessToken = token, RefreshToken = GetRefreshToken(ctx) };
        }
        catch
        {
            return null;
        }
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
            "User" => user.Role is "User" or "Staff" or "Admin",
            _ => false
        };
    }

    public async Task<(bool Success, string? Error)> LoginAsync(HttpContext ctx, string identifier, string password)
    {
        var result = await _api.PostAsync<LoginResponse>("/api/auth/login", new LoginRequest { Identifier = identifier, Password = password });
        if (result?.Success == true && result.Data != null)
        {
            SetTokenCookies(ctx, result.Data.AccessToken, result.Data.RefreshToken, result.Data.ExpiresIn);
            await SignInCookieAsync(ctx, result.Data);
            return (true, null);
        }

        return (false, result?.Message ?? "Invalid login information.");
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(HttpContext ctx, string username, string email, string password)
    {
        var result = await _api.PostAsync<object>("/api/auth/register", new RegisterRequest { Username = username, Email = email, Password = password });
        return result?.Success == true
            ? (true, null)
            : (false, result?.Message ?? "Registration failed.");
    }

    public async Task<(bool Success, string? Error)> ForgotPasswordAsync(string email)
    {
        var result = await _api.PostAsync<object>("/api/auth/forgot-password", new ForgotPasswordRequest { Email = email });
        return result?.Success == true
            ? (true, null)
            : (false, result?.Message ?? "Could not send password reset request.");
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(string token, string newPassword, string confirmPassword)
    {
        var result = await _api.PostAsync<object>("/api/auth/reset-password", new ResetPasswordRequest
        {
            Token = token,
            NewPassword = newPassword,
            ConfirmPassword = confirmPassword
        });

        return result?.Success == true
            ? (true, null)
            : (false, result?.Message ?? "Invalid or expired reset token.");
    }

    public async Task LogoutAsync(HttpContext ctx)
    {
        var refreshToken = GetRefreshToken(ctx);
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _api.PostAsync<object>("/api/auth/logout", new LogoutRequest { RefreshToken = refreshToken });
        }

        ctx.Response.Cookies.Delete(TokenCookie);
        ctx.Response.Cookies.Delete(RefreshCookie);
        await ctx.SignOutAsync("Cookies");
    }

    private static void SetTokenCookies(HttpContext ctx, string accessToken, string? refreshToken, int expiresIn)
    {
        ctx.Response.Cookies.Append(TokenCookie, accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddSeconds(expiresIn > 0 ? expiresIn : 900)
        });

        if (!string.IsNullOrEmpty(refreshToken))
        {
            ctx.Response.Cookies.Append(RefreshCookie, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
        }
    }

    private static async Task SignInCookieAsync(HttpContext ctx, LoginResponse response)
    {
        var user = response.User;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user?.Id.ToString() ?? ""),
            new(ClaimTypes.Name, user?.Username ?? ""),
            new(ClaimTypes.Email, user?.Email ?? ""),
            new(ClaimTypes.Role, user?.Role ?? "User")
        };

        if (!string.IsNullOrWhiteSpace(user?.Avatar))
        {
            claims.Add(new Claim("avatar", user.Avatar));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies"));
        await ctx.SignInAsync("Cookies", principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn > 0 ? response.ExpiresIn : 900)
        });
    }
}
