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

    public EditModel(IApiService api, IAuthService auth, IWebHostEnvironment environment) : base(api, auth)
    {
        _environment = environment;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        await LoadAsync(id);
        Input = new()
        {
            Title = Novel.Title,
            Description = Novel.Description,
            CoverImage = Novel.CoverImage,
            CategoryId = Novel.Category?.Id,
            TagIds = Novel.Tags.Select(t => t.Id).ToList()
        };
        SelectedTagIds = Input.TagIds;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        Input.TagIds = SelectedTagIds.Take(10).ToList();
        await ApplyCoverImageAsync();

        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError("Input.Title", "Title is required.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(id);
            return Page();
        }

        var result = await Api.PutAsync<NovelSummaryDto>($"/api/novels/{id}", Input, Token);
        if (result?.Success == false)
        {
            ModelState.AddModelError("", result.Message ?? "Unable to update novel.");
            await LoadAsync(id);
            return Page();
        }

        TempData["Success"] = "Novel updated.";
        return RedirectToPage("/Publish/Manage", new { id });
    }

    private async Task ApplyCoverImageAsync()
    {
        if (CoverImageFile == null || CoverImageFile.Length == 0) return;

        if (CoverImageFile.Length > 2 * 1024 * 1024)
        {
            ModelState.AddModelError("CoverImageFile", "Cover image must be 2MB or smaller.");
            return;
        }

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(CoverImageFile.ContentType))
        {
            ModelState.AddModelError("CoverImageFile", "Cover image must be JPG, PNG, or WEBP.");
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

    private async Task LoadAsync(int id)
    {
        var novelTask = Api.GetAsync<NovelDetailDto>($"/api/novels/{id}", Token);
        var catTask = Api.GetAsync<List<CategoryDto>>("/api/categories", Token);
        var tagTask = Api.GetAsync<List<TagDto>>("/api/tags", Token);
        await Task.WhenAll(novelTask, catTask, tagTask);
        Novel = novelTask.Result?.Data ?? MockNovel(id);
        Categories = catTask.Result?.Data ?? MockCategories();
        Tags = tagTask.Result?.Data ?? MockTags();
    }
}
