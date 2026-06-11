using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MoodPlaylistApi.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {

        protected string GetClaimValue(string claimType)
            => HttpContext.User.FindFirst(claimType)?.Value ??
            throw new Exception($"Claim '{claimType}' not found.");

        protected string GetUserId() => GetClaimValue(ClaimTypes.NameIdentifier);
        protected string GetName() => GetClaimValue(ClaimTypes.Name);
        protected string GetEmail() => GetClaimValue(ClaimTypes.Email);
    }
}
