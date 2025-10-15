using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UserList()
        {
            return View();
        }
    }
}
