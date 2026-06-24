using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace litnovel_frontend.Pages.Publish;

public class StatisticsModel : PublishPageModel
{
    public NovelDetailDto Novel { get; set; } = new();
    public NovelAnalyticsDto Analytics { get; set; } = new();

    public StatisticsModel(IApiService api, IAuthService auth) : base(api, auth) { }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var novelTask = Api.GetAsync<NovelDetailDto>($"/api/novels/{id}", Token);
        var analyticsTask = Api.GetAsync<NovelAnalyticsDto>($"/api/novels/{id}/analytics", Token);
        await Task.WhenAll(novelTask, analyticsTask);
        if (!IsApiSuccess(novelTask.Result) || novelTask.Result?.Data == null)
        {
            TempData["Error"] = ApiFailureMessage(novelTask.Result, "Unable to load novel.");
            return RedirectToPage("/Publish/Index");
        }

        Novel = novelTask.Result.Data;
        Analytics = analyticsTask.Result?.Data ?? new() { NovelId = id };
        return Page();
    }
}
