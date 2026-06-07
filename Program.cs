using litnovel_frontend.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Services ───
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// API HttpClient
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001";
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
app.UseAuthorization();

app.MapRazorPages();

app.Run();
