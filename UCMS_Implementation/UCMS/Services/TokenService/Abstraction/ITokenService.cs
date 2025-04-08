using System.Security.Claims;

namespace UCMS.Services.TokenService.Abstraction;

public interface ITokenService
{
    public string GenerateToken(List<Claim> claims);
    public int GetUserId(ClaimsPrincipal claimsPrincipal);
    public string? GetUserRole(ClaimsPrincipal user);
}