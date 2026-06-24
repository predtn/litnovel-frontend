using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Backup;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public List<BackupDto> Backups { get; set; } = [];
    public string? LoadError { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();
        var result = await api.GetAsync<List<BackupDto>>("/api/admin/backups", auth.GetToken(HttpContext));
        if (result?.Success == true && result.Data != null) Backups = result.Data;
        else LoadError = result?.Message ?? "Khong the tai danh sach backup.";
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var result = await api.PostAsync<object>("/api/admin/backups", null, auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = result.Message ?? "Backup job started.";
        else TempData["Error"] = result?.Message ?? "Could not start backup job.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRestoreAsync(string id, string confirmationText)
    {
        var result = await api.PostAsync<object>($"/api/admin/backups/{id}/restore", new { confirmationText }, auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = result.Message ?? "Restore job started.";
        else TempData["Error"] = result?.Message ?? "Could not start restore job.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var result = await api.DeleteAsync<object>($"/api/admin/backups/{id}", auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = result.Message ?? "Backup deleted.";
        else TempData["Error"] = result?.Message ?? "Could not delete backup.";
        return RedirectToPage();
    }

    private void SetShell()
    {
        ViewData["AdminSection"] = "backup";
        var user = auth.GetCurrentUser(HttpContext);
        if (user == null) return;
        ViewData["UserName"] = user.Username;
        ViewData["UserEmail"] = user.Email;
    }
}
