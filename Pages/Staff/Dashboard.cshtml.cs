using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Staff;

public class DashboardModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    public StaffDashboardDto Dashboard { get; set; } = new();
    public List<NovelSummaryDto> PendingNovels { get; set; } = [];
    public List<ReportDto> PendingReports { get; set; } = [];

    public DashboardModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        if (!_auth.IsInRole(HttpContext, "Staff")) return RedirectToPage("/Index");

        var dbTask     = _api.GetAsync<StaffDashboardDto>("/api/staff/dashboard", token);
        var novelTask  = _api.GetAsync<PagedData<NovelSummaryDto>>("/api/staff/novels/pending?page=1&size=5", token);
        var reportTask = _api.GetAsync<PagedData<ReportDto>>("/api/staff/reports?status=Pending&page=1&size=5", token);
        await Task.WhenAll(dbTask, novelTask, reportTask);

        Dashboard      = dbTask.Result?.Data     ?? new() { PendingNovels = 3, PendingChapters = 5, OpenReports = 8 };
        PendingNovels  = novelTask.Result?.Data?.Items  ?? GetMockPendingNovels();
        PendingReports = reportTask.Result?.Data?.Items ?? GetMockPendingReports();

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserEmail"] = user.Email; }
        return Page();
    }

    private List<NovelSummaryDto> GetMockPendingNovels() =>
    [
        new() { Id = 10, Title = "Kiếm Đạo Vô Song", Author = new() { Username = "author_a" }, UpdatedAt = DateTime.UtcNow.AddHours(-2) },
        new() { Id = 11, Title = "Thần Hoàng Tái Thế", Author = new() { Username = "author_b" }, UpdatedAt = DateTime.UtcNow.AddHours(-5) },
        new() { Id = 12, Title = "Đế Vương Chi Lộ",   Author = new() { Username = "author_c" }, UpdatedAt = DateTime.UtcNow.AddDays(-1) },
    ];

    private List<ReportDto> GetMockPendingReports() =>
    [
        new() { Id = 1, ReportType = "Inappropriate", Status = "Pending", Reporter = new() { Username = "user_x" }, TargetNovel = new() { Title = "Truyện A" } },
        new() { Id = 2, ReportType = "Copyright",     Status = "Pending", Reporter = new() { Username = "user_y" }, TargetNovel = new() { Title = "Truyện B" } },
    ];
}
