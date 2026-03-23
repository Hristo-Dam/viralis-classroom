using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Viralis.Web.Controllers
{
    // Not decorated with [Authorize] — error pages must be reachable by anyone,
    // including unauthenticated users who hit a 404 or 403.
    public class ErrorController : Controller
    {
        // Handles status codes routed by UseStatusCodePagesWithReExecute("/Error/{0}")
        [Route("Error/{statusCode}")]
        public IActionResult HandleStatusCode(int statusCode)
        {
            return statusCode switch
            {
                404 => View("NotFound"),
                403 => View("Forbidden"),
                401 => View("Forbidden"),   // treat Unauthorised the same as Forbidden for UX
                _   => View("Index", statusCode)
            };
        }

        // Catches unhandled exceptions via UseExceptionHandler("/Error/Index")
        [Route("Error/Index")]
        public IActionResult Index()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            // You could log exceptionFeature?.Error here with ILogger if needed.
            // Don't expose exception details to the user in production.
            return View("Index", (int?)null);
        }
    }
}
