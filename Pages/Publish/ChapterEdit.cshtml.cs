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
    public bool CanEditChapter => CanEditSubmittedContent(Chapter.Status);

    public ChapterEditModel(IApiService api, IAuthService auth) : base(api, auth) { }

    public async Task<IActionResult> OnGetAsync(int id, int volumeId, int novelId)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var loaded = await LoadAsync(id, volumeId, novelId);
        if (!loaded) return RedirectToPage("/Publish/Chapters", new { volumeId, novelId });
        if (!CanEditChapter)
        {
            TempData["Error"] = "Chương đã được duyệt hoặc đang chờ duyệt nên không thể chỉnh sửa trực tiếp.";
            return RedirectToPage("/Publish/Chapters", new { volumeId, novelId });
        }

        Input = new()
        {
            ChapterNumber = Chapter.ChapterNumber,
            Title = Chapter.Title,
            Content = Chapter.Content,
            ReleaseDate = Chapter.ReleaseDate
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
            TempData["Error"] = "Chương đã được duyệt hoặc đang chờ duyệt nên không thể chỉnh sửa trực tiếp.";
            return RedirectToPage("/Publish/Chapters", new { volumeId, novelId });
        }

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

        TempData["Success"] = "Đã cập nhật chương.";
        return RedirectToPage("/Publish/Chapters", new { volumeId, novelId });
    }

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
            : text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static bool HasText(string? value) => !string.IsNullOrWhiteSpace(ToPlainText(value));

    private static string ToPlainText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        return Regex.Replace(value, "<.*?>", " ").Replace("&nbsp;", " ").Trim();
    }
}
