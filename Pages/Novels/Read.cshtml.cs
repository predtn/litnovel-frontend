using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Novels;

public class ReadModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public ChapterDetailDto? Chapter { get; set; }
    public NovelDetailDto? Novel { get; set; }
    public List<CommentDto> Comments { get; set; } = [];
    public int TotalComments { get; set; }
    public int? CurrentUserId { get; set; }

    public ReadModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    public async Task OnGetAsync(string novelSlug, string chapterSlug)
    {
        var token = _auth.GetToken(HttpContext);
        var chapterResult = await _api.GetAsync<ChapterDetailDto>($"/api/chapters/{chapterSlug}", token);
        Chapter = chapterResult?.Data;

        if (Chapter != null)
        {
            var novelResult = await _api.GetAsync<NovelDetailDto>($"/api/novels/{novelSlug}", token);
            Novel = novelResult?.Data;

            var cmtResult = await _api.GetAsync<PagedData<CommentDto>>($"/api/chapters/{Chapter.Id}/comments?page=1&size=20", token);
            Comments = cmtResult?.Data?.Items ?? [];
            TotalComments = cmtResult?.Data?.TotalElements ?? Comments.Count;
        }

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null)
        {
            CurrentUserId = user.Id;
            ViewData["UserName"] = user.Username;
            ViewData["UserAvatar"] = user.Avatar;
        }
    }

    public async Task<IActionResult> OnPostCommentAsync([FromRoute] string novelSlug, [FromRoute] string chapterSlug, [FromForm] int chapterId, string content)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        await _api.PostAsync<object>($"/api/chapters/{chapterId}/comments", new { content }, token);
        return RedirectToPage(new { novelSlug, chapterSlug });
    }

    public async Task<IActionResult> OnPostReplyAsync([FromRoute] string novelSlug, [FromRoute] string chapterSlug, [FromForm] int chapterId, int parentCommentId, string content)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        await _api.PostAsync<object>($"/api/comments/{parentCommentId}/replies", new { content }, token);
        return RedirectToPage(new { novelSlug, chapterSlug });
    }

}
