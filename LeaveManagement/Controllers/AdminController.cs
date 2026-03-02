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
        public IActionResult LeaveRequests()
        {
            return View();
        }

        public IActionResult LeaveType()
        {
            return View();
        }
    }
}
