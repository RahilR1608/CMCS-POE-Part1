using Microsoft.AspNetCore.Mvc;

namespace CMCS.Prototype.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Index() => View();
    }
}
