using Microsoft.AspNetCore.Mvc;

namespace EXE202.Controllers
{
    public class PageInformationController : Controller
    {
        public IActionResult AboutUs()
        {
            return View("Views/PageInformation/AboutUs.cshtml");
        }

        public IActionResult TermsOfUse()
        {
            return View("Views/PageInformation/TermsOfUse.cshtml");
        }

        public IActionResult PrivacyPolicy()
        {
            return View("Views/PageInformation/PrivacyPolicy.cshtml");
        }
    }
}
