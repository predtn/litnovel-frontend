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
    public List<TagDto> Tags { get; set; } = [];
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalElements { get; set; }
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public int CategoryId { get; set; }
    public List<int> SelectedTagIds { get; set; } = [];
    public string Sort { get; set; } = "updatedAt";
    public HashSet<int> FavoriteIds { get; set; } = [];

    public IndexModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    public async Task OnGetAsync(string? keyword, string? status, int categoryId = 0, [FromQuery] List<int>? tagId = null, int page = 1, string sort = "updatedAt")
    {
        Keyword = keyword;
        Status = status;
        CategoryId = categoryId;
        SelectedTagIds = tagId?.Where(id => id > 0).Distinct().ToList() ?? [];
        Page = page;
        Sort = sort;

        var token = _auth.GetToken(HttpContext);
        var qs = $"/api/novels?page={page}&size=20&sort={sort}&order=desc"
            + (string.IsNullOrEmpty(keyword) ? "" : $"&keyword={Uri.EscapeDataString(keyword)}")
            + (string.IsNullOrEmpty(status)  ? "" : $"&status={status}")
            + (categoryId > 0                ? $"&categoryId={categoryId}" : "")
            + string.Concat(SelectedTagIds.Select(id => $"&tagId={id}"));

        var novelTask = _api.GetAsync<PagedData<NovelSummaryDto>>(qs, token);
        var catTask   = _api.GetAsync<List<CategoryDto>>("/api/categories");
        var tagTask   = _api.GetAsync<List<TagDto>>("/api/tags");

        // Fetch danh sách yêu thích server-side nếu người dùng đã đăng nhập
        Task<ApiResponse<PagedData<NovelSummaryDto>>?>? favTask = null;
        if (!string.IsNullOrWhiteSpace(token))
        {
            favTask = _api.GetAsync<PagedData<NovelSummaryDto>>("/api/users/me/favorites?page=1&size=200", token);
        }

        await Task.WhenAll(
            novelTask, catTask, tagTask,
            favTask ?? Task.FromResult<ApiResponse<PagedData<NovelSummaryDto>>?>(null));

        var data = novelTask.Result?.Data;
        Novels        = data?.Items ?? [];
        TotalPages    = data?.TotalPages ?? 1;
        TotalElements = data?.TotalElements ?? Novels.Count;
        Categories    = catTask.Result?.Data ?? [];
        Tags          = tagTask.Result?.Data ?? [];

        if (favTask != null)
        {
            FavoriteIds = favTask.Result?.Data?.Items
                .Select(f => f.Id)
                .ToHashSet() ?? [];
        }
    }
}

