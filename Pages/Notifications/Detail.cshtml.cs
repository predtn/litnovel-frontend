using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Notifications;

public class DetailModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public NotificationDto? Notification { get; set; }
    public string? ErrorMessage { get; set; }

    public DetailModel(IApiService api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserAvatar"] = user.Avatar; }

        // Thử GET /api/notifications/{id} trước
        var r = await _api.GetAsync<NotificationDto>($"/api/notifications/{id}", token);
        if (r?.Data != null)
        {
            Notification = r.Data;
        }
        else
        {
            // Fallback: load từ list rồi filter — phòng khi backend endpoint mới chưa sẵn sàng
            var listR = await _api.GetAsync<NotificationListDto>("/api/notifications?size=100", token);
            if (listR?.Data?.Items != null)
            {
                var match = listR.Data.Items.FirstOrDefault(n => n.Id == id);
                if (match != null) Notification = match;
            }

            if (Notification == null)
                ErrorMessage = r?.Message ?? "Không tìm thấy thông báo hoặc bạn không có quyền xem.";
        }

        // Normalize type field
        if (Notification != null && string.IsNullOrEmpty(Notification.Type))
            Notification.Type = Notification.NotificationType;

        // Auto mark as read
        if (Notification != null && !Notification.IsRead)
        {
            await _api.PutAsync<object>($"/api/notifications/{id}/read", null, token);
            Notification.IsRead = true;
            HttpContext.Session.Remove("_notif_count");
            HttpContext.Session.Remove("_notif_count_at");
        }

        return Page();
    }

}
