using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace litnovel_frontend.Pages.Publish;

public class ChaptersModel : PublishPageModel
{
    public NovelDetailDto Novel { get; set; } = new();
    public VolumeDto Volume { get; set; } = new();
    public List<ChapterNavDto> Chapters { get; set; } = [];

    public ChaptersModel(IApiService api, IAuthService auth) : base(api, auth) { }

    public async Task<IActionResult> OnGetAsync(int volumeId, int novelId)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        await LoadAsync(volumeId, novelId);
        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(int volumeId, int novelId, int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var result = await Api.PostAsync<object>($"/api/chapters/{id}/submit", null, Token);
        SetApiResultMessage(result, "Chapter submitted.", "Unable to submit chapter.");
        return RedirectToPage(new { volumeId, novelId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int volumeId, int novelId, int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var result = await Api.DeleteAsync<object>($"/api/chapters/{id}", Token);
        SetApiResultMessage(result, "Chapter deleted.", "Unable to delete chapter.");
        return RedirectToPage(new { volumeId, novelId });
    }

    private async Task LoadAsync(int volumeId, int novelId)
    {
        var novelTask = Api.GetAsync<NovelDetailDto>($"/api/novels/{novelId}", Token);
        var chapterTask = Api.GetAsync<PagedData<ChapterNavDto>>($"/api/volumes/{volumeId}/chapters" + ODataQuery.Build(size: 50, orderBy: "ChapterNumber asc"), Token);
        await Task.WhenAll(novelTask, chapterTask);
        Novel = novelTask.Result?.Data ?? MockNovel(novelId);
        Volume = Novel.Volumes.FirstOrDefault(v => v.Id == volumeId) ?? Novel.Volumes.First();
        if (chapterTask.Result?.Success == false)
        {
            TempData["Error"] = ApiFailureMessage(chapterTask.Result, "Unable to load chapters.");
        }

        Chapters = chapterTask.Result?.Data?.Items ?? Novel.Volumes.FirstOrDefault(v => v.Id == volumeId)?.Chapters ?? [];
        await FillMissingWordCountsAsync();
    }

    private async Task FillMissingWordCountsAsync()
    {
        var missing = Chapters.Where(c => c.WordCount <= 0).ToList();
        if (missing.Count == 0) return;

        var detailTasks = missing.ToDictionary(
            c => c.Id,
            c => Api.GetAsync<ChapterDetailDto>($"/api/chapters/{c.Id}", Token));

        await Task.WhenAll(detailTasks.Values);

        foreach (var chapter in missing)
        {
            var detail = detailTasks[chapter.Id].Result?.Data;
            if (detail == null && chapter.WordCount <= 0)
            {
                detail = MockChapter(chapter.Id);
            }

            chapter.WordCount = CountWords(detail?.Content);
        }
    }

    private static int CountWords(string? value)
    {
        var text = ToPlainText(value);
        return string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static string ToPlainText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        return Regex.Replace(value, "<.*?>", " ").Replace("&nbsp;", " ").Trim();
    }
}
