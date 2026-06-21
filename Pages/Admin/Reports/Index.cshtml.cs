using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Reports;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public List<AdminReportDto> Reports { get; set; } = [];
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public string? LoadError { get; set; }

    public async Task<IActionResult> OnGetAsync(string? type, string? status)
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();
        Type = type ?? "";
        Status = status ?? "";
        var qs = $"/api/admin/reports?page=1&size=20{(string.IsNullOrEmpty(Type) ? "" : $"&type={Type}")}{(string.IsNullOrEmpty(Status) ? "" : $"&status={Status}")}";
        var result = await api.GetAsync<PagedData<AdminReportDto>>(qs, auth.GetToken(HttpContext));
        if (result?.Success == true && result.Data != null) Reports = result.Data.Items;
        else LoadError = result?.Message ?? "Khong the tai danh sach report.";
        return Page();
    }

    private void SetShell()
    {
        ViewData["AdminSection"] = "reports";
        var user = auth.GetCurrentUser(HttpContext);
        if (user == null) return;
        ViewData["UserName"] = user.Username;
        ViewData["UserEmail"] = user.Email;
    }
}
