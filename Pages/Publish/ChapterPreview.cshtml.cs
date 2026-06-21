using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace litnovel_frontend.Pages.Publish;

public class ChapterPreviewModel : PublishPageModel
{
    public ChapterDetailDto Chapter { get; set; } = new();
    public int VolumeId { get; set; }
    public int NovelId { get; set; }

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
            TempData["Error"] = ApiFailureMessage(result, "Unable to load chapter preview.");
            return RedirectToPage("/Publish/Chapters", new { volumeId, novelId });
        }

        Chapter = result?.Data ?? new();
        Chapter.Novel ??= MockNovel(novelId);
        Chapter.Volume ??= MockNovel(novelId).Volumes.FirstOrDefault(v => v.Id == volumeId);
        return Page();
    }
}
