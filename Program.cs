using litnovel_frontend.Filters;
using litnovel_frontend.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ─── Services ───
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddFolderApplicationModelConvention("/",
        model => model.Filters.Add(new Microsoft.AspNetCore.Mvc.ServiceFilterAttribute(typeof(NotificationCountFilter))));
});
builder.Services.AddScoped<NotificationCountFilter>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".aspnet-data-protection-keys")));

// API HttpClient
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5181";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Auth service
builder.Services.AddScoped<IAuthService, AuthService>();

// Session (for TempData)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cookie Auth for Razor Pages authorization
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ─── Pipeline ───
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.Use(async (context, next) =>
{
    static bool ShouldSkipSessionSync(PathString path)
    {
        var value = path.Value ?? "";
        return value.StartsWith("/css/", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/js/", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/Auth/Logout", StringComparison.OrdinalIgnoreCase);
    }

    if (context.User.Identity?.IsAuthenticated == true && !ShouldSkipSessionSync(context.Request.Path))
    {
        var auth = context.RequestServices.GetRequiredService<IAuthService>();
        var validation = await auth.ValidateSessionAsync(context);
        if (!string.IsNullOrWhiteSpace(validation.Message))
        {
            context.Session.SetString("SessionNotice", validation.Message);
            context.Session.SetString("SessionNoticeType", validation.State == SessionValidationState.LoggedOut ? "error" : "success");
        }

        if (validation.State == SessionValidationState.LoggedOut)
        {
            context.Response.Redirect(validation.RedirectPath ?? "/Auth/Login");
            return;
        }
    }

    await next();
});
app.UseAuthorization();

app.MapRazorPages();

app.Run();
