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

    public bool CanSubmitChapter(string? status) => CanSubmitForReview(status);
    public bool CanEditChapter(string? status) => CanEditChapterStatus(status);
    public bool CanWithdrawChapter(string? status) => IsPendingReview(status);
    public bool CanDeleteChapter(string? status)
        => string.Equals(status, "Draft", StringComparison.OrdinalIgnoreCase) || IsApprovedChapterStatus(status);

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

        var chapterResult = await Api.GetAsync<ChapterDetailDto>($"/api/chapters/{id}", Token);
        if (IsApiSuccess(chapterResult) && chapterResult?.Data != null && !CanSubmitForReview(chapterResult.Data.Status))
        {
            TempData["Error"] = "Chương chỉ có thể gửi duyệt khi đang là bản nháp hoặc cần chỉnh sửa.";
            return RedirectToPage(new { volumeId, novelId });
        }

        var result = await Api.PostAsync<object>($"/api/chapters/{id}/submit", null, Token);
        SetApiResultMessage(result, "Chương đã được gửi duyệt.", "Chưa thể gửi duyệt chương lúc này.");
        return RedirectToPage(new { volumeId, novelId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int volumeId, int novelId, int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;

        var chapterResult = await Api.GetAsync<ChapterDetailDto>($"/api/chapters/{id}", Token);
        if (!IsApiSuccess(chapterResult) || chapterResult?.Data == null)
        {
            TempData["Error"] = ApiFailureMessage(chapterResult, "Không thể tải thông tin chương.");
            return RedirectToPage(new { volumeId, novelId });
        }

        if (!CanDeleteChapter(chapterResult.Data.Status))
        {
            TempData["Error"] = "Chỉ có thể xóa chương đang là bản nháp hoặc đã được duyệt.";
            return RedirectToPage(new { volumeId, novelId });
        }

        var result = await Api.DeleteAsync<object>($"/api/chapters/{id}", Token);
        SetApiResultMessage(result, "Đã xóa chương.", "Chưa thể xóa chương lúc này.");
        return RedirectToPage(new { volumeId, novelId });
    }

    public async Task<IActionResult> OnPostWithdrawAsync(int volumeId, int novelId, int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;

        var chapterResult = await Api.GetAsync<ChapterDetailDto>($"/api/chapters/{id}", Token);
        if (IsApiSuccess(chapterResult) && chapterResult?.Data != null && !IsPendingReview(chapterResult.Data.Status))
        {
            TempData["Error"] = "Chỉ có thể hủy gửi duyệt khi chương đang chờ duyệt.";
            return RedirectToPage(new { volumeId, novelId });
        }

        var result = await Api.PostAsync<ChapterNavDto>($"/api/chapters/{id}/withdraw", null, Token);
        if (!IsApiSuccess(result))
        {
            TempData["Error"] = ApiFailureMessage(result, "Chưa thể hủy gửi duyệt chương lúc này.");
            return RedirectToPage(new { volumeId, novelId });
        }

        var verifyResult = await Api.GetAsync<ChapterDetailDto>($"/api/chapters/{id}", Token);
        if (!string.Equals(verifyResult?.Data?.Status, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Hệ thống chưa cập nhật chương về bản nháp. Vui lòng thử lại sau.";
            return RedirectToPage(new { volumeId, novelId });
        }

        TempData["Success"] = "Đã hủy gửi duyệt. Chương đã trở lại bản nháp.";
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
