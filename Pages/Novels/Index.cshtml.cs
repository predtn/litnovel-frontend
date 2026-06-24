using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Novels;

public class IndexModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public List<NovelSummaryDto> Novels { get; set; } = [];
    public List<CategoryDto> Categories { get; set; } = [];
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalElements { get; set; }
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public int CategoryId { get; set; }
    public string Sort { get; set; } = "updatedAt";

    public IndexModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    public async Task OnGetAsync(string? keyword, string? status, int categoryId = 0, int page = 1, string sort = "updatedAt")
    {
        Keyword = keyword;
        Status = status;
        CategoryId = categoryId;
        Page = page;
        Sort = sort;

        var token = _auth.GetToken(HttpContext);
        var qs = $"/api/novels?page={page}&size=20&sort={sort}&order=desc"
            + (string.IsNullOrEmpty(keyword) ? "" : $"&keyword={Uri.EscapeDataString(keyword)}")
            + (string.IsNullOrEmpty(status)  ? "" : $"&status={status}")
            + (categoryId > 0                ? $"&categoryId={categoryId}" : "");

        var novelTask = _api.GetAsync<PagedData<NovelSummaryDto>>(qs, token);
        var catTask   = _api.GetAsync<List<CategoryDto>>("/api/categories");
        await Task.WhenAll(novelTask, catTask);

        var data = novelTask.Result?.Data;
        Novels        = data?.Items ?? [];
        TotalPages    = data?.TotalPages ?? 1;
        TotalElements = data?.TotalElements ?? Novels.Count;
        Categories    = catTask.Result?.Data ?? [];
    }
}
