using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace litnovel_frontend.Pages.Publish;

public class ManageModel : PublishPageModel
{
    public NovelDetailDto Novel { get; set; } = new();
    public bool HasPendingChapters => Novel.Volumes
        .SelectMany(volume => volume.Chapters)
        .Any(chapter => IsPendingReview(chapter.Status));
    public bool CanSubmitNovel => CanSubmitForReview(Novel.Status);
    public bool CanEditNovel => CanEditNovelStatus(Novel.Status);
    public bool CanCancelReview => IsPendingReview(Novel.Status);
    public bool CanDeleteNovel => !IsPendingReview(Novel.Status) && !HasPendingChapters;
    public string EditUnavailableMessage => IsPendingReview(Novel.Status)
        ? "Truyện đang chờ duyệt, không thể chỉnh sửa thông tin truyện."
        : $"Không thể chỉnh sửa khi truyện đang {DisplayText.Status(Novel.Status).ToLowerInvariant()}.";

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

        var novel = await LoadNovelForActionAsync(id);
        if (novel == null) return RedirectToPage("/Publish/Index");
        if (!CanSubmitForReview(novel.Status))
        {
            TempData["Error"] = "Truyện chỉ có thể gửi duyệt khi đang là bản nháp hoặc cần chỉnh sửa.";
            return RedirectToPage(new { id });
        }

        var result = await Api.PostAsync<object>($"/api/novels/{id}/submit", null, Token);
        SetApiResultMessage(result, "Truyện đã được gửi duyệt.", "Chưa thể gửi duyệt truyện lúc này.");
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;

        var novel = await LoadNovelForActionAsync(id);
        if (novel == null) return RedirectToPage("/Publish/Index");
        if (IsPendingReview(novel.Status))
        {
            TempData["Error"] = "Truyện đang chờ duyệt nên chưa thể xóa.";
            return RedirectToPage(new { id });
        }

        if (novel.Volumes.SelectMany(volume => volume.Chapters).Any(chapter => IsPendingReview(chapter.Status)))
        {
            TempData["Error"] = "Truyện còn chương đang chờ duyệt nên chưa thể xóa.";
            return RedirectToPage(new { id });
        }

        var result = await Api.DeleteAsync<object>($"/api/novels/{id}", Token);
        SetApiResultMessage(result, "Đã xóa truyện.", "Chưa thể xóa truyện lúc này.");
        return RedirectToPage("/Publish/Index");
    }

    public async Task<IActionResult> OnPostCancelReviewAsync(int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;

        var novel = await LoadNovelForActionAsync(id);
        if (novel == null) return RedirectToPage("/Publish/Index");
        if (!IsPendingReview(novel.Status))
        {
            TempData["Error"] = "Chỉ có thể hủy gửi duyệt khi truyện đang chờ duyệt.";
            return RedirectToPage(new { id });
        }

        var result = await Api.PostAsync<NovelSummaryDto>($"/api/novels/{id}/withdraw", null, Token);
        if (!IsApiSuccess(result))
        {
            TempData["Error"] = ApiFailureMessage(result, "Chưa thể hủy gửi duyệt lúc này.");
            return RedirectToPage(new { id });
        }

        var verifyResult = await Api.GetAsync<NovelDetailDto>($"/api/novels/{id}", Token);
        if (!string.Equals(verifyResult?.Data?.Status, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Hệ thống chưa cập nhật truyện về bản nháp. Vui lòng thử lại sau.";
            return RedirectToPage(new { id });
        }

        TempData["Success"] = "Đã hủy gửi duyệt. Truyện đã trở lại bản nháp.";
        return RedirectToPage(new { id });
    }

    private async Task<NovelDetailDto?> LoadNovelForActionAsync(int id)
    {
        var result = await Api.GetAsync<NovelDetailDto>($"/api/novels/{id}", Token);
        if (IsApiSuccess(result) && result?.Data != null) return result.Data;

        TempData["Error"] = ApiFailureMessage(result, "Không thể tải thông tin truyện.");
        return null;
    }
}
