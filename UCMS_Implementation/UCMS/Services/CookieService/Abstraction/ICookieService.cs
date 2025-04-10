namespace UCMS.Services.CookieService.Abstraction;

public interface ICookieService
{
    public void CreateCookie(string data, TimeSpan? expires = null);
    public string? GetCookieValue();
    public void DeleteTokenCookie();
}