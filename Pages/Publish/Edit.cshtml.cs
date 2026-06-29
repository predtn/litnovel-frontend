using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace litnovel_frontend.Pages.Publish;

public class EditModel : PublishPageModel
{
    private readonly IWebHostEnvironment _environment;

    [BindProperty] public NovelUpsertRequest Input { get; set; } = new();
    [BindProperty] public List<int> SelectedTagIds { get; set; } = [];
    [BindProperty] public IFormFile? CoverImageFile { get; set; }
    public NovelDetailDto Novel { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = [];
    public List<TagDto> Tags { get; set; } = [];
    public bool CanEditNovel => CanEditNovelStatus(Novel.Status);
    public bool WillSubmitForReviewAfterSave => ShouldMoveNovelToPendingAfterEdit(Novel.Status);
    public string SaveButtonText => WillSubmitForReviewAfterSave ? "Lưu và gửi duyệt lại" : "Lưu bản nháp";

    public EditModel(IApiService api, IAuthService auth, IWebHostEnvironment environment) : base(api, auth)
    {
        _environment = environment;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var loaded = await LoadAsync(id);
        if (!loaded) return RedirectToPage("/Publish/Index");
        if (!CanEditNovel)
        {
            TempData["Error"] = EditBlockedMessage("truyện");
            return RedirectToPage("/Publish/Manage", new { id });
        }

        Input = new()
        {
            Title = Novel.Title,
            Description = Novel.Description,
            CoverImage = Novel.CoverImage,
            CategoryId = Novel.Category?.Id,
            TagIds = Novel.Tags.Select(t => t.Id).ToList(),
            Status = Novel.Status
        };
        SelectedTagIds = Input.TagIds;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        var loaded = await LoadAsync(id);
        if (!loaded) return RedirectToPage("/Publish/Index");
        if (!CanEditNovel)
        {
            TempData["Error"] = EditBlockedMessage("truyện");
            return RedirectToPage("/Publish/Manage", new { id });
        }

        Input.TagIds = SelectedTagIds.Distinct().ToList();
        Input.Status = WillSubmitForReviewAfterSave ? "Pending" : "Draft";
        await ApplyCoverImageAsync();

        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError("Input.Title", "Cần nhập tiêu đề.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await Api.PutAsync<NovelSummaryDto>($"/api/novels/{id}", Input, Token);
        if (!IsApiSuccess(result))
        {
            ModelState.AddModelError("", ApiFailureMessage(result, "Không thể cập nhật truyện."));
            return Page();
        }

        TempData["Success"] = WillSubmitForReviewAfterSave
            ? "Đã lưu thay đổi và gửi truyện vào hàng chờ duyệt."
            : "Đã lưu bản nháp truyện.";
        return RedirectToPage("/Publish/Manage", new { id });
    }

    private string EditBlockedMessage(string contentName)
        => IsPendingReview(Novel.Status)
            ? $"{char.ToUpperInvariant(contentName[0])}{contentName[1..]} đang chờ duyệt. Vui lòng hủy gửi duyệt trước khi chỉnh sửa."
            : $"{char.ToUpperInvariant(contentName[0])}{contentName[1..]} đang bị khóa nên không thể chỉnh sửa.";

    private async Task ApplyCoverImageAsync()
    {
        if (CoverImageFile == null || CoverImageFile.Length == 0) return;

        if (CoverImageFile.Length > 2 * 1024 * 1024)
        {
            ModelState.AddModelError("CoverImageFile", "Ảnh bìa phải có dung lượng từ 2MB trở xuống.");
            return;
        }

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(CoverImageFile.ContentType))
        {
            ModelState.AddModelError("CoverImageFile", "Ảnh bìa phải là tệp JPG, PNG hoặc WEBP.");
            return;
        }

        var extension = Path.GetExtension(CoverImageFile.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = Path.Combine("uploads", "covers", fileName);
        var outputDirectory = Path.Combine(_environment.WebRootPath, "uploads", "covers");
        Directory.CreateDirectory(outputDirectory);

        var outputPath = Path.Combine(outputDirectory, fileName);
        await using (var stream = System.IO.File.Create(outputPath))
        {
            await CoverImageFile.CopyToAsync(stream);
        }

        Input.CoverImage = $"{Request.Scheme}://{Request.Host}/{relativePath.Replace('\\', '/')}";
    }

    private async Task<bool> LoadAsync(int id)
    {
        var novelTask = Api.GetAsync<NovelDetailDto>($"/api/novels/{id}", Token);
        var catTask = Api.GetAsync<List<CategoryDto>>("/api/categories", Token);
        var tagTask = Api.GetAsync<List<TagDto>>("/api/tags", Token);
        await Task.WhenAll(novelTask, catTask, tagTask);
        if (!IsApiSuccess(novelTask.Result) || novelTask.Result?.Data == null)
        {
            TempData["Error"] = ApiFailureMessage(novelTask.Result, "Không thể tải truyện.");
            return false;
        }

        Novel = novelTask.Result.Data;
        Categories = catTask.Result?.Data ?? [];
        Tags = tagTask.Result?.Data ?? [];
        return true;
    }
}
