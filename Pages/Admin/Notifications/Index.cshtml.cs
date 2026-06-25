using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Notifications;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public List<SentNotificationDto> Notifications { get; set; } = [];
    public List<UserDetailDto> RecipientUsers { get; set; } = [];
    public string? LoadError { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();
        var token = auth.GetToken(HttpContext);
        var historyTask = api.GetAsync<PagedData<SentNotificationDto>>("/api/admin/notifications/sent?page=1&size=20", token);
        var usersTask = api.GetAsync<PagedData<UserDetailDto>>("/api/admin/users?page=1&size=500", token);

        await Task.WhenAll(historyTask, usersTask);

        var historyResult = historyTask.Result;
        if (historyResult?.Success == true && historyResult.Data != null) Notifications = historyResult.Data.Items;
        else LoadError = historyResult?.Message ?? "Khong the tai lich su thong bao.";

        var usersResult = usersTask.Result;
        if (usersResult?.Success == true && usersResult.Data != null)
        {
            RecipientUsers = usersResult.Data.Items
                .Where(user => !user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                .OrderBy(user => user.Username)
                .ToList();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSendAsync(string notificationType, string message, bool targetAll, int? targetUserId)
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");

        if (!targetAll && !targetUserId.HasValue)
        {
            TempData["Error"] = "Vui lòng chọn người nhận cụ thể.";
            return RedirectToPage();
        }

        var token = auth.GetToken(HttpContext);
        if (!targetAll && targetUserId.HasValue)
        {
            var userResult = await api.GetAsync<UserDetailDto>($"/api/admin/users/{targetUserId.Value}", token);
            if (userResult?.Success != true || userResult.Data == null)
            {
                TempData["Error"] = "Không tìm thấy người nhận đã chọn.";
                return RedirectToPage();
            }

            if (userResult.Data.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Không thể gửi thông báo user cụ thể đến tài khoản admin.";
                return RedirectToPage();
            }
        }

        var result = await api.PostAsync<object>(
            "/api/admin/notifications",
            new { notificationType, message, targetAll, targetUserId = targetAll ? null : targetUserId },
            token);

        if (result?.Success == true) TempData["Success"] = result.Message ?? "Notification sent.";
        else TempData["Error"] = result?.Message ?? "Could not send notification.";
        return RedirectToPage();
    }

    private void SetShell()
    {
        ViewData["AdminSection"] = "notifications";
        var user = auth.GetCurrentUser(HttpContext);
        if (user == null) return;
        ViewData["UserName"] = user.Username;
        ViewData["UserEmail"] = user.Email;
    }
}
