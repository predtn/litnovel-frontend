using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Novels;
public class SearchModel : PageModel
{
    private readonly IApiService _api;
    public List<NovelSummaryDto> Results { get; set; } = [];
    public string? Keyword { get; set; }
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalElements { get; set; }
    public SearchModel(IApiService api) { _api = api; }
    public async Task OnGetAsync(string? keyword, int page = 1)
    {
        Keyword = keyword; Page = page;
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var r = await _api.GetAsync<PagedData<NovelSummaryDto>>($"/api/novels?keyword={Uri.EscapeDataString(keyword)}&page={page}&size=15&sort=viewCount&order=desc");
            Results = r?.Data?.Items ?? [];
            TotalPages = r?.Data?.TotalPages ?? 1;
            TotalElements = r?.Data?.TotalElements ?? Results.Count;
        }
    }
}
