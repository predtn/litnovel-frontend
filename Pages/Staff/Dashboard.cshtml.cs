using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Staff;

public class DashboardModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    public StaffDashboardDto Dashboard { get; set; } = new();
    public List<PendingNovelDto> PendingNovels { get; set; } = [];
    public List<StaffReportDto> PendingReports { get; set; } = [];

    public DashboardModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        if (!_auth.IsInRole(HttpContext, "Staff")) return RedirectToPage("/Index");

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserEmail"] = user.Email; }
        ViewData["ActiveNav"] = "staff";

        // Sequential awaits — backend uses scoped DbContext (not thread-safe)
        var db      = await _api.GetAsync<StaffDashboardDto>("/api/staff/dashboard", token);
        var novels  = await _api.GetAsync<PagedData<PendingNovelDto>>("/api/staff/novels/pending?page=1&size=5", token);
        var reports = await _api.GetAsync<PagedData<StaffReportDto>>("/api/staff/reports?status=Pending&page=1&size=5", token);

        Dashboard      = db?.Data ?? new();
        PendingNovels  = novels?.Data?.Items ?? [];
        PendingReports = reports?.Data?.Items ?? [];

        return Page();
    }
}
