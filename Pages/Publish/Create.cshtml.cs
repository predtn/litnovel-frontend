using litnovel_frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace litnovel_frontend.Pages.Publish;

public class CreateModel : PublishPageModel
{
    private readonly IWebHostEnvironment _environment;

    [BindProperty] public NovelUpsertRequest Input { get; set; } = new();
    [BindProperty] public List<int> SelectedTagIds { get; set; } = [];
    [BindProperty] public IFormFile? CoverImageFile { get; set; }
    public List<CategoryDto> Categories { get; set; } = [];
    public List<TagDto> Tags { get; set; } = [];

    public CreateModel(IApiService api, IAuthService auth, IWebHostEnvironment environment) : base(api, auth)
    {
        _environment = environment;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var guard = RequireAuthor();
        if (guard != null) return guard;
        await LoadLookupsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(bool submit = false)
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
            await LoadLookupsAsync();
            return Page();
        }

        var created = await Api.PostAsync<NovelSummaryDto>("/api/novels", Input, Token);
        if (created?.Success == false)
        {
            ModelState.AddModelError("", created.Message ?? "Unable to create novel.");
            await LoadLookupsAsync();
            return Page();
        }

        var id = created?.Data?.Id;
        if (!id.HasValue || id.Value <= 0)
        {
            id = await ResolveCreatedNovelIdAsync(Input.Title);
        }

        if (submit && id.HasValue)
        {
            await Api.PostAsync<object>($"/api/novels/{id}/submit", null, Token);
        }

        TempData["Success"] = submit ? "Novel saved and submitted for review." : "Novel draft created.";
        return id.HasValue ? RedirectToPage("/Publish/Manage", new { id }) : RedirectToPage("/Publish/Index");
    }

    private async Task<int?> ResolveCreatedNovelIdAsync(string title)
    {
        var result = await Api.GetAsync<PagedData<NovelSummaryDto>>(
            "/api/novels/my" + ODataQuery.Build(size: 10, orderBy: "UpdatedAt desc"),
            Token);

        return result?.Data?.Items
            .FirstOrDefault(n => string.Equals(n.Title, title, StringComparison.OrdinalIgnoreCase))
            ?.Id;
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

    private async Task LoadLookupsAsync()
    {
        var catTask = Api.GetAsync<List<CategoryDto>>("/api/categories", Token);
        var tagTask = Api.GetAsync<List<TagDto>>("/api/tags", Token);
        await Task.WhenAll(catTask, tagTask);
        Categories = catTask.Result?.Data ?? MockCategories();
        Tags = tagTask.Result?.Data ?? MockTags();
    }
}
