using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Staff;

public class ReportsModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    public List<StaffReportDto> Reports { get; set; } = [];
    public string? StatusFilter { get; set; }
    public string? KindFilter { get; set; }
    public int PendingCount { get; set; }
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;

    public ReportsModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> OnGetAsync(
        [FromQuery] string? status = "Pending",
        [FromQuery] string? kind = null,
        [FromQuery] int page = 1)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        if (!_auth.IsInRole(HttpContext, "Staff")) return RedirectToPage("/Index");

        StatusFilter = status;
        KindFilter   = kind;
        Page         = page < 1 ? 1 : page;

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserEmail"] = user.Email; }

        var kindParam   = string.IsNullOrEmpty(kind) ? "" : $"&kind={kind}";
        var statusParam = string.IsNullOrEmpty(status) ? "" : $"&status={status}";
        var reportsTask = _api.GetAsync<PagedData<StaffReportDto>>($"/api/staff/reports?page={Page}&size=20{kindParam}{statusParam}", token);
        var pendingTask = _api.GetAsync<PagedData<StaffReportDto>>("/api/staff/reports?status=Pending&page=1&size=1", token);
        await Task.WhenAll(reportsTask, pendingTask);

        var data = reportsTask.Result?.Data;
        Reports      = data?.Items ?? [];
        TotalPages   = data?.TotalPages ?? 1;
        PendingCount = pendingTask.Result?.Data?.TotalElements ?? 0;

        return Page();
    }
}
