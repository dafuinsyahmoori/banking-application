using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BankingApplication.Utilities
{
    public class AuthenticationUtility(IHttpContextAccessor httpContextAccessor)
    {
        public async Task SignInAsync(string id, string role, string emailOrUsername)
        {
            var claims = new Claim[]
            {
                new("Role", role),
                new("ID", id),
                new(ClaimTypes.NameIdentifier, emailOrUsername)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await httpContextAccessor.HttpContext!.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }
    }
}