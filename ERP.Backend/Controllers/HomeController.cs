using Microsoft.AspNetCore.Mvc;

namespace ERP.Backend.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Serves the React SPA index page for all non-API routes.
        /// The React app handles its own client-side routing.
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }
    }
}
