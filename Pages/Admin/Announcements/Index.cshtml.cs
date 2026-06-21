using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Announcements;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public List<AnnouncementDto> Announcements { get; set; } = [];
    public string? LoadError { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();
        var result = await api.GetAsync<List<AnnouncementDto>>("/api/admin/announcements", auth.GetToken(HttpContext));
        if (result?.Success == true && result.Data != null) Announcements = result.Data;
        else LoadError = result?.Message ?? "Khong the tai announcements.";
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(string title, string content, DateTime startDate, DateTime? endDate, bool isActive)
    {
        if (endDate.HasValue && endDate.Value <= startDate)
        {
            TempData["Error"] = "End date phải sau start date.";
            return RedirectToPage();
        }

        var result = await api.PostAsync<object>("/api/admin/announcements", new
        {
            title,
            content,
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc),
            endDate = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc) : (DateTime?)null,
            isActive
        }, auth.GetToken(HttpContext));

        if (result?.Success == true) TempData["Success"] = "Đã tạo announcement công khai.";
        else TempData["Error"] = result?.Message ?? "Không thể tạo announcement.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await api.DeleteAsync<object>($"/api/admin/announcements/{id}", auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Đã xóa announcement.";
        else TempData["Error"] = result?.Message ?? "Không thể xóa announcement.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var result = await api.PutAsync<object>($"/api/admin/announcements/{id}/toggle", null, auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Đã bật/tắt announcement.";
        else TempData["Error"] = result?.Message ?? "Không thể bật/tắt announcement.";
        return RedirectToPage();
    }

    private void SetShell()
    {
        ViewData["AdminSection"] = "announcements";
        var user = auth.GetCurrentUser(HttpContext);
        if (user == null) return;
        ViewData["UserName"] = user.Username;
        ViewData["UserEmail"] = user.Email;
    }
}
