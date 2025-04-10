using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UCMS.Models;

namespace UCMS.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class RoleBasedAuthorizationAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public RoleBasedAuthorizationAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = (User)context.HttpContext.Items["User"];
        
        if (user == null)
        {
            context.Result = new UnauthorizedResult();
        }
        else if (!_roles.Contains(user.Role.Name)) // remove role from cookie claims
        {
            context.Result = new ForbidResult();
        }
    }
}

