using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace JustLearnIT.Security
{
    public class RoleAuthFilter : TypeFilterAttribute
    {
        public RoleAuthFilter(string roleNames) : base(typeof(RoleRequirementFilter))
        {
            Arguments = new object[] { roleNames };
        }
    }

    public class RoleRequirementFilter : IAuthorizationFilter
    {
        readonly string[] _roles;

        public RoleRequirementFilter(string roles)
        {
            _roles = roles.Split(',');
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = AuthService.GetJWTRole(context.HttpContext.Session.GetString("TOKEN"));

            if (!_roles.Contains(role))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}

