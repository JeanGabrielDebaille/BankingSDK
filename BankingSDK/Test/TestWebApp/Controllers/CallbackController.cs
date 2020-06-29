using Microsoft.AspNetCore.Mvc;
using TestWebApp.Models;

namespace TestWebApp.Controllers
{
    public class CallbackController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            if (!string.IsNullOrEmpty(Storage.FinalizeUrl))
            {
                return Redirect(Storage.FinalizeUrl + "?" + HttpContext.Request.QueryString);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}