using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Staff;

public class ReportDetailModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    public StaffReportDto? Report { get; set; }
    public string? Kind { get; set; }
    public string? ActionMessage { get; set; }
    public int? NovelAuthorId { get; set; }
    public string? NovelAuthorUsername { get; set; }

    public ReportDetailModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    private bool ValidateAccess(out string? token)
    {
        token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return false;
        if (!_auth.IsInRole(HttpContext, "Staff")) return false;
        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserEmail"] = user.Email; }
        return true;
    }

    public async Task<IActionResult> OnGetAsync([FromQuery] int id, [FromQuery] string kind)
    {
        if (!ValidateAccess(out var token)) return RedirectToPage("/Auth/Login");
        Kind = kind;
        var result = await _api.GetAsync<StaffReportDto>($"/api/staff/reports/{id}?kind={kind}", token);
        Report = result?.Data;
        if (Report == null) return NotFound();

        NovelAuthorId       = Report.TargetNovel?.Author?.Id;
        NovelAuthorUsername = Report.TargetNovel?.Author?.Username;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        [FromForm] int id, [FromForm] string kind,
        [FromForm] string action, [FromForm] string? actionTaken, [FromForm] string? resolutionNotes)
    {
        if (!ValidateAccess(out var token)) return RedirectToPage("/Auth/Login");
        Kind = kind;

        var payload = new { action, actionTaken, resolutionNotes };
        var result = await _api.PutAsync<object>($"/api/staff/reports/{id}/resolve?kind={kind}", payload, token);
        if (result?.Success == true)
        {
            TempData["SuccessMessage"] = "Báo cáo đã được xử lý thành công!";
            return RedirectToPage("/Staff/Reports");
        }
        // Reload
        var report = await _api.GetAsync<StaffReportDto>($"/api/staff/reports/{id}?kind={kind}", token);
        Report = report?.Data;
        ActionMessage = result?.Message ?? "Có lỗi xảy ra.";
        return Page();
    }
}
