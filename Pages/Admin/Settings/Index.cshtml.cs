using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Settings;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public AdminSettingsDto Settings { get; set; } = new();
    public string? LoadError { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var token = auth.GetToken(HttpContext);
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();
        var result = await api.GetAsync<AdminSettingsDto>("/api/admin/settings", token);
        if (result?.Success == true && result.Data != null) Settings = result.Data;
        else LoadError = result?.Message ?? "Khong the tai cau hinh he thong.";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        string siteName,
        string tagline,
        bool maintenanceMode,
        int maxNovelDescriptionLength,
        int maxChapterLength,
        int maxTagsPerNovel,
        int reviewSLAHours,
        string? autoFlagKeywords)
    {
        var request = new AdminSettingsDto
        {
            General = new() { SiteName = siteName, Tagline = tagline, MaintenanceMode = maintenanceMode },
            Content = new() { MaxNovelDescriptionLength = maxNovelDescriptionLength, MaxChapterLength = maxChapterLength, MaxTagsPerNovel = maxTagsPerNovel },
            Moderation = new()
            {
                ReviewSLAHours = reviewSLAHours,
                AutoFlagKeywords = string.IsNullOrWhiteSpace(autoFlagKeywords)
                    ? []
                    : autoFlagKeywords.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList()
            }
        };

        var result = await api.PutAsync<object>("/api/admin/settings", request, auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = result.Message ?? "Settings saved.";
        else TempData["Error"] = result?.Message ?? "Could not save settings.";
        return RedirectToPage();
    }

    private void SetShell()
    {
        ViewData["AdminSection"] = "settings";
        var user = auth.GetCurrentUser(HttpContext);
        if (user == null) return;
        ViewData["UserName"] = user.Username;
        ViewData["UserEmail"] = user.Email;
    }
}
