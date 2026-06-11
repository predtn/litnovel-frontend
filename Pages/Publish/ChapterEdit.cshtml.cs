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

    public ChapterEditModel(IApiService api, IAuthService auth) : base(api, auth) { }

    public async Task<IActionResult> OnGetAsync(int id, int volumeId, int novelId)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        await LoadAsync(id, volumeId, novelId);
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
        if (string.IsNullOrWhiteSpace(Input.Title) || !HasText(Input.Content))
        {
            ModelState.AddModelError("", "Chapter title and content are required.");
            await LoadAsync(id, volumeId, novelId);
            return Page();
        }

        var result = await Api.PutAsync<ChapterNavDto>($"/api/chapters/{id}", Input, Token);
        if (!IsApiSuccess(result))
        {
            ModelState.AddModelError("", ApiFailureMessage(result, "Unable to update chapter."));
            await LoadAsync(id, volumeId, novelId);
            return Page();
        }

        TempData["Success"] = "Chapter updated.";
        return RedirectToPage("/Publish/Chapters", new { volumeId, novelId });
    }

    private async Task LoadAsync(int id, int volumeId, int novelId)
    {
        VolumeId = volumeId;
        NovelId = novelId;
        var result = await Api.GetAsync<ChapterDetailDto>($"/api/chapters/{id}", Token);
        Chapter = result?.Data ?? MockChapter(id);
        Chapter.Novel ??= MockNovel(novelId);
        Chapter.Volume ??= MockNovel(novelId).Volumes.FirstOrDefault(v => v.Id == volumeId);
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
