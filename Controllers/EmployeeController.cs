using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MyProfile()
        {
            return View();
        }
    }
}
