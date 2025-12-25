using Microsoft.AspNetCore.Mvc;

namespace SV22T1080075.Shop.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
