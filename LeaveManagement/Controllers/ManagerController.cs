using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult EmployeeList()
        {
            return View();
        }

        public IActionResult LeaveRequests()
        {
            return View();
        }

        public IActionResult MyProfile()
        {
            return View();
        }
    }
}