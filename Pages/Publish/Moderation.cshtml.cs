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
            ReviewerNotes = n.Status == "Rejected" ? "Vui lòng chỉnh sửa thông tin trước khi gửi lại." : null
        }).ToList() ?? [];

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
            return [];
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
                ReviewerNotes = x.Chapter.Status == "Rejected" ? "Nội dung cần định dạng rõ ràng hơn." : null
            })
            .OrderByDescending(i => i.SubmittedAt)
            .ToList();

        return items;
    }
}
