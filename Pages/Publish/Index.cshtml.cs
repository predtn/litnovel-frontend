using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace litnovel_frontend.Pages.Publish;

public class IndexModel : PublishPageModel
{
    public List<NovelSummaryDto> Novels { get; set; } = [];
    public string? Status { get; set; }
    public string Sort { get; set; } = "updatedAt";
    public new int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalElements { get; set; }
    public int TotalChapters => Novels.Sum(n => n.TotalChapters);
    public int TotalViews => Novels.Sum(n => n.ViewCount);
    public double AverageRating => Novels.Where(n => n.RatingAverage > 0).DefaultIfEmpty().Average(n => n?.RatingAverage ?? 0);

    public IndexModel(IApiService api, IAuthService auth) : base(api, auth) { }

    public async Task<IActionResult> OnGetAsync(string? status, string sort = "updatedAt", int page = 1)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;

        Status = status;
        Sort = sort;
        Page = page;

        var endpoint = "/api/novels/my" + ODataQuery.Build(
            page: page,
            size: 20,
            orderBy: ODataQuery.OrderBy(sort),
            filters: [ODataQuery.Eq("Status", status)]);
        var result = await Api.GetAsync<PagedData<NovelSummaryDto>>(endpoint, Token);
        var data = result?.Data;
        Novels = data?.Items ?? [];
        TotalPages = data?.TotalPages ?? 1;
        TotalElements = data?.TotalElements ?? Novels.Count;
        return Page();
    }
}
