using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Announcements;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    private const int BannerVisibleSeconds = 30;
    private const int BannerRepeatCount = 3;
    private const int BannerRepeatGapMinutes = 15;

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

    public async Task<IActionResult> OnPostAddAsync(string title, string content, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            TempData["Error"] = "Vui lòng nhập tiêu đề và nội dung thông báo.";
            return RedirectToPage();
        }

        var startDate = DateTime.UtcNow;
        var endDate = startDate
            .AddMinutes(BannerRepeatGapMinutes * (BannerRepeatCount - 1))
            .AddSeconds(BannerVisibleSeconds);
        var token = auth.GetToken(HttpContext);

        var result = await api.PostAsync<AnnouncementDto>("/api/admin/announcements", new
        {
            title = title.Trim(),
            content = content.Trim(),
            startDate,
            endDate,
            isActive
        }, token);

        if (result?.Success != true)
        {
            TempData["Error"] = result?.Message ?? "Backend chưa lưu được banner.";
            return RedirectToPage();
        }

        var saved = await VerifyAnnouncementSavedAsync(result.Data, title, content, startDate, token);
        if (saved)
        {
            TempData["Success"] = "Banner đã được lưu và sẵn sàng hiển thị cho người dùng.";
        }
        else
        {
            TempData["Error"] = "Backend báo tạo thành công nhưng chưa tìm thấy banner trong danh sách lưu trữ. User sẽ chưa thấy banner qua /api/announcements.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await api.DeleteAsync<object>($"/api/admin/announcements/{id}", auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Đã xóa banner.";
        else TempData["Error"] = result?.Message ?? "Không thể xóa banner.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var result = await api.PutAsync<object>($"/api/admin/announcements/{id}/toggle", null, auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Đã cập nhật trạng thái banner.";
        else TempData["Error"] = result?.Message ?? "Không thể cập nhật trạng thái banner.";
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

    private async Task<bool> VerifyAnnouncementSavedAsync(AnnouncementDto? created, string title, string content, DateTime startDate, string? token)
    {
        var result = await api.GetAsync<List<AnnouncementDto>>("/api/admin/announcements", token);
        if (result?.Success != true || result.Data == null) return false;

        if (created?.Id > 0 && result.Data.Any(item => item.Id == created.Id))
        {
            return true;
        }

        var normalizedTitle = title.Trim();
        var normalizedContent = content.Trim();
        return result.Data.Any(item =>
            string.Equals(item.Title?.Trim(), normalizedTitle, StringComparison.Ordinal) &&
            string.Equals(item.Content?.Trim(), normalizedContent, StringComparison.Ordinal) &&
            Math.Abs((item.StartDate.ToUniversalTime() - startDate).TotalMinutes) <= 2);
    }
}
