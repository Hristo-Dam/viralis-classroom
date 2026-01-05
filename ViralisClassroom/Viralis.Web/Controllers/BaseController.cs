using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Viralis.Web.Controllers
{
    [Authorize, AutoValidateAntiforgeryToken]
    public class BaseController : Controller
    {
    }
}
