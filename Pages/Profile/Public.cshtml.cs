using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Profile;
public class PublicModel : PageModel
{
    private readonly IApiService _api;
    public new UserDetailDto? User { get; set; }
    public List<NovelSummaryDto> Novels { get; set; } = [];
    public PublicModel(IApiService api) { _api = api; }
    public async Task OnGetAsync(int userId)
    {
        var t1 = _api.GetAsync<UserDetailDto>($"/api/users/{userId}");
        var t2 = _api.GetAsync<PagedData<NovelSummaryDto>>($"/api/novels?authorId={userId}&page=1&size=12");
        await Task.WhenAll(t1, t2);
        User = t1.Result?.Data ?? new() { Id = userId, Username = "author_demo", Role = "User", Status = "Online", Reputation = 980, Stats = new() { NovelsCreated = 5, ChaptersPublished = 120 }, Badges = [new() { Name = "Tác giả tích cực", Icon = "✍️" }], CreatedAt = DateTime.UtcNow.AddDays(-365) };
        Novels = t2.Result?.Data?.Items ?? [new() { Title = "Long Vương Truyền Thuyết", Slug = "long-vuong", RatingAverage = 4.5 }];
    }
}
