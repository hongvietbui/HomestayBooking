using Microsoft.AspNetCore.Mvc;

namespace EXE202.Controllers
{
    public class HostController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
