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
            Results = r?.Data?.Items ?? MockSearch(keyword);
            TotalPages = r?.Data?.TotalPages ?? 1;
            TotalElements = r?.Data?.TotalElements ?? Results.Count;
        }
    }
    private List<NovelSummaryDto> MockSearch(string kw) =>
    [
        new() { Title = $"{kw} - Kết quả 1", Slug = "result-1", Author = new() { Username = "author1" }, Status = "Ongoing", RatingAverage = 4.5, ViewCount = 12000, TotalChapters = 120 },
        new() { Title = $"{kw} - Kết quả 2", Slug = "result-2", Author = new() { Username = "author2" }, Status = "Ended",   RatingAverage = 4.2, ViewCount = 8500,  TotalChapters = 280 },
    ];
}
