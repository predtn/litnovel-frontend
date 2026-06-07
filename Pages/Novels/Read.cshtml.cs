using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Novels;

public class ReadModel : PageModel
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;

    public ChapterDetailDto? Chapter { get; set; }
    public List<CommentDto> Comments { get; set; } = [];
    public int TotalComments { get; set; }

    public ReadModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

    public async Task OnGetAsync(string novelSlug, int chapterId)
    {
        var token = _auth.GetToken(HttpContext);
        var chTask  = _api.GetAsync<ChapterDetailDto>($"/api/chapters/{chapterId}", token);
        var cmtTask = _api.GetAsync<PagedData<CommentDto>>($"/api/chapters/{chapterId}/comments?page=1&size=20");
        await Task.WhenAll(chTask, cmtTask);

        Chapter       = chTask.Result?.Data ?? GetMockChapter(chapterId, novelSlug);
        Comments      = cmtTask.Result?.Data?.Items ?? GetMockComments();
        TotalComments = cmtTask.Result?.Data?.TotalElements ?? Comments.Count;

        var user = _auth.GetCurrentUser(HttpContext);
        if (user != null) { ViewData["UserName"] = user.Username; ViewData["UserAvatar"] = user.Avatar; }
    }

    public async Task<IActionResult> OnPostCommentAsync(int chapterId, string content)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        await _api.PostAsync<object>($"/api/chapters/{chapterId}/comments", new { content }, token);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReplyAsync(int chapterId, int parentCommentId, string content)
    {
        var token = _auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        await _api.PostAsync<object>($"/api/comments/{parentCommentId}/replies", new { content }, token);
        return RedirectToPage();
    }

    private ChapterDetailDto GetMockChapter(int id, string novelSlug) => new()
    {
        Id = id, ChapterNumber = 1, Title = "Chương 1: Thức Tỉnh",
        Slug = $"chuong-1-thuc-tinh",
        Content = @"<p>Bầu trời u ám phủ xuống mặt đất, những đám mây đen kịt che khuất ánh mặt trời. Từ trong hang động sâu thẳm, một làn hơi thở dài vang vọng, báo hiệu sự thức tỉnh của một sinh linh đã ngủ suốt ngàn năm...</p>
<p>Long Thiên mở mắt ra, đôi mắt vàng kim phát ra ánh sáng rực rỡ trong bóng tối. Trí ức của hắn từ từ trở lại — ký ức của Long Vương Thượng Cổ, kẻ đã từng thống trị cả thiên địa.</p>
<p>Nhưng thế giới này... đã khác rồi. Hắn cảm nhận được những dòng năng lượng kỳ lạ lưu chuyển trong không khí — không phải linh khí thuần túy như xưa, mà là thứ gì đó mạnh mẽ hơn, hỗn độn hơn.</p>
<p>— Tu tiên thế giới... Ta đã trở lại — hắn thì thầm, giọng khàn khàn vì ngàn năm chưa mở miệng.</p>
<p>Đứng dậy khỏi tảng đá, Long Thiên bước ra khỏi hang. Ánh sáng mặt trời chiếu thẳng vào mặt hắn, một cảm giác quen thuộc mà đã rất lâu hắn không được tận hưởng.</p>
<p>Hành trình của hắn... bắt đầu từ đây.</p>",
        Status = "Published",
        Novel = new() { Id = 1, Title = "Long Vương Truyền Thuyết", Slug = novelSlug },
        Volume = new() { Id = 1, VolumeNumber = 1, Title = "Tập 1: Tái Sinh" },
        PrevChapter = null,
        NextChapter = new() { Id = id + 1, ChapterNumber = 2, Title = "Chương 2: Thế Giới Mới" },
        CreatedAt = DateTime.UtcNow.AddDays(-60)
    };

    private List<CommentDto> GetMockComments() =>
    [
        new() { Id = 1, User = new() { Username = "doc_gia_1" }, Content = "Mở đầu cực hay! Long Vương tái sinh rồi.", LikeCount = 15, CreatedAt = DateTime.UtcNow.AddHours(-2),
            Replies = [new() { Id = 10, User = new() { Username = "doc_gia_2" }, Content = "Ừ, mình cũng thấy vậy!", LikeCount = 3, CreatedAt = DateTime.UtcNow.AddHours(-1) }] },
        new() { Id = 2, User = new() { Username = "doc_gia_3" }, Content = "Lối viết rất cuốn, đợi chương sau!", LikeCount = 8, CreatedAt = DateTime.UtcNow.AddHours(-5) },
    ];
}
