using Microsoft.AspNetCore.Mvc;

namespace ExpenseSplitter.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
