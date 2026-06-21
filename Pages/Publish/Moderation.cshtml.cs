using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace litnovel_frontend.Pages.Publish;

public class ModerationModel : PublishPageModel
{
    public List<ModerationItemDto> Items { get; set; } = [];
    public string Tab { get; set; } = "novels";

    public ModerationModel(IApiService api, IAuthService auth) : base(api, auth) { }

    public async Task<IActionResult> OnGetAsync(string tab = "novels")
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        Tab = tab;

        if (Tab == "chapters")
        {
            Items = await LoadChapterModerationItemsAsync();
            return Page();
        }

        var result = await Api.GetAsync<PagedData<NovelSummaryDto>>(
            "/api/novels/my" + ODataQuery.Build(size: 50, filters: [ODataQuery.Eq("Status", "Pending")]),
            Token);
        Items = result?.Data?.Items.Select(n => new ModerationItemDto
        {
            Id = n.Id,
            Title = n.Title,
            Type = "Novel",
            Status = n.Status,
            SubmittedAt = n.UpdatedAt,
            ReviewerNotes = n.Status == "Rejected" ? "Please revise metadata before resubmitting." : null
        }).ToList() ?? MockItems();

        return Page();
    }

    private async Task<List<ModerationItemDto>> LoadChapterModerationItemsAsync()
    {
        var novelsResult = await Api.GetAsync<PagedData<NovelSummaryDto>>(
            "/api/novels/my" + ODataQuery.Build(size: 50, orderBy: "UpdatedAt desc"),
            Token);
        var novels = novelsResult?.Data?.Items;
        if (novels == null || novels.Count == 0)
        {
            return MockChapterItems();
        }

        var detailTasks = novels.Select(n => Api.GetAsync<NovelDetailDto>($"/api/novels/{n.Id}", Token)).ToList();
        await Task.WhenAll(detailTasks);

        var items = detailTasks
            .Select(t => t.Result?.Data)
            .Where(n => n != null)
            .SelectMany(n => n!.Volumes.SelectMany(v => v.Chapters.Select(c => new { Novel = n, Chapter = c })))
            .Where(x => x.Chapter.Status is "Pending" or "Rejected" or "Published")
            .Select(x => new ModerationItemDto
            {
                Id = x.Chapter.Id,
                Title = $"{x.Novel!.Title} - {x.Chapter.Title}",
                Type = "Chapter",
                Status = x.Chapter.Status,
                SubmittedAt = x.Chapter.UpdatedAt == default ? x.Chapter.CreatedAt : x.Chapter.UpdatedAt,
                ReviewerNotes = x.Chapter.Status == "Rejected" ? "Content needs clearer formatting." : null
            })
            .OrderByDescending(i => i.SubmittedAt)
            .ToList();

        return items.Count > 0 ? items : MockChapterItems();
    }

    private static List<ModerationItemDto> MockItems() =>
    [
        new() { Id = 43, Title = "Moonlit Archive", Type = "Novel", Status = "Pending", SubmittedAt = DateTime.UtcNow.AddDays(-1) },
        new() { Id = 45, Title = "Glass Crown Errata", Type = "Novel", Status = "Rejected", SubmittedAt = DateTime.UtcNow.AddDays(-5), ReviewerNotes = "Cover image URL is invalid." },
        new() { Id = 44, Title = "Neon Blade School", Type = "Novel", Status = "Published", SubmittedAt = DateTime.UtcNow.AddDays(-12) }
    ];

    private static List<ModerationItemDto> MockChapterItems()
    {
        var novel = MockNovel(42);
        return novel.Volumes.SelectMany(v => v.Chapters)
            .Where(c => c.Status is "Pending" or "Rejected" or "Published")
            .Select(c => new ModerationItemDto
            {
                Id = c.Id,
                Title = $"{novel.Title} - {c.Title}",
                Type = "Chapter",
                Status = c.Status,
                SubmittedAt = c.UpdatedAt == default ? c.CreatedAt : c.UpdatedAt,
                ReviewerNotes = c.Status == "Rejected" ? "Content needs clearer formatting." : null
            }).ToList();
    }
}
