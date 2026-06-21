using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.AuditLogs;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public List<AuditLogDto> Logs { get; set; } = [];
    public string? LoadError { get; set; }

    public async Task<IActionResult> OnGetAsync(int? actorId, string? entityType, DateTime? fromDate, DateTime? toDate)
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();
        var query = $"/api/admin/audit-logs?page=1&size=50"
            + (actorId.HasValue ? $"&actorId={actorId}" : "")
            + (string.IsNullOrWhiteSpace(entityType) ? "" : $"&entityType={Uri.EscapeDataString(entityType)}")
            + (fromDate.HasValue ? $"&fromDate={fromDate:yyyy-MM-dd}" : "")
            + (toDate.HasValue ? $"&toDate={toDate:yyyy-MM-dd}" : "");
        var result = await api.GetAsync<PagedData<AuditLogDto>>(query, auth.GetToken(HttpContext));
        if (result?.Success == true && result.Data != null) Logs = result.Data.Items;
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
