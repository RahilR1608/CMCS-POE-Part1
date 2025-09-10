using Microsoft.AspNetCore.Mvc;

namespace CMCS.Prototype.Controllers
{
    public class CoordinatorController : Controller
    {
        public IActionResult Index() => View();
    }
}
