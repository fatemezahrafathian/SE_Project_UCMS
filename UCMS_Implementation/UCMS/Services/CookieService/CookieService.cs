using UCMS.Services.CookieService.Abstraction;

namespace UCMS.Services.CookieService;

public class CookieService : ICookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public void CreateCookie(string data, TimeSpan? expires = null)
    {
        var context = _httpContextAccessor.HttpContext;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.Add(expires ?? TimeSpan.FromHours(1))
        };

        context.Response.Cookies.Append("access_token", data, cookieOptions);
    }
    public string? GetCookieValue()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies["access_token"];
    }
    public void DeleteTokenCookie()
    {
        var context = _httpContextAccessor.HttpContext;
        context.Response.Cookies.Delete("access_token");
    }
}