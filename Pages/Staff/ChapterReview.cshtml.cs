using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Staff;

public class ChapterReviewModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    public ChapterReviewDetailDto? Chapter { get; set; }
    public string? ActionMessage { get; set; }

    public ChapterReviewModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    private bool ValidateAccess(out string? token)
    {
        token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return false;
        if (!_auth.IsInRole(HttpContext, "Staff")) return false;
        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserEmail"] = user.Email; }
        return true;
    }

    public async Task<IActionResult> OnGetAsync([FromQuery] int id)
    {
        if (!ValidateAccess(out var token)) return RedirectToPage("/Auth/Login");
        var result = await _api.GetAsync<ChapterReviewDetailDto>($"/api/staff/chapters/{id}", token);
        Chapter = result?.Data;
        if (Chapter == null) return NotFound();
        if (!IsPending(Chapter.Status))
        {
            TempData["Error"] = "Chương này không còn trong hàng chờ duyệt.";
            return RedirectToPage("/Staff/PendingChapters");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync([FromForm] int id, [FromForm] string action, [FromForm] string? reason)
    {
        if (!ValidateAccess(out var token)) return RedirectToPage("/Auth/Login");
        var current = await _api.GetAsync<ChapterReviewDetailDto>($"/api/staff/chapters/{id}", token);
        if (current?.Data == null || !IsPending(current.Data.Status))
        {
            TempData["Error"] = "Chương này đã được rút khỏi hàng chờ duyệt.";
            return RedirectToPage("/Staff/PendingChapters");
        }

        var payload = new { action, reason };
        var result = await _api.PutAsync<object>($"/api/staff/chapters/{id}/moderate", payload, token);
        if (result?.Success == true)
        {
            TempData["SuccessMessage"] = $"Chương đã được {action.ToLower()} thành công!";
            return RedirectToPage("/Staff/PendingChapters");
        }
        // Reload
        var ch = await _api.GetAsync<ChapterReviewDetailDto>($"/api/staff/chapters/{id}", token);
        Chapter = ch?.Data;
        if (Chapter == null || !IsPending(Chapter.Status))
        {
            TempData["Error"] = "Chương này không còn trong hàng chờ duyệt.";
            return RedirectToPage("/Staff/PendingChapters");
        }
        ActionMessage = result?.Message ?? "Có lỗi xảy ra.";
        return Page();
    }

    private static bool IsPending(string? status)
        => string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase);
}
