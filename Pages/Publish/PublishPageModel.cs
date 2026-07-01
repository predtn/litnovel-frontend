using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace litnovel_frontend.Pages.Publish;

public abstract class PublishPageModel : PageModel
{
    protected readonly IApiService Api;
    protected readonly IAuthService Auth;

    protected PublishPageModel(IApiService api, IAuthService auth)
    {
        Api = api;
        Auth = auth;
    }

    protected IActionResult? RequireAuthor()
    {
        var token = Auth.GetToken(HttpContext);
        if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");
        if (!Auth.IsInRole(HttpContext, "User")) return RedirectToPage("/Index");

        var user = Auth.GetCurrentUser(HttpContext);
        if (user != null)
        {
            ViewData["UserName"] = user.Username;
            ViewData["UserEmail"] = user.Email;
            ViewData["UserAvatar"] = user.Avatar;
        }

        ViewData["ActiveNav"] = "publish";
        return null;
    }

    protected string? Token => Auth.GetToken(HttpContext);

    protected bool IsApiSuccess<T>(ApiResponse<T>? result) => result?.Success == true;

    protected string ApiFailureMessage<T>(ApiResponse<T>? result, string fallback)
        => IsProductionSafeMessage(result?.Message) ? result!.Message! : fallback;

    protected void SetApiResultMessage<T>(ApiResponse<T>? result, string successFallback, string failureFallback)
    {
        if (IsApiSuccess(result))
        {
            TempData["Success"] = string.IsNullOrWhiteSpace(result?.Message) ? successFallback : result.Message;
            return;
        }

        TempData["Error"] = ApiFailureMessage(result, failureFallback);
    }

    protected static bool CanSubmitForReview(string? status)
        => IsDraftLikeStatus(status);

    protected static bool CanEditNovelStatus(string? status)
        => IsDraftLikeStatus(status) || IsPublicNovelStatus(status);

    protected static bool CanEditChapterStatus(string? status)
        => IsDraftLikeStatus(status) || IsPublicChapterStatus(status);

    protected static bool CanChangeNovelLifecycleStatus(string? status)
        => IsAllowedNovelLifecycleStatus(status);

    protected static bool IsAllowedNovelLifecycleStatus(string? status)
        => string.Equals(status, "Ongoing", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Ended", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Hiatus", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Dropped", StringComparison.OrdinalIgnoreCase);

    protected static bool ShouldMoveNovelToPendingAfterEdit(string? status)
        => IsPublicNovelStatus(status);

    protected static bool ShouldMoveChapterToPendingAfterEdit(string? status)
        => IsPublicChapterStatus(status);

    protected static bool IsApprovedChapterStatus(string? status)
        => IsPublicChapterStatus(status);

    protected static bool IsPendingReview(string? status)
        => string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase);

    protected static bool IsLockedStatus(string? status)
        => string.Equals(status, "Locked", StringComparison.OrdinalIgnoreCase);

    private static bool IsDraftLikeStatus(string? status)
        => string.Equals(status, "Draft", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Rejected", StringComparison.OrdinalIgnoreCase);

    private static bool IsPublicNovelStatus(string? status)
        => string.Equals(status, "Ongoing", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Ended", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Hiatus", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Dropped", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Canceled", StringComparison.OrdinalIgnoreCase);

    private static bool IsPublicChapterStatus(string? status)
        => string.Equals(status, "Published", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Scheduled", StringComparison.OrdinalIgnoreCase);

    private static bool IsProductionSafeMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return false;

        var blocked = new[]
        {
            "Request failed",
            "Unable to connect",
            "HTTP ",
            "API ",
            "backend",
            "failed",
            "exception",
            "stack"
        };

        return !blocked.Any(value => message.Contains(value, StringComparison.OrdinalIgnoreCase));
    }
}
