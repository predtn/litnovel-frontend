using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace litnovel_frontend.Services;

public class AuthUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Avatar { get; set; }
    public string Role { get; set; } = "User";
    public string Status { get; set; } = "Online";
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
    Task<(bool Success, string? Error, AuthUser? User)> LoginAsync(HttpContext ctx, string identifier, string password);
    Task<(bool Success, string? Error)> RegisterAsync(HttpContext ctx, string username, string email, string password);
    Task LogoutAsync(HttpContext ctx);
    Task<SessionValidationResult> ValidateSessionAsync(HttpContext ctx);
    bool IsAuthenticated(HttpContext ctx);
    bool IsInRole(HttpContext ctx, string role);
}

public enum SessionValidationState
{
    Valid,
    LoggedOut,
    Updated
}

public record SessionValidationResult(SessionValidationState State, string? Message = null, string? RedirectPath = null);

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
        if (string.IsNullOrEmpty(token) && ctx.User.Identity?.IsAuthenticated != true) return null;
        try
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                claims.AddRange(jwt.Claims);
            }

            claims.AddRange(ctx.User.Claims);
            var idClaim = claims.FirstOrDefault(c => c.Type is "sub" or "nameid" || c.Type == ClaimTypes.NameIdentifier)?.Value;
            var id = int.TryParse(idClaim, out var i) ? i : 0;
            var name = claims.FirstOrDefault(c => c.Type is "unique_name" or "name" || c.Type == ClaimTypes.Name)?.Value ?? "";
            var email = claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email)?.Value ?? "";
            var role = GetRoleFromClaims(claims) ?? "User";
            var status = claims.FirstOrDefault(c => c.Type.Equals("status", StringComparison.OrdinalIgnoreCase))?.Value ?? "Online";
            var avatar = claims.FirstOrDefault(c => c.Type == "avatar")?.Value;
            return new AuthUser { Id = id, Username = name, Email = email, Role = role, Status = status, Avatar = avatar, AccessToken = token ?? "" };
        }
        catch { return null; }
    }

    public bool IsAuthenticated(HttpContext ctx) =>
        !string.IsNullOrEmpty(GetToken(ctx)) || ctx.User.Identity?.IsAuthenticated == true;

    public bool IsInRole(HttpContext ctx, string role)
    {
        var user = GetCurrentUser(ctx);
        if (user == null) return false;
        if (role.Equals("User", StringComparison.OrdinalIgnoreCase)) return IsAuthenticated(ctx);

        return role switch
        {
            "Admin" => HasRole(user.Role, "Admin"),
            "Staff" => HasRole(user.Role, "Staff") || HasRole(user.Role, "Admin"),
            "User"  => true,
            _ => false
        };
    }

    public async Task<(bool Success, string? Error, AuthUser? User)> LoginAsync(HttpContext ctx, string identifier, string password)
    {
        var result = await _api.PostAsync<LoginResponse>("/api/auth/login", new LoginRequest { Identifier = identifier, Password = password });
        if (result?.Success == true && result.Data != null)
        {
            var loginUser = result.Data.User ?? ParseUserFromToken(result.Data.AccessToken);
            if (loginUser.Status.Equals("Banned", StringComparison.OrdinalIgnoreCase))
            {
                await LogoutAsync(ctx);
                return (false, "Tài khoản của bạn đã bị cấm.", null);
            }

            SetTokenCookies(ctx, result.Data.AccessToken, result.Data.RefreshToken, result.Data.ExpiresIn);
            await SignInCookieAsync(ctx, result.Data);
            return (true, null, loginUser);
        }
        return (false, result?.Message ?? "Thông tin đăng nhập không hợp lệ.", null);
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(HttpContext ctx, string username, string email, string password)
    {
        var result = await _api.PostAsync<object>("/api/auth/register", new RegisterRequest { Username = username, Email = email, Password = password });
        if (result?.Success == true) return (true, null);
        return (false, result?.Message ?? "Đăng ký thất bại.");
    }

    public async Task LogoutAsync(HttpContext ctx)
    {
        ctx.Response.Cookies.Delete(TokenCookie);
        ctx.Response.Cookies.Delete(RefreshCookie);
        ctx.Response.Cookies.Delete(UserCookie);
        await ctx.SignOutAsync("Cookies");
    }

    public async Task<SessionValidationResult> ValidateSessionAsync(HttpContext ctx)
    {
        var token = GetToken(ctx);
        if (string.IsNullOrWhiteSpace(token)) return new(SessionValidationState.Valid);

        var current = GetCurrentUser(ctx);
        var result = await _api.GetAsync<UserDetailDto>("/api/users/me", token);
        if (result?.Success != true || result.Data == null)
        {
            await LogoutAsync(ctx);
            return new(SessionValidationState.LoggedOut, "Phiên đăng nhập đã hết hạn hoặc tài khoản không còn được phép truy cập.", "/Auth/Login");
        }

        var fresh = result.Data;
        if (fresh.Status.Equals("Banned", StringComparison.OrdinalIgnoreCase))
        {
            await LogoutAsync(ctx);
            return new(SessionValidationState.LoggedOut, "Tài khoản của bạn đã bị cấm. Vui lòng liên hệ quản trị viên nếu cần hỗ trợ.", "/Auth/Login");
        }

        if (current == null ||
            !fresh.Role.Equals(current.Role, StringComparison.OrdinalIgnoreCase) ||
            !fresh.Status.Equals(current.Status, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(fresh.Username, current.Username, StringComparison.Ordinal) ||
            !string.Equals(fresh.Email, current.Email, StringComparison.Ordinal) ||
            !string.Equals(fresh.Avatar, current.Avatar, StringComparison.Ordinal))
        {
            var updatedUser = new AuthUser
            {
                Id = fresh.Id,
                Username = fresh.Username,
                Email = fresh.Email,
                Avatar = fresh.Avatar,
                Role = fresh.Role,
                Status = fresh.Status,
                AccessToken = token,
                RefreshToken = ctx.Request.Cookies[RefreshCookie]
            };

            await SignInCookieAsync(ctx, new LoginResponse
            {
                AccessToken = token,
                RefreshToken = updatedUser.RefreshToken,
                ExpiresIn = 900,
                User = updatedUser
            });

            var message = fresh.Role.Equals("Staff", StringComparison.OrdinalIgnoreCase) &&
                current?.Role.Equals("Staff", StringComparison.OrdinalIgnoreCase) != true
                    ? "Tài khoản của bạn đã được cấp quyền Staff. Bạn có thể truy cập Staff Dashboard."
                    : null;

            return new(SessionValidationState.Updated, message);
        }

        return new(SessionValidationState.Valid);
    }

    private static void SetTokenCookies(HttpContext ctx, string accessToken, string? refreshToken, int expiresIn)
    {
        ctx.Response.Cookies.Append(TokenCookie, accessToken, new CookieOptions
        {
            HttpOnly = true, Secure = ctx.Request.IsHttps, SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddSeconds(expiresIn > 0 ? expiresIn : 900)
        });
        if (!string.IsNullOrEmpty(refreshToken))
            ctx.Response.Cookies.Append(RefreshCookie, refreshToken, new CookieOptions
            {
                HttpOnly = true, Secure = ctx.Request.IsHttps, SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
    }

    private static async Task SignInCookieAsync(HttpContext ctx, LoginResponse login)
    {
        var user = login.User ?? ParseUserFromToken(login.AccessToken);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role),
            new("status", string.IsNullOrWhiteSpace(user.Status) ? "Online" : user.Status)
        };

        if (!string.IsNullOrWhiteSpace(user.Avatar))
        {
            claims.Add(new Claim("avatar", user.Avatar));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync("Cookies", new ClaimsPrincipal(identity), new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(login.ExpiresIn > 0 ? login.ExpiresIn : 900)
        });
    }

    private static AuthUser ParseUserFromToken(string token)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var id = int.TryParse(jwt.Claims.FirstOrDefault(c => c.Type is "sub" or "nameid" || c.Type == ClaimTypes.NameIdentifier)?.Value, out var i) ? i : 0;
            return new AuthUser
            {
                Id = id,
                Username = jwt.Claims.FirstOrDefault(c => c.Type is "unique_name" or "name" || c.Type == ClaimTypes.Name)?.Value ?? "",
                Email = jwt.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email)?.Value ?? "",
                Role = GetRoleFromClaims(jwt.Claims) ?? "User",
                Status = jwt.Claims.FirstOrDefault(c => c.Type.Equals("status", StringComparison.OrdinalIgnoreCase))?.Value ?? "Online",
                Avatar = jwt.Claims.FirstOrDefault(c => c.Type == "avatar")?.Value,
                AccessToken = token
            };
        }
        catch
        {
            return new AuthUser { AccessToken = token };
        }
    }

    private static string? GetRoleFromClaims(IEnumerable<Claim> claims)
    {
        var roleClaim = claims.FirstOrDefault(c =>
            c.Type.Equals("role", StringComparison.OrdinalIgnoreCase) ||
            c.Type.Equals("roles", StringComparison.OrdinalIgnoreCase) ||
            c.Type == ClaimTypes.Role);

        return string.IsNullOrWhiteSpace(roleClaim?.Value) ? null : roleClaim.Value.Trim();
    }

    private static bool HasRole(string? actualRole, string expectedRole)
        => string.Equals(actualRole, expectedRole, StringComparison.OrdinalIgnoreCase);
}
