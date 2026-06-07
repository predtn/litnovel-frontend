using litnovel_frontend.Services; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace litnovel_frontend.Pages.Admin.Statistics;
public class StatData { public int NewUsers; public int UserGrowthPercent; public int NewNovels; public int ChaptersThisWeek; public int TotalViews; public List<TopNovelItem> TopNovels=[];  public List<TopAuthorItem> TopAuthors=[]; }
public record TopNovelItem(int Rank, string Title, int ViewCount);
public record TopAuthorItem(string Username, int NovelsCreated, int TotalViews);
public class IndexModel : PageModel
{
    private readonly IApiService _api; private readonly IAuthService _auth;
    public StatData Stats { get; set; } = new();
    public IndexModel(IApiService api, IAuthService auth) { _api = api; _auth = auth; }
    public async Task<IActionResult> OnGetAsync()
    {
        var token = _auth.GetToken(HttpContext);
        if (!_auth.IsInRole(HttpContext, "Admin")) return RedirectToPage("/Index");
        var u = _auth.GetCurrentUser(HttpContext); if (u!=null){ViewData["UserName"]=u.Username;ViewData["UserEmail"]=u.Email;}
        Stats = new()
        {
            NewUsers=234, UserGrowthPercent=12, NewNovels=45, ChaptersThisWeek=340, TotalViews=4800000,
            TopNovels=[new(1,"Long Vương Truyền Thuyết",128432),new(2,"Thiên Đạo Thư Viện",98120),new(3,"Vô Hạn Phục Hồi",85200),new(4,"Mộng Tình Thiên Hạ",76400),new(5,"Chiến Thần Tái Lâm",65800)],
            TopAuthors=[new("author_star",12,280000),new("tac_gia_1",8,180000),new("writer_pro",5,120000)]
        };
        return Page();
    }
}
