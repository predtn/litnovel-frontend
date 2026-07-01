using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.AuditLogs;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    private const int PageSize = 10;

    public List<AuditLogDto> Logs { get; set; } = [];
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalElements { get; set; }
    public string? LoadError { get; set; }
    public string? ValidationError { get; set; }
    public string ActorRole { get; set; } = "";
    public string Query { get; set; } = "";

    public async Task<IActionResult> OnGetAsync(
        DateTime? fromDate,
        DateTime? toDate,
        string? actorRole,
        string? q,
        [FromQuery(Name = "page")] int pageNumber = 1)
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();

        FromDate = fromDate?.ToString("yyyy-MM-dd");
        ToDate = toDate?.ToString("yyyy-MM-dd");
        ActorRole = actorRole ?? "";
        Query = q ?? "";
        Page = Math.Max(1, pageNumber);

        if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
        {
            ValidationError = "Khoảng ngày không hợp lệ. Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.";
            return Page();
        }

        var token = auth.GetToken(HttpContext);
        var logs = new List<AuditLogDto>();
        var totalElements = 0;

        if (!IsStaffOnly)
        {
            var query = $"/api/admin/audit-logs?page={Page}&size={PageSize}"
                + (fromDate.HasValue ? $"&fromDate={FromDate}" : "")
                + (toDate.HasValue ? $"&toDate={ToDate}" : "");
            var result = await api.GetAsync<PagedData<AuditLogDto>>(query, token);
            if (result?.Success == true && result.Data != null)
            {
                logs.AddRange(result.Data.Items);
                totalElements += result.Data.TotalElements;
            }
            else LoadError = result?.Message ?? "Khong the tai audit logs.";
        }

        if (!IsAdminOnly)
        {
            var staffResult = await LoadStaffActionsAsync(Page, fromDate, toDate, token);
            logs.AddRange(staffResult.Items);
            totalElements += staffResult.TotalElements;
        }

        Logs = ApplySearch(logs)
            .OrderByDescending(log => log.DisplayCreatedAt)
            .Take(PageSize)
            .ToList();
        TotalElements = string.IsNullOrWhiteSpace(Query) ? totalElements : Logs.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling(TotalElements / (double)PageSize));

        return Page();
    }

    private bool IsAdminOnly => string.Equals(ActorRole, "admin", StringComparison.OrdinalIgnoreCase);
    private bool IsStaffOnly => string.Equals(ActorRole, "staff", StringComparison.OrdinalIgnoreCase);

    private IEnumerable<AuditLogDto> ApplySearch(IEnumerable<AuditLogDto> logs)
    {
        if (string.IsNullOrWhiteSpace(Query)) return logs;

        var term = Query.Trim();
        return logs.Where(log =>
            Contains(log.DisplayActor, term) ||
            Contains(log.DisplayAction, term) ||
            Contains(log.DisplayEntity, term) ||
            Contains(log.DisplayIp, term));
    }

    private static bool Contains(string? value, string term)
        => value?.Contains(term, StringComparison.OrdinalIgnoreCase) == true;

    private async Task<(List<AuditLogDto> Items, int TotalElements)> LoadStaffActionsAsync(int page, DateTime? fromDate, DateTime? toDate, string? token)
    {
        var query = $"/api/staff/history?page={page}&size={PageSize}"
            + (fromDate.HasValue ? $"&fromDate={FromDate}" : "")
            + (toDate.HasValue ? $"&toDate={ToDate}" : "");
        var result = await api.GetAsync<PagedData<ModerationHistoryDto>>(query, token);
        if (result?.Success != true || result.Data == null) return ([], 0);

        var staffLogs = result.Data.Items
            .Where(item => !fromDate.HasValue || item.PerformedAt.Date >= fromDate.Value.Date)
            .Where(item => !toDate.HasValue || item.PerformedAt.Date <= toDate.Value.Date)
            .Select(item => new AuditLogDto
            {
                Id = item.Id,
                ActorId = item.StaffId,
                ActorUsername = item.StaffUsername,
                Action = item.Action,
                EntityType = item.TargetType,
                EntityId = item.TargetId,
                EntityKey = item.TargetTitle,
                CreatedAt = item.PerformedAt,
                IpAddress = "Staff action"
            })
            .ToList();

        return (staffLogs, result.Data.TotalElements);
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
