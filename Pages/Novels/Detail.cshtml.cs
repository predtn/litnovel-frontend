using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Novels;

public class DetailModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public NovelDetailDto? Novel { get; set; }
    public ReadingProgressDto? CurrentReadingProgress { get; set; }
    public List<ReviewDto> Reviews { get; set; } = [];
    public int CurrentUserId { get; set; }

    public DetailModel(IApiService api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task OnGetAsync(string slug)
    {
        var token = _auth.GetToken(HttpContext);
        var novelResult = await _api.GetAsync<NovelDetailDto>($"/api/novels/{slug}", token);
        Novel = novelResult?.Data;

        if (Novel != null)
        {
            var reviewResult = await _api.GetAsync<ReviewPageDto>($"/api/novels/{Novel.Id}/reviews?page=1&size=10", token);
            Reviews = reviewResult?.Data?.Items ?? [];

            if (!string.IsNullOrWhiteSpace(token))
            {
                var progressResult = await _api.GetAsync<PagedData<ReadingProgressDto>>("/api/users/me/reading-history?filter=in-progress&page=1&size=100", token);
                CurrentReadingProgress = progressResult?.Data?.Items
                    .FirstOrDefault(item => item.Novel?.Id == Novel.Id && item.LastChapter != null);
            }
        }

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null)
        {
            ViewData["UserName"] = user.Username;
            ViewData["UserAvatar"] = user.Avatar;
            CurrentUserId = user.Id;
        }
    }

    public async Task<IActionResult> OnPostReviewAsync([FromRoute] string slug, [FromForm] int id, [FromForm] byte rating, [FromForm] string? reviewText)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        await _api.PostAsync<object>($"/api/novels/{id}/reviews", new { rating, review = reviewText }, token);
        return RedirectToPage(new { slug });
    }

    public async Task<IActionResult> OnPostUpdateReviewAsync([FromRoute] string slug, [FromForm] int reviewId, [FromForm] byte rating, [FromForm] string? reviewText)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        await _api.PutAsync<object>($"/api/reviews/{reviewId}", new { rating, review = reviewText }, token);
        return RedirectToPage(new { slug });
    }

    public async Task<IActionResult> OnPostReportAsync([FromRoute] string slug, int targetNovelId, string reportType, string description, int? targetChapterId)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        await _api.PostAsync<object>("/api/reports/novels", new { targetNovelId, targetChapterId, reportType, description }, token);
        TempData["Success"] = "Báo cáo đã được gửi.";
        return RedirectToPage(new { slug });
    }
}
