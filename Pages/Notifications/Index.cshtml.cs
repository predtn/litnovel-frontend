using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Notifications;

public class IndexModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public List<NotificationDto> Notifications { get; set; } = [];
    public int UnreadCount { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public string? Filter { get; set; }

    public IndexModel(IApiService api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> OnGetAsync(string? filter, int page = 1)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

        Filter = filter;
        CurrentPage = page;

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserAvatar"] = user.Avatar; }

        // Build query — server trả về unreadCount trong response body
        var isRead = filter switch { "unread" => "false", "read" => "true", _ => null };
        var qs = $"/api/notifications?page={page}&size=15{(isRead != null ? $"&isRead={isRead}" : "")}";

        var r = await _api.GetAsync<NotificationListDto>(qs, token);
        if (r?.Data != null)
        {
            // Map từ backend DTO
            Notifications = r.Data.Items.Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.NotificationType,
                NotificationType = n.NotificationType,
                Message = n.Message,
                EntityType = n.EntityType,
                EntityId = n.EntityId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();

            TotalPages = r.Data.TotalPages;
            // unreadCount trả về trực tiếp từ API (không cần gọi thêm)
            UnreadCount = r.Data.UnreadCount;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostMarkOneAsync(int id)
    {
        var token = _auth.GetToken(HttpContext);
        if (!string.IsNullOrEmpty(token))
        {
            await _api.PutAsync<object>($"/api/notifications/{id}/read", null, token);
            // Xóa cache badge navbar để hiển thị count mới
            HttpContext.Session.Remove("_notif_count");
            HttpContext.Session.Remove("_notif_count_at");
        }
        return RedirectToPage(new { filter = Filter, page = CurrentPage });
    }

    public async Task<IActionResult> OnPostMarkAllAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (!string.IsNullOrEmpty(token))
        {
            await _api.PutAsync<object>("/api/notifications/read-all", null, token);
            // Xóa cache badge navbar
            HttpContext.Session.Remove("_notif_count");
            HttpContext.Session.Remove("_notif_count_at");
        }
        return RedirectToPage();
    }

}
