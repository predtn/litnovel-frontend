using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Tags;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public List<TagDto> Tags { get; set; } = [];
    public string? LoadError { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var token = auth.GetToken(HttpContext);
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();

        var result = await api.GetAsync<List<TagDto>>("/api/admin/tags", token);
        if (result?.Success == true && result.Data != null)
        {
            Tags = result.Data;
            await EnrichNovelCountsAsync(token);
        }
        else
        {
            LoadError = result?.Message ?? "Khong the tai danh sach tag.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(string name)
    {
        var result = await api.PostAsync<object>("/api/admin/tags", new { name, slug = ToSlug(name) }, auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Tag created.";
        else TempData["Error"] = result?.Message ?? "Create tag failed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, string name)
    {
        var result = await api.PutAsync<object>($"/api/admin/tags/{id}", new { name, slug = ToSlug(name) }, auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Tag updated.";
        else TempData["Error"] = result?.Message ?? "Update tag failed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await api.DeleteAsync<object>($"/api/admin/tags/{id}", auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Tag deleted.";
        else TempData["Error"] = result?.Message ?? "Delete tag failed.";
        return RedirectToPage();
    }

    private void SetShell()
    {
        ViewData["AdminSection"] = "tags";
        var user = auth.GetCurrentUser(HttpContext);
        if (user == null) return;
        ViewData["UserName"] = user.Username;
        ViewData["UserEmail"] = user.Email;
    }

    private async Task EnrichNovelCountsAsync(string? token)
    {
        foreach (var tag in Tags)
        {
            var result = await api.GetAsync<PagedData<NovelSummaryDto>>($"/api/novels?tagId={tag.Id}&page=1&size=1", token);
            if (result?.Success == true && result.Data != null)
            {
                tag.NovelCount = result.Data.TotalElements;
            }
        }
    }

    private static string ToSlug(string value)
    {
        var normalized = value.ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
        var slug = System.Text.RegularExpressions.Regex.Replace(new string(chars).Normalize(System.Text.NormalizationForm.FormC), @"[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "tag" : slug;
    }
}
