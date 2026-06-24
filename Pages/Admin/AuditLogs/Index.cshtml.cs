using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.AuditLogs;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public List<AuditLogDto> Logs { get; set; } = [];
    public int? ActorId { get; set; }
    public string? EntityType { get; set; }
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalElements { get; set; }
    public string? LoadError { get; set; }

    public async Task<IActionResult> OnGetAsync(int? actorId, string? entityType, DateTime? fromDate, DateTime? toDate, int page = 1)
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();

        ActorId = actorId;
        EntityType = entityType;
        FromDate = fromDate?.ToString("yyyy-MM-dd");
        ToDate = toDate?.ToString("yyyy-MM-dd");
        Page = Math.Max(1, page);

        var query = $"/api/admin/audit-logs?page={Page}&size=25"
            + (ActorId.HasValue ? $"&actorId={ActorId}" : "")
            + (string.IsNullOrWhiteSpace(EntityType) ? "" : $"&entityType={Uri.EscapeDataString(EntityType)}")
            + (fromDate.HasValue ? $"&fromDate={FromDate}" : "")
            + (toDate.HasValue ? $"&toDate={ToDate}" : "");
        var result = await api.GetAsync<PagedData<AuditLogDto>>(query, auth.GetToken(HttpContext));
        if (result?.Success == true && result.Data != null)
        {
            Logs = result.Data.Items;
            TotalPages = Math.Max(1, result.Data.TotalPages);
            TotalElements = result.Data.TotalElements;
        }
        else LoadError = result?.Message ?? "Khong the tai audit logs.";
        return Page();
    }

    private void SetShell()
    {
        ViewData["AdminSection"] = "audit";
        var user = auth.GetCurrentUser(HttpContext);
        if (user == null) return;
        ViewData["UserName"] = user.Username;
        ViewData["UserEmail"] = user.Email;
    }
}
