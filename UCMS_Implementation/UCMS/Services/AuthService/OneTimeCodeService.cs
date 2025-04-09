using UCMS.Models;
using UCMS.Services.AuthService.Abstraction;

namespace UCMS.Services.AuthService;

public class OneTimeCodeService: IOneTimeCodeService
{
    public OneTimeCode Generate(int minutes = 5)
    {
        var randomCode = new Random().Next(100000, 999999).ToString();
        return new OneTimeCode
        {
            Code = randomCode,
            Expiry = DateTime.UtcNow.AddMinutes(minutes)
        };
    }

    public bool IsValid(OneTimeCode inputCode,OneTimeCode userOneTimeCode)
    {
        return userOneTimeCode.Code == inputCode.Code && DateTime.UtcNow <= userOneTimeCode.Expiry;
    }
}