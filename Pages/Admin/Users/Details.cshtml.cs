using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Users;

public class DetailsModel(IApiService api, IAuthService auth) : PageModel
{
    public UserDetailDto UserDetail { get; set; } = new();
    public string? LoadError { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();
        var result = await api.GetAsync<UserDetailDto>($"/api/admin/users/{id}", auth.GetToken(HttpContext));
        if (result?.Success == true && result.Data != null) UserDetail = result.Data;
        else LoadError = result?.Message ?? "Khong the tai chi tiet nguoi dung.";
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(int id, string role, string status)
    {
        var token = auth.GetToken(HttpContext);
        var result = await api.PutAsync<object>($"/api/admin/users/{id}", new { role, status }, token);
        if (result?.Success == true)
        {
            if (role.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                await SendUserNotificationAsync(id, "Tài khoản của bạn đã được cấp quyền Staff. Staff Dashboard hiện đã sẵn sàng để sử dụng.", token);
            }

            if (status.Equals("Banned", StringComparison.OrdinalIgnoreCase))
            {
                await SendUserNotificationAsync(id, "Tài khoản của bạn đã bị cấm do vi phạm quy định cộng đồng.", token);
            }

            TempData["Success"] = "Đã cập nhật người dùng.";
        }
        else
        {
            TempData["Error"] = result?.Message ?? "Không thể cập nhật người dùng.";
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostBanAsync(int id, string? reason)
    {
        var token = auth.GetToken(HttpContext);
        var result = await api.PostAsync<object>($"/api/admin/users/{id}/ban", new { reason = string.IsNullOrWhiteSpace(reason) ? "Admin action" : reason }, token);
        if (result?.Success == true)
        {
            await SendUserNotificationAsync(id, "Tài khoản của bạn đã bị cấm do vi phạm quy định cộng đồng.", token);
            TempData["Success"] = "Đã cấm người dùng và gửi thông báo.";
        }
        else
        {
            TempData["Error"] = result?.Message ?? "Không thể cấm người dùng.";
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostWarnAsync(int id, string reason, string severity)
    {
        await api.PostAsync<object>($"/api/staff/users/{id}/warn", new { reason, severity }, auth.GetToken(HttpContext));
        TempData["Success"] = "Warning issued.";
        return RedirectToPage(new { id });
    }

    private void SetShell()
    {
        ViewData["AdminSection"] = "users";
        var user = auth.GetCurrentUser(HttpContext);
        if (user == null) return;
        ViewData["UserName"] = user.Username;
        ViewData["UserEmail"] = user.Email;
    }

    private async Task SendUserNotificationAsync(int userId, string message, string? token)
    {
        await api.PostAsync<object>("/api/admin/notifications", new
        {
            notificationType = "SystemAlert",
            message,
            targetAll = false,
            targetUserId = userId
        }, token);
    }

}
