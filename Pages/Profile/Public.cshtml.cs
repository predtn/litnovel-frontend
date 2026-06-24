using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Profile;

public class PublicModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public new UserDetailDto? User { get; set; }
    public List<NovelSummaryDto> Novels { get; set; } = [];
    public bool IsOwnProfile { get; set; }

    [BindProperty] public string ReportType { get; set; } = "Harassment";
    [BindProperty] public string? Description { get; set; }

    public PublicModel(IApiService api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task OnGetAsync(int userId)
    {
        IsOwnProfile = _auth.GetCurrentUser(HttpContext)?.Id == userId;
        var profileTask = _api.GetAsync<UserDetailDto>($"/api/users/{userId}");
        var novelsTask = _api.GetAsync<PagedData<NovelSummaryDto>>($"/api/novels?authorId={userId}&page=1&size=12");
        await Task.WhenAll(profileTask, novelsTask);

        User = profileTask.Result?.Data;
        Novels = novelsTask.Result?.Data?.Items ?? [];
    }

    public async Task<IActionResult> OnPostReportAsync(int userId)
    {
        if (_auth.GetCurrentUser(HttpContext)?.Id == userId)
        {
            TempData["Error"] = "Bạn không thể báo cáo chính mình.";
            return RedirectToPage(new { userId });
        }

        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = $"/Profile/{userId}" });
        }

        var result = await _api.PostAsync<object>("/api/reports/users", new
        {
            targetUserId = userId,
            targetCommentId = (int?)null,
            reportType = ReportType,
            description = Description
        }, token);

        TempData[result?.Success == true ? "Success" : "Error"] = result?.Success == true
            ? "Report submitted."
            : (result?.Message ?? "Could not submit report.");

        return RedirectToPage(new { userId });
    }
}
