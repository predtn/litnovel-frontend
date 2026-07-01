using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace litnovel_frontend.Pages.Publish;

public class ChapterPreviewModel : PublishPageModel
{
    public ChapterDetailDto Chapter { get; set; } = new();
    public int VolumeId { get; set; }
    public int NovelId { get; set; }
    public bool CanEditChapter => CanEditChapterStatus(Chapter.Status);
    public bool CanRestoreChapter => IsPendingDeletion(Chapter.Status);
    public string EditUnavailableMessage => IsPendingReview(Chapter.Status)
        ? "Chương đang chờ duyệt, không thể chỉnh sửa."
        : "Chương đang bị khóa nên không thể chỉnh sửa.";

    public ChapterPreviewModel(IApiService api, IAuthService auth) : base(api, auth) { }

    public async Task<IActionResult> OnGetAsync(int id, int volumeId, int novelId)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;

        VolumeId = volumeId;
        NovelId = novelId;

        var result = await Api.GetAsync<ChapterDetailDto>($"/api/chapters/{id}", Token);
        if (!IsApiSuccess(result))
        {
            TempData["Error"] = ApiFailureMessage(result, "Không thể tải bản xem trước chương.");
            return RedirectToPage("/Publish/Chapters", new { volumeId, novelId });
        }

        Chapter = result?.Data ?? new();
        if (Chapter.Novel == null || Chapter.Volume == null)
        {
            var novelResult = await Api.GetAsync<NovelDetailDto>($"/api/novels/{novelId}", Token);
            var novel = novelResult?.Data;
            Chapter.Novel ??= novel;
            Chapter.Volume ??= novel?.Volumes.FirstOrDefault(v => v.Id == volumeId);
        }
        return Page();
    }

    public async Task<IActionResult> OnPostRestoreAsync(int id, int volumeId, int novelId)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;

        var chapterResult = await Api.GetAsync<ChapterDetailDto>($"/api/chapters/{id}", Token);
        if (!IsApiSuccess(chapterResult) || chapterResult?.Data == null)
        {
            TempData["Error"] = ApiFailureMessage(chapterResult, "Không thể tải thông tin chương.");
            return RedirectToPage("/Publish/Chapters", new { volumeId, novelId, status = "PendingDeletion" });
        }

        if (!IsPendingDeletion(chapterResult.Data.Status))
        {
            TempData["Error"] = "Chỉ có thể khôi phục chương đang chờ xóa.";
            return RedirectToPage(new { id, volumeId, novelId });
        }

        var result = await Api.PostAsync<ChapterNavDto>($"/api/chapters/{id}/restore", null, Token);
        SetApiResultMessage(result, "Đã khôi phục chương.", "Chưa thể khôi phục chương lúc này.");
        return RedirectToPage(new { id, volumeId, novelId });
    }
}
