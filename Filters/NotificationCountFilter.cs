using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Filters;

/// <summary>
/// Inject unread notification count vào ViewData["UnreadCount"] cho mọi Razor Page.
/// Chỉ chạy khi user đã đăng nhập. Dùng cache trong Session để tránh gọi API mỗi request.
/// </summary>
public class NotificationCountFilter : IAsyncPageFilter
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public NotificationCountFilter(IApiService api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (context.HandlerInstance is PageModel page &&
            context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            var token = _auth.GetToken(context.HttpContext);
            if (!string.IsNullOrEmpty(token))
            {
                // Kiểm tra cache trong session (TTL 60 giây)
                var httpContext = context.HttpContext;
                var cached = httpContext.Session.GetString("_notif_count");
                var cachedAt = httpContext.Session.GetString("_notif_count_at");

                bool cacheValid = cached != null &&
                    cachedAt != null &&
                    DateTime.TryParse(cachedAt, out var at) &&
                    (DateTime.UtcNow - at).TotalSeconds < 60;

                int unreadCount = 0;

                if (cacheValid)
                {
                    _ = int.TryParse(cached, out unreadCount);
                }
                else
                {
                    try
                    {
                        var r = await _api.GetAsync<PagedData<NotificationDto>>(
                            "/api/notifications?isRead=false&size=1", token);
                        unreadCount = r?.Data?.TotalElements ?? 0;

                        // Cache vào session
                        httpContext.Session.SetString("_notif_count", unreadCount.ToString());
                        httpContext.Session.SetString("_notif_count_at", DateTime.UtcNow.ToString("O"));
                    }
                    catch
                    {
                        // Bỏ qua lỗi API — không làm gián đoạn request
                    }
                }

                page.ViewData["UnreadCount"] = unreadCount;
            }
        }

        await next();
    }
}
