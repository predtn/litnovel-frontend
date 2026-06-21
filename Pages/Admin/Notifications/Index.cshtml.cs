using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Notifications;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public List<SentNotificationDto> Notifications { get; set; } = [];
    public string? LoadError { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();
        var result = await api.GetAsync<PagedData<SentNotificationDto>>("/api/admin/notifications/sent?page=1&size=20", auth.GetToken(HttpContext));
        if (result?.Success == true && result.Data != null) Notifications = result.Data.Items;
        else LoadError = result?.Message ?? "Khong the tai lich su thong bao.";
        return Page();
    }

    public async Task<IActionResult> OnPostSendAsync(string notificationType, string message, bool targetAll, int? targetUserId)
    {
        await api.PostAsync<object>("/api/admin/notifications", new { notificationType, message, targetAll, targetUserId }, auth.GetToken(HttpContext));
        TempData["Success"] = "Notification sent.";
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
