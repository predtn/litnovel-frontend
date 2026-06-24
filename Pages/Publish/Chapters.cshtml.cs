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
        var loaded = await LoadAsync(volumeId, novelId);
        if (!loaded) return RedirectToPage("/Publish/Manage", new { id = novelId });
        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(int volumeId, int novelId, int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var result = await Api.PostAsync<object>($"/api/chapters/{id}/submit", null, Token);
        SetApiResultMessage(result, "Chương đã được gửi duyệt.", "Không thể gửi duyệt chương.");
        return RedirectToPage(new { volumeId, novelId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int volumeId, int novelId, int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var result = await Api.DeleteAsync<object>($"/api/chapters/{id}", Token);
        SetApiResultMessage(result, "Đã xóa chương.", "Không thể xóa chương.");
        return RedirectToPage(new { volumeId, novelId });
    }

    private async Task<bool> LoadAsync(int volumeId, int novelId)
    {
        var novelTask = Api.GetAsync<NovelDetailDto>($"/api/novels/{novelId}", Token);
        var chapterTask = Api.GetAsync<PagedData<ChapterNavDto>>($"/api/volumes/{volumeId}/chapters" + ODataQuery.Build(size: 50, orderBy: "ChapterNumber asc"), Token);
        await Task.WhenAll(novelTask, chapterTask);
        if (!IsApiSuccess(novelTask.Result) || novelTask.Result?.Data == null)
        {
            TempData["Error"] = ApiFailureMessage(novelTask.Result, "Không thể tải truyện.");
            return false;
        }

        Novel = novelTask.Result.Data;
        var volume = Novel.Volumes.FirstOrDefault(v => v.Id == volumeId);
        if (volume == null)
        {
            TempData["Error"] = "Không thể tải tập.";
            return false;
        }

        Volume = volume;
        if (chapterTask.Result?.Success == false)
        {
            TempData["Error"] = ApiFailureMessage(chapterTask.Result, "Không thể tải danh sách chương.");
        }

        Chapters = chapterTask.Result?.Data?.Items ?? Novel.Volumes.FirstOrDefault(v => v.Id == volumeId)?.Chapters ?? [];
        await FillMissingWordCountsAsync();
        return true;
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
