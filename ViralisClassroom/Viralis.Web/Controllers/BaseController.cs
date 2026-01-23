using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Viralis.Web.Controllers
{
    [Authorize, AutoValidateAntiforgeryToken]
    public abstract class BaseController : Controller
    {
        protected Guid CurrentUserId
            => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
