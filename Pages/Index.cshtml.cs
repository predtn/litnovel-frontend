using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages;

public class IndexModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public List<CategoryDto> Categories { get; set; } = [];
    public List<NovelSummaryDto> TrendingNovels { get; set; } = [];
    public List<NovelSummaryDto> NewNovels { get; set; } = [];
    public List<NovelSummaryDto> TopRatedNovels { get; set; } = [];
    public List<ReadingProgressDto> ContinueReading { get; set; } = [];

    public IndexModel(IApiService api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);

        // Load homepage content in parallel.
        var catTask          = _api.GetAsync<List<CategoryDto>>("/api/categories");
        var trendingTask     = _api.GetAsync<PagedData<NovelSummaryDto>>("/api/novels?sort=viewCount&order=desc&status=Ongoing&page=1&size=24");
        var newTask          = _api.GetAsync<PagedData<NovelSummaryDto>>("/api/novels?sort=updatedAt&order=desc&status=Ongoing&page=1&size=8");
        var topTask          = _api.GetAsync<PagedData<NovelSummaryDto>>("/api/novels?sort=ratingAverage&order=desc&status=Ongoing&page=1&size=24");
        var readingTask      = !string.IsNullOrWhiteSpace(token)
            ? LoadContinueReadingAsync(token)
            : Task.FromResult(new List<ReadingProgressDto>());
        var unreadTask       = !string.IsNullOrWhiteSpace(token)
            ? LoadUnreadCountAsync(token)
            : Task.FromResult(0);

        await Task.WhenAll(catTask, trendingTask, newTask, topTask, readingTask, unreadTask);

        Categories      = catTask.Result?.Data ?? [];
        TrendingNovels  = (trendingTask.Result?.Data?.Items ?? [])
            .Where(novel => novel.ViewCount > 0)
            .Take(8)
            .ToList();
        NewNovels       = newTask.Result?.Data?.Items ?? [];
        TopRatedNovels  = (topTask.Result?.Data?.Items ?? [])
            .Where(novel => novel.RatingAverage > 0)
            .Take(8)
            .ToList();
        ContinueReading = readingTask.Result;
        ViewData["UnreadCount"] = unreadTask.Result;

        // Set user info for layout
        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null)
        {
            ViewData["UserName"]  = user.Username;
            ViewData["UserEmail"] = user.Email;
            ViewData["UserAvatar"]= user.Avatar;
        }
    }

    private async Task<List<ReadingProgressDto>> LoadContinueReadingAsync(string token)
    {
        var pagedResult = await _api.GetAsync<PagedData<ReadingProgressDto>>("/api/users/me/reading-history?filter=in-progress&page=1&size=4", token);
        if (pagedResult?.Success == true && pagedResult.Data?.Items != null)
        {
            return pagedResult.Data.Items;
        }

        var listResult = await _api.GetAsync<List<ReadingProgressDto>>("/api/users/me/reading-history?filter=in-progress&page=1&size=4", token);
        return listResult?.Success == true && listResult.Data != null ? listResult.Data.Take(4).ToList() : [];
    }

    private async Task<int> LoadUnreadCountAsync(string token)
    {
        var listResult = await _api.GetAsync<List<NotificationDto>>("/api/notifications?isRead=false", token);
        if (listResult?.Success == true && listResult.Data != null)
        {
            return listResult.Data.Count;
        }

        var pagedResult = await _api.GetAsync<NotificationListDto>("/api/notifications?isRead=false", token);
        if (pagedResult?.Success == true && pagedResult.Data != null)
        {
            return pagedResult.Data.UnreadCount > 0 ? pagedResult.Data.UnreadCount : pagedResult.Data.Items.Count;
        }

        return 0;
    }

}
