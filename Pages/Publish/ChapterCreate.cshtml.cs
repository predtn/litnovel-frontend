using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace litnovel_frontend.Pages.Publish;

public class ChapterCreateModel : PublishPageModel
{
    [BindProperty] public ChapterUpsertRequest Input { get; set; } = new();
    public NovelDetailDto Novel { get; set; } = new();
    public VolumeDto Volume { get; set; } = new();
    public int WordCount => CountWords(Input.Content);

    public ChapterCreateModel(IApiService api, IAuthService auth) : base(api, auth) { }

    public async Task<IActionResult> OnGetAsync(int volumeId, int novelId)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var loaded = await LoadAsync(volumeId, novelId);
        if (!loaded) return RedirectToPage("/Publish/Manage", new { id = novelId });
        Input.ChapterNumber = (Novel.Volumes.FirstOrDefault(v => v.Id == volumeId)?.Chapters.Count ?? 0) + 1;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int volumeId, int novelId, bool submit = false)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        Input.Status = "Draft";
        if (string.IsNullOrWhiteSpace(Input.Title) || !HasText(Input.Content))
        {
            ModelState.AddModelError("", "Cần nhập tiêu đề và nội dung chương.");
            await LoadAsync(volumeId, novelId);
            return Page();
        }

        var result = await Api.PostAsync<ChapterNavDto>($"/api/volumes/{volumeId}/chapters", Input, Token);
        if (!IsApiSuccess(result))
        {
            ModelState.AddModelError("", ApiFailureMessage(result, "Không thể tạo chương."));
            await LoadAsync(volumeId, novelId);
            return Page();
        }

        if (!submit && result?.Data?.Id is int draftId && draftId > 0 && !IsDraft(result.Data.Status))
        {
            var draftResult = await Api.PutAsync<ChapterNavDto>($"/api/chapters/{draftId}", Input, Token);
            if (!IsApiSuccess(draftResult))
            {
                ModelState.AddModelError("", ApiFailureMessage(draftResult, "Chương đã được tạo nhưng không thể giữ ở bản nháp."));
                await LoadAsync(volumeId, novelId);
                return Page();
            }
        }

        if (submit && result?.Data?.Id is int id && id > 0)
        {
            var submitResult = await Api.PostAsync<object>($"/api/chapters/{id}/submit", null, Token);
            if (!IsApiSuccess(submitResult))
            {
                ModelState.AddModelError("", ApiFailureMessage(submitResult, "Chương đã được lưu bản nháp nhưng không thể gửi duyệt."));
                await LoadAsync(volumeId, novelId);
                return Page();
            }
        }

        TempData["Success"] = submit ? "Chương đã được lưu và gửi duyệt." : "Đã lưu bản nháp chương.";
        return RedirectToManageVolumes(novelId);
    }

    private IActionResult RedirectToManageVolumes(int novelId) => RedirectToPage("/Publish/Manage", pageHandler: null, routeValues: new { id = novelId }, fragment: "volumes");

    private static bool IsDraft(string? status)
        => string.Equals(status, "Draft", StringComparison.OrdinalIgnoreCase);

    private async Task<bool> LoadAsync(int volumeId, int novelId)
    {
        var result = await Api.GetAsync<NovelDetailDto>($"/api/novels/{novelId}", Token);
        if (!IsApiSuccess(result) || result?.Data == null)
        {
            TempData["Error"] = ApiFailureMessage(result, "Không thể tải truyện.");
            return false;
        }

        Novel = result.Data;
        var volume = Novel.Volumes.FirstOrDefault(v => v.Id == volumeId);
        if (volume == null)
        {
            TempData["Error"] = "Không thể tải tập.";
            return false;
        }

        Volume = volume;
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
