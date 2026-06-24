using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Admin.NovelOverride;

public class IndexModel(IApiService api, IAuthService auth) : PageModel
{
    public NovelDetailDto? Novel { get; set; }
    public List<NovelSummaryDto> SearchResults { get; set; } = [];
    public List<UserDetailDto> AuthorCandidates { get; set; } = [];
    public string? Query { get; set; }
    public string? LoadError { get; set; }
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync(string? q, int? novelId, [FromQuery(Name = "page")] int pageNumber = 1)
    {
        if (!auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        SetShell();
        Query = q;
        Page = Math.Max(1, pageNumber);
        var token = auth.GetToken(HttpContext);
        await LoadAuthorCandidatesAsync(token);
        await LoadNovelListAsync(token);

        if (novelId.HasValue)
        {
            await LoadNovelAsync(novelId.Value, token);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostStatusAsync(int id, string status, string reason)
    {
        var result = await api.PutAsync<object>($"/api/admin/novels/{id}/status", new { status, reason }, auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Đã cập nhật trạng thái novel.";
        else TempData["Error"] = result?.Message ?? "Không thể cập nhật trạng thái novel.";
        return RedirectToPage(new { novelId = id });
    }

    public async Task<IActionResult> OnPostAuthorAsync(int id, int authorId, string reason)
    {
        var result = await api.PutAsync<object>($"/api/admin/novels/{id}/author", new { authorId, reason }, auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Đã cập nhật tác giả novel.";
        else TempData["Error"] = result?.Message ?? "Không thể cập nhật tác giả novel.";
        return RedirectToPage(new { novelId = id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await api.DeleteAsync<object>($"/api/novels/{id}", auth.GetToken(HttpContext));
        if (result?.Success == true) TempData["Success"] = "Novel deleted.";
        else TempData["Error"] = result?.Message ?? "Could not delete novel.";
        return RedirectToPage();
    }

    private async Task LoadNovelAsync(int id, string? token)
    {
        var result = await api.GetAsync<NovelDetailDto>($"/api/novels/{id}", token);
        if (result?.Success == true) Novel = result.Data;
        else LoadError = result?.Message ?? "Không thể tải novel.";
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

    private async Task LoadAuthorCandidatesAsync(string? token)
    {
        var users = await api.GetAsync<PagedData<UserDetailDto>>("/api/admin/users?role=User&page=1&size=200", token);
        if (users?.Success == true && users.Data != null)
        {
            AuthorCandidates = users.Data.Items
                .Where(user => !user.Status.Equals("Banned", StringComparison.OrdinalIgnoreCase))
                .OrderBy(user => user.Username)
                .ToList();
        }
    }

    private void SetShell()
    {
        ViewData["AdminSection"] = "novel-override";
        var user = auth.GetCurrentUser(HttpContext);
        if (user == null) return;
        ViewData["UserName"] = user.Username;
        ViewData["UserEmail"] = user.Email;
    }

}
