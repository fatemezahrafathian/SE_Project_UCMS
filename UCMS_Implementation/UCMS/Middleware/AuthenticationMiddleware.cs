using System.Security.Claims;
using UCMS.Repositories.UserRepository.Abstraction;

namespace UCMS.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            var user = await userRepository.GetUserByIdAsync(int.Parse(userId));
            if (user != null)
            {
                context.Items["User"] = user;
            }
        }

        await _next(context);
    }
}
