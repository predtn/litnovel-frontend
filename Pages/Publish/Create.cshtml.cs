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
    public string? ResponseMessage { get; set; }
    public List<FormFeedbackItem> ResponseErrors { get; set; } = [];

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
        Input.Status = "Draft";
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
        if (!IsApiSuccess(created))
        {
            SetResponseError(ApiFailureMessage(created, "Unable to create novel."));
            await LoadLookupsAsync();
            return Page();
        }

        var id = created?.Data?.Id;
        if (!id.HasValue || id.Value <= 0)
        {
            id = await ResolveCreatedNovelIdAsync(Input.Title);
        }

        var createdStatus = created?.Data?.Status;
        if (!submit && id.HasValue && !IsDraft(createdStatus))
        {
            var draftResult = await Api.PutAsync<NovelSummaryDto>($"/api/novels/{id}", Input, Token);
            if (!IsApiSuccess(draftResult))
            {
                SetResponseError(ApiFailureMessage(draftResult, "Novel was created, but could not be kept as a draft."));
                await LoadLookupsAsync();
                return Page();
            }
        }

        if (submit && id.HasValue)
        {
            var submitResult = await Api.PostAsync<object>($"/api/novels/{id}/submit", null, Token);
            if (!IsApiSuccess(submitResult))
            {
                SetResponseError(ApiFailureMessage(submitResult, "Novel was saved as a draft, but could not be submitted."));
                await LoadLookupsAsync();
                return Page();
            }
        }

        TempData["Success"] = submit ? "Novel saved and submitted for review." : "Novel draft created.";
        return id.HasValue ? RedirectToPage("/Publish/Manage", new { id }) : RedirectToPage("/Publish/Index");
    }

    private static bool IsDraft(string? status)
        => string.Equals(status, "Draft", StringComparison.OrdinalIgnoreCase);

    private void SetResponseError(string message)
    {
        ResponseMessage = "Please check the highlighted fields and try again.";
        ResponseErrors = ParseFeedbackItems(message);

        if (ResponseErrors.Count == 0)
        {
            ResponseErrors.Add(new FormFeedbackItem("Form", CleanErrorText(message)));
        }

        foreach (var error in ResponseErrors)
        {
            if (TryMapFieldKey(error.FieldKey, out var modelKey))
            {
                ModelState.AddModelError(modelKey, error.Message);
            }
        }
    }

    private static List<FormFeedbackItem> ParseFeedbackItems(string message)
    {
        var items = new List<FormFeedbackItem>();
        var matches = System.Text.RegularExpressions.Regex.Matches(
            message,
            @"--\s*(?<field>[^:]+):\s*(?<message>.*?)(?=\s+Severity:\s*\w+|\s+--|$)",
            System.Text.RegularExpressions.RegexOptions.Singleline);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var field = match.Groups["field"].Value.Trim();
            var error = CleanErrorText(match.Groups["message"].Value);
            if (!string.IsNullOrWhiteSpace(error))
            {
                items.Add(new FormFeedbackItem(ToFriendlyFieldName(field), error, field));
            }
        }

        if (items.Count > 0) return items;

        var cleaned = CleanErrorText(message);
        if (!string.IsNullOrWhiteSpace(cleaned))
        {
            items.Add(new FormFeedbackItem("Form", cleaned));
        }

        return items;
    }

    private static string CleanErrorText(string value)
    {
        var cleaned = value
            .Replace("Validation failed:", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+Severity:\s*\w+", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return cleaned.Trim(' ', '-', '.', ':');
    }

    private static string ToFriendlyFieldName(string field) => field.Trim() switch
    {
        "CategoryId" => "Category",
        "TagIds" => "Tags",
        "Title" => "Title",
        "Description" => "Description",
        "CoverImage" => "Cover image",
        _ => field.Trim()
    };

    private static bool TryMapFieldKey(string field, out string modelKey)
    {
        modelKey = field switch
        {
            "CategoryId" => "Input.CategoryId",
            "TagIds" => "SelectedTagIds",
            "Title" => "Input.Title",
            "Description" => "Input.Description",
            "CoverImage" => "Input.CoverImage",
            _ => ""
        };

        return !string.IsNullOrWhiteSpace(modelKey);
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
        Categories = catTask.Result?.Data ?? [];
        Tags = tagTask.Result?.Data ?? [];
    }
}

public record FormFeedbackItem(string Field, string Message, string FieldKey = "");
