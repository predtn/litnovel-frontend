using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.ChapterOverride;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public NovelDetailDto? Novel { get; set; }
    public ChapterDetailDto? Chapter { get; set; }
    public List<NovelSummaryDto> SearchResults { get; set; } = [];
    public string? Query { get; set; }
    public string? LoadError { get; set; }
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync(string? q, int? novelId, int? chapterId, int page = 1)
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();
        Query = q;
        Page = Math.Max(1, page);
        var token = auth.GetToken(HttpContext);
        await LoadNovelListAsync(token);

        if (novelId.HasValue)
        {
            var novelResult = await api.GetAsync<NovelDetailDto>($"/api/novels/{novelId.Value}", token);
            if (novelResult?.Success == true) Novel = novelResult.Data;
            else LoadError = novelResult?.Message ?? "Không thể tải novel.";
        }
        if (chapterId.HasValue)
        {
            var result = await api.GetAsync<ChapterDetailDto>($"/api/chapters/{chapterId.Value}", token);
            if (result?.Success == true) Chapter = result.Data;
            else LoadError = result?.Message ?? "Không thể tải chapter.";
        }
        return Page();
    }

    private async Task LoadNovelListAsync(string? token)
    {
        var endpoint = $"/api/novels?page={Page}&size=12";
        if (!string.IsNullOrWhiteSpace(Query))
        {
            endpoint += $"&keyword={Uri.EscapeDataString(Query)}";
        }

        var list = await api.GetAsync<PagedData<NovelSummaryDto>>(endpoint, token);
        if (list?.Success == true && list.Data != null)
        {
            SearchResults = list.Data.Items;
            TotalPages = Math.Max(1, list.Data.TotalPages);
        }
        else
        {
            LoadError = list?.Message ?? "Không tải được danh sách novel.";
        }
    }

    public async Task<IActionResult> OnPostSaveAsync(int id, int? novelId, string title, string content, string status)
    {
        await api.PutAsync<object>($"/api/chapters/{id}", new { title, content }, auth.GetToken(HttpContext));
        await api.PutAsync<object>($"/api/admin/chapters/{id}/status", new { status }, auth.GetToken(HttpContext));
        TempData["Success"] = "Chapter override saved.";
        return RedirectToPage(new { chapterId = id, novelId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await api.DeleteAsync<object>($"/api/chapters/{id}", auth.GetToken(HttpContext));
        TempData["Success"] = "Chapter deleted.";
        return RedirectToPage();
    }

    private void SetShell()
    {
        ViewData["AdminSection"] = "chapter-override";
        var user = auth.GetCurrentUser(HttpContext);
        if (user == null) return;
        ViewData["UserName"] = user.Username;
        ViewData["UserEmail"] = user.Email;
    }

}
