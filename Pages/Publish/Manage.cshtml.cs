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
            TempData["Error"] = ApiFailureMessage(result, "Không thể tải truyện.");
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
        SetApiResultMessage(result, "Truyện đã được gửi duyệt.", "Không thể gửi duyệt truyện.");
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var result = await Api.DeleteAsync<object>($"/api/novels/{id}", Token);
        SetApiResultMessage(result, "Đã xóa truyện.", "Không thể xóa truyện.");
        return RedirectToPage("/Publish/Index");
    }
}
