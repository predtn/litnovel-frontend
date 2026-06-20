using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Users;

public class IndexModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public List<UserDetailDto> Users { get; set; } = [];
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalElements { get; set; }
    public string? Keyword { get; set; }
    public string? RoleFilter { get; set; }
    public string? StatusFilter { get; set; }
    public string? LoadError { get; set; }

    public IndexModel(IApiService api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> OnGetAsync(string? keyword, string? role, string? status, int page = 1)
    {
        var token = _auth.GetToken(HttpContext);
        if (!_auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");

        Keyword = keyword;
        RoleFilter = role;
        StatusFilter = status;
        Page = page;

        var qs = $"/api/admin/users?page={page}&size=20"
            + (string.IsNullOrEmpty(keyword) ? "" : $"&keyword={Uri.EscapeDataString(keyword)}")
            + (string.IsNullOrEmpty(role) ? "" : $"&role={role}")
            + (string.IsNullOrEmpty(status) ? "" : $"&status={status}");

        var result = await _api.GetAsync<PagedData<UserDetailDto>>(qs, token);
        if (result?.Success == true && result.Data != null)
        {
            Users = result.Data.Items;
            TotalPages = result.Data.TotalPages;
            TotalElements = result.Data.TotalElements;
        }
        else
        {
            LoadError = result?.Message ?? "Không thể tải danh sách người dùng.";
            TotalPages = 1;
            TotalElements = 0;
        }

        SetShell();
        return Page();
    }

    public async Task<IActionResult> OnPostBanAsync(int userId)
    {
        var token = _auth.GetToken(HttpContext);
        var result = await _api.PostAsync<object>($"/api/admin/users/{userId}/ban", new { reason = "Admin action" }, token);
        if (result?.Success == true)
        {
            await SendUserNotificationAsync(userId, "Tài khoản của bạn đã bị cấm do vi phạm quy định cộng đồng.", token);
            TempData["Success"] = "Đã cấm người dùng và gửi thông báo.";
        }
        else
        {
            TempData["Error"] = result?.Message ?? "Không thể cấm người dùng.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnbanAsync(int userId)
    {
        var token = _auth.GetToken(HttpContext);
        var result = await _api.PostAsync<object>($"/api/admin/users/{userId}/unban", null, token);
        if (result?.Success == true) TempData["Success"] = "Đã bỏ cấm người dùng.";
        else TempData["Error"] = result?.Message ?? "Không thể bỏ cấm người dùng.";
        return RedirectToPage();
    }

    private async Task SendUserNotificationAsync(int userId, string message, string? token)
    {
        await _api.PostAsync<object>("/api/admin/notifications", new
        {
            notificationType = "SystemAlert",
            message,
            targetAll = false,
            targetUserId = userId
        }, token);
    }

    private void SetShell()
    {
        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null)
        {
            ViewData["UserName"] = user.Username;
            ViewData["UserEmail"] = user.Email;
        }

        ViewData["AdminSection"] = "users";
    }
}
