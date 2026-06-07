using litnovel_frontend.Services; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Admin.Settings;
public class SystemSettings { public string SiteName="LitNovel"; public string SiteDescription="Nền tảng đọc và xuất bản truyện chữ"; public string ContactEmail="admin@litnovel.com"; public bool RequireModerationForNovels=true; public bool RequireModerationForChapters=true; public bool AllowGuestRead=true; public int MaxChaptersPerDay=5; public int SessionTimeoutMinutes=60; }
public class IndexModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public SystemSettings Settings { get; set; } = new();
    public IndexModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (!_auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        var r = await _api.GetAsync<SystemSettings>("/api/admin/settings", token);
        Settings = r?.Data ?? new();
        var u = _auth.GetCurrentUser(HttpContext); if (u!=null){ViewData["UserName"]=u.Username;ViewData["UserEmail"]=u.Email;}
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(string siteName, string? siteDescription, string? contactEmail, bool requireModerationForNovels, bool requireModerationForChapters, bool allowGuestRead, int maxChaptersPerDay, int sessionTimeout)
    {
        var token = _auth.GetToken(HttpContext);
        await _api.PutAsync<object>("/api/admin/settings", new { siteName, siteDescription, contactEmail, requireModerationForNovels, requireModerationForChapters, allowGuestRead, maxChaptersPerDay, sessionTimeoutMinutes = sessionTimeout }, token);
        TempData["Success"] = "Đã lưu cài đặt!"; return RedirectToPage();
    }
}
