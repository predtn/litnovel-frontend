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
        await LoadAsync(volumeId, novelId);
        Input.ChapterNumber = (Novel.Volumes.FirstOrDefault(v => v.Id == volumeId)?.Chapters.Count ?? 0) + 1;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int volumeId, int novelId, bool submit = false)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        if (string.IsNullOrWhiteSpace(Input.Title) || !HasText(Input.Content))
        {
            ModelState.AddModelError("", "Chapter title and content are required.");
            await LoadAsync(volumeId, novelId);
            return Page();
        }

        var result = await Api.PostAsync<ChapterNavDto>($"/api/volumes/{volumeId}/chapters", Input, Token);
        if (result?.Success == false)
        {
            ModelState.AddModelError("", result.Message ?? "Unable to create chapter.");
            await LoadAsync(volumeId, novelId);
            return Page();
        }

        if (submit && result?.Data?.Id is int id && id > 0)
        {
            await Api.PostAsync<object>($"/api/chapters/{id}/submit", null, Token);
        }

        TempData["Success"] = submit ? "Chapter saved and submitted." : "Chapter draft saved.";
        return RedirectToPage("/Publish/Chapters", new { volumeId, novelId });
    }

    private async Task LoadAsync(int volumeId, int novelId)
    {
        var result = await Api.GetAsync<NovelDetailDto>($"/api/novels/{novelId}", Token);
        Novel = result?.Data ?? MockNovel(novelId);
        Volume = Novel.Volumes.FirstOrDefault(v => v.Id == volumeId) ?? Novel.Volumes.First();
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
