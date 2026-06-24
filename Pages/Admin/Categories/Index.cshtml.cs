using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.Categories;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public List<CategoryDto> Categories { get; set; } = [];
    public List<TaxonomyListItemDto> Items { get; set; } = [];
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalElements { get; set; }
    public string? LoadError { get; set; }
    private const int PageSize = 10;

    public async Task<IActionResult> OnGetAsync([FromQuery(Name = "page")] int pageNumber = 1)
    {
        var token = auth.GetToken(HttpContext);
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();
        Page = Math.Max(1, pageNumber);

        var result = await api.GetAsync<List<CategoryDto>>("/api/admin/categories", token);
        if (result?.Success == true && result.Data != null)
        {
            Categories = result.Data;
            await EnrichNovelCountsAsync(token);
            TotalElements = Categories.Count;
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalElements / (double)PageSize));
            Page = Math.Min(Page, TotalPages);
            Items = Categories
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .Select(category => new TaxonomyListItemDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Slug = category.Slug,
                    NovelCount = category.NovelCount
                })
                .ToList();
        }
        else
        {
            LoadError = result?.Message ?? "Khong the tai danh sach the loai.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(string name)
    {
        var result = await api.PostAsync<object>("/api/admin/categories", new { name, slug = ToSlug(name) }, auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Category created.";
        else TempData["Error"] = result?.Message ?? "Create category failed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, string name)
    {
        var result = await api.PutAsync<object>($"/api/admin/categories/{id}", new { name, slug = ToSlug(name) }, auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Category updated.";
        else TempData["Error"] = result?.Message ?? "Update category failed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await api.DeleteAsync<object>($"/api/admin/categories/{id}", auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Category deleted.";
        else TempData["Error"] = result?.Message ?? "Delete category failed.";
        return RedirectToPage();
    }

    private void SetShell()
    {
        ViewData["AdminSection"] = "categories";
        var user = auth.GetCurrentUser(HttpContext);
        if (user == null) return;
        ViewData["UserName"] = user.Username;
        ViewData["UserEmail"] = user.Email;
    }

    private async Task EnrichNovelCountsAsync(string? token)
    {
        foreach (var category in Categories)
        {
            var result = await api.GetAsync<PagedData<NovelSummaryDto>>($"/api/novels?categoryId={category.Id}&page=1&size=1", token);
            if (result?.Success == true && result.Data != null)
            {
                category.NovelCount = result.Data.TotalElements;
            }
        }
    }

    private static string ToSlug(string value)
    {
        var normalized = value.ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
        var slug = System.Text.RegularExpressions.Regex.Replace(new string(chars).Normalize(System.Text.NormalizationForm.FormC), @"[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "category" : slug;
    }
}
