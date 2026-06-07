using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Notifications;
public class IndexModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public List<NotificationDto> Notifications { get; set; } = [];
    public int UnreadCount { get; set; }
    public IndexModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        var r = await _api.GetAsync<List<NotificationDto>>("/api/notifications", token);
        Notifications = r?.Data ?? GetMock();
        UnreadCount = Notifications.Count(n => !n.IsRead);
        var u = _auth.GetCurrentUser(HttpContext);
        if (u != null) { ViewData["UserName"] = u.Username; ViewData["UserAvatar"] = u.Avatar; }
        return Page();
    }
    public async Task<IActionResult> OnPostMarkAllAsync()
    {
        var token = _auth.GetToken(HttpContext);
        await _api.PutAsync<object>("/api/notifications/read-all", null, token);
        return RedirectToPage();
    }
    private List<NotificationDto> GetMock() =>
    [
        new(){Id=1, Type="NewChapter", Title="Chương mới từ Long Vương Truyền Thuyết", Body="Chương 52 vừa được đăng!", IsRead=false, CreatedAt=DateTime.UtcNow.AddHours(-1)},
        new(){Id=2, Type="CommentReply", Title="Có người trả lời bình luận của bạn", Body="Cảm ơn bạn, mình cũng nghĩ vậy!", IsRead=false, CreatedAt=DateTime.UtcNow.AddHours(-3)},
        new(){Id=3, Type="BadgeEarned", Title="Bạn đã đạt huy hiệu mới!", Body="Huy hiệu: Độc giả chăm chỉ", IsRead=true, CreatedAt=DateTime.UtcNow.AddDays(-1)},
    ];
}
