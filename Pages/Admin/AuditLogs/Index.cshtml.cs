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

    public async Task<IActionResult> OnGetAsync(DateTime? fromDate, DateTime? toDate, [FromQuery(Name = "page")] int pageNumber = 1)
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();

        FromDate = fromDate?.ToString("yyyy-MM-dd");
        ToDate = toDate?.ToString("yyyy-MM-dd");
        Page = Math.Max(1, pageNumber);

        if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
        {
            ValidationError = "Khoảng ngày không hợp lệ. Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.";
            return Page();
        }

        var query = $"/api/admin/audit-logs?page={Page}&size={PageSize}"
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

        await LoadStaffActionsAsync(Page, fromDate, toDate, auth.GetToken(HttpContext));

        return Page();
    }

    private async Task LoadStaffActionsAsync(int page, DateTime? fromDate, DateTime? toDate, string? token)
    {
        var query = $"/api/staff/history?page={page}&size={PageSize}"
            + (fromDate.HasValue ? $"&fromDate={FromDate}" : "")
            + (toDate.HasValue ? $"&toDate={ToDate}" : "");
        var result = await api.GetAsync<PagedData<ModerationHistoryDto>>(query, token);
        if (result?.Success != true || result.Data == null) return;

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

        Logs = Logs
            .Concat(staffLogs)
            .OrderByDescending(log => log.DisplayCreatedAt)
            .Take(PageSize)
            .ToList();
        TotalElements += result.Data.TotalElements;
        TotalPages = Math.Max(1, (int)Math.Ceiling(TotalElements / (double)PageSize));
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
