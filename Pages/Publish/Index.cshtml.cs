using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace litnovel_frontend.Pages.Publish;

public class IndexModel : PublishPageModel
{
    public List<NovelSummaryDto> Novels { get; set; } = [];
    public List<NovelSummaryDto> SummaryNovels { get; set; } = [];
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public string Sort { get; set; } = "updatedAt";
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalElements { get; set; }
    public int SummaryTotalElements { get; set; }
    public int TotalChapters => SummaryNovels.Sum(n => n.TotalChapters);
    public int TotalViews => SummaryNovels.Sum(n => n.ViewCount);
    public double AverageRating => SummaryNovels.Where(n => n.RatingAverage > 0).DefaultIfEmpty().Average(n => n?.RatingAverage ?? 0);

    public IndexModel(IApiService api, IAuthService auth) : base(api, auth) { }

    public async Task<IActionResult> OnGetAsync(string? status, string? keyword, string sort = "updatedAt", int page = 1)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;

        Keyword = keyword;
        Status = status;
        Sort = sort;
        Page = page;

        await LoadSummaryAsync();

        var endpoint = "/api/novels/my" + ODataQuery.Build(
            page: page,
            size: 20,
            orderBy: ODataQuery.OrderBy(sort),
            filters:
            [
                NovelStatusFilter(status),
                ODataQuery.ContainsAny(keyword, "Title")
            ]);
        var result = await Api.GetAsync<PagedData<NovelSummaryDto>>(endpoint, Token);
        var data = result?.Data;
        Novels = data?.Items ?? [];
        TotalPages = data?.TotalPages ?? 1;
        TotalElements = data?.TotalElements ?? Novels.Count;
        return Page();
    }

    private async Task LoadSummaryAsync()
    {
        const int summaryPageSize = 100;
        var page = 1;
        var totalPages = 1;

        SummaryNovels = [];
        SummaryTotalElements = 0;

        do
        {
            var endpoint = "/api/novels/my" + ODataQuery.Build(
                page: page,
                size: summaryPageSize,
                orderBy: ODataQuery.OrderBy("updatedAt"),
                filters: [NovelStatusFilter(null)]);
            var result = await Api.GetAsync<PagedData<NovelSummaryDto>>(endpoint, Token);
            var data = result?.Data;
            if (data == null) break;

            SummaryNovels.AddRange(data.Items);
            SummaryTotalElements = data.TotalElements;
            totalPages = Math.Max(1, data.TotalPages);
            page++;
        }
        while (page <= totalPages);

        if (SummaryTotalElements <= 0)
        {
            SummaryTotalElements = SummaryNovels.Count;
        }
    }

    private static string NovelStatusFilter(string? status)
    {
        return string.IsNullOrWhiteSpace(status)
            ? ODataQuery.Ne("Status", "PendingDeletion")
            : ODataQuery.Eq("Status", status);
    }
}
