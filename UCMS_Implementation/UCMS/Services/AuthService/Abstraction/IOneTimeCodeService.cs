using UCMS.Models;

namespace UCMS.Services.AuthService.Abstraction;

public interface IOneTimeCodeService
{
    OneTimeCode Generate(int minutes = 5);
    public bool IsValid(OneTimeCode inputCode,OneTimeCode userOneTimeCode);
}