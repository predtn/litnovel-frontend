using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Staff;

public class NovelReviewModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public NovelReviewDetailDto? Novel { get; set; }
    public bool ActionSuccess { get; set; }
    public string? ActionMessage { get; set; }

    public NovelReviewModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    private async Task<bool> ValidateAccess()
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return false;
        if (!_auth.IsInRole(HttpContext, "Staff")) return false;
        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserEmail"] = user.Email; }
        return true;
    }

    public async Task<IActionResult> OnGetAsync([FromQuery] int id)
    {
        if (!await ValidateAccess()) return RedirectToPage("/Auth/Login");
        var token = _auth.GetToken(HttpContext)!;

        var result = await _api.GetAsync<NovelReviewDetailDto>($"/api/staff/novels/{id}", token);
        Novel = result?.Data;
        if (Novel == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync([FromForm] int id, [FromForm] string action, [FromForm] string? reason)
    {
        if (!await ValidateAccess()) return RedirectToPage("/Auth/Login");
        var token = _auth.GetToken(HttpContext)!;

        var payload = new { action, reason };
        var result = await _api.PutAsync<object>($"/api/staff/novels/{id}/moderate", payload, token);
        if (result?.Success == true)
        {
            TempData["SuccessMessage"] = $"Tiểu thuyết đã được {action.ToLower()} thành công!";
            return RedirectToPage("/Staff/PendingNovels");
        }

        // Reload and show error
        var novel = await _api.GetAsync<NovelReviewDetailDto>($"/api/staff/novels/{id}", token);
        Novel = novel?.Data;
        ActionMessage = result?.Message ?? "Có lỗi xảy ra.";
        return Page();
    }
}
