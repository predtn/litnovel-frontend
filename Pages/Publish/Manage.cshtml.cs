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
        Novel = result?.Data ?? MockNovel(id);
        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var result = await Api.PostAsync<object>($"/api/novels/{id}/submit", null, Token);
        TempData[result?.Success == false ? "Error" : "Success"] = result?.Message ?? "Novel submitted for review.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var result = await Api.DeleteAsync<object>($"/api/novels/{id}", Token);
        TempData[result?.Success == false ? "Error" : "Success"] = result?.Message ?? "Novel deleted.";
        return RedirectToPage("/Publish/Index");
    }
}
