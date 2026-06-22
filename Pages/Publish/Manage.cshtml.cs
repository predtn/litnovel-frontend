using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace litnovel_frontend.Pages.Publish;

public class ManageModel : PublishPageModel
{
    public NovelDetailDto Novel { get; set; } = new();

    public ManageModel(IApiService api, IAuthService auth) : base(api, auth) { }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var result = await Api.GetAsync<NovelDetailDto>($"/api/novels/{id}", Token);
        if (!IsApiSuccess(result) || result?.Data == null)
        {
            TempData["Error"] = ApiFailureMessage(result, "Unable to load novel.");
            return RedirectToPage("/Publish/Index");
        }

        Novel = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var result = await Api.PostAsync<object>($"/api/novels/{id}/submit", null, Token);
        SetApiResultMessage(result, "Novel submitted for review.", "Unable to submit novel.");
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var result = await Api.DeleteAsync<object>($"/api/novels/{id}", Token);
        SetApiResultMessage(result, "Novel deleted.", "Unable to delete novel.");
        return RedirectToPage("/Publish/Index");
    }
}
