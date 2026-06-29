using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace litnovel_frontend.Pages.Publish;

public class ChapterEditModel : PublishPageModel
{
    [BindProperty] public ChapterUpsertRequest Input { get; set; } = new();
    public ChapterDetailDto Chapter { get; set; } = new();
    public int VolumeId { get; set; }
    public int NovelId { get; set; }
    public int WordCount => CountWords(Input.Content);
    public bool CanEditChapter => CanEditChapterStatus(Chapter.Status);
    public bool WillSubmitForReviewAfterSave => true;
    public string SaveButtonText => "Lưu và gửi duyệt lại";

    public ChapterEditModel(IApiService api, IAuthService auth) : base(api, auth) { }

    public async Task<IActionResult> OnGetAsync(int id, int volumeId, int novelId)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var loaded = await LoadAsync(id, volumeId, novelId);
        if (!loaded) return RedirectToPage("/Publish/Chapters", new { volumeId, novelId });
        if (!CanEditChapter)
        {
            TempData["Error"] = EditBlockedMessage();
            return RedirectToPage("/Publish/Chapters", new { volumeId, novelId });
        }

        Input = new()
        {
            ChapterNumber = Chapter.ChapterNumber,
            Title = Chapter.Title,
            Content = Chapter.Content,
            ReleaseDate = Chapter.ReleaseDate,
            Status = Chapter.Status
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, int volumeId, int novelId)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var loaded = await LoadAsync(id, volumeId, novelId);
        if (!loaded) return RedirectToPage("/Publish/Chapters", new { volumeId, novelId });
        if (!CanEditChapter)
        {
            TempData["Error"] = EditBlockedMessage();
            return RedirectToPage("/Publish/Chapters", new { volumeId, novelId });
        }

        Input.Status = "Pending";
        if (string.IsNullOrWhiteSpace(Input.Title) || !HasText(Input.Content))
        {
            ModelState.AddModelError("", "Cần nhập tiêu đề và nội dung chương.");
            await LoadAsync(id, volumeId, novelId);
            return Page();
        }

        var result = await Api.PutAsync<ChapterNavDto>($"/api/chapters/{id}", Input, Token);
        if (!IsApiSuccess(result))
        {
            ModelState.AddModelError("", ApiFailureMessage(result, "Không thể cập nhật chương."));
            await LoadAsync(id, volumeId, novelId);
            return Page();
        }

        var submitResult = await Api.PostAsync<object>($"/api/chapters/{id}/submit", null, Token);
        if (!IsApiSuccess(submitResult))
        {
            var verifyResult = await Api.GetAsync<ChapterDetailDto>($"/api/chapters/{id}", Token);
            if (!IsPendingReview(verifyResult?.Data?.Status))
            {
                ModelState.AddModelError("", ApiFailureMessage(submitResult, "Đã lưu thay đổi nhưng chưa thể gửi duyệt chương."));
                await LoadAsync(id, volumeId, novelId);
                return Page();
            }
        }

        TempData["Success"] = "Đã lưu thay đổi và gửi chương vào hàng chờ duyệt.";
        return RedirectToPage("/Publish/Chapters", new { volumeId, novelId });
    }

    private string EditBlockedMessage()
        => IsPendingReview(Chapter.Status)
            ? "Chương đang chờ duyệt, không thể chỉnh sửa."
            : "Chương đang bị khóa nên không thể chỉnh sửa.";

    private async Task<bool> LoadAsync(int id, int volumeId, int novelId)
    {
        VolumeId = volumeId;
        NovelId = novelId;
        var result = await Api.GetAsync<ChapterDetailDto>($"/api/chapters/{id}", Token);
        if (!IsApiSuccess(result) || result?.Data == null)
        {
            TempData["Error"] = ApiFailureMessage(result, "Không thể tải chương.");
            return false;
        }

        Chapter = result.Data;
        if (Chapter.Novel == null || Chapter.Volume == null)
        {
            var novelResult = await Api.GetAsync<NovelDetailDto>($"/api/novels/{novelId}", Token);
            var novel = novelResult?.Data;
            Chapter.Novel ??= novel;
            Chapter.Volume ??= novel?.Volumes.FirstOrDefault(v => v.Id == volumeId);
        }

        return true;
    }

    private static int CountWords(string? value)
    {
        var text = ToPlainText(value);
        return string.IsNullOrWhiteSpace(text)
            ? 0
            : Regex.Matches(text, @"[\p{L}\p{N}]+(?:['’.-][\p{L}\p{N}]+)*").Count;
    }

    private static bool HasText(string? value) => !string.IsNullOrWhiteSpace(ToPlainText(value));

    private static string ToPlainText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        return Regex.Replace(value, "<.*?>", " ")
            .Replace("&nbsp;", " ")
            .Replace('\u00a0', ' ')
            .Trim();
    }
}
