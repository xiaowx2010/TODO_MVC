using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TODO.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.ActiveType = "Home";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        [Authorize]
        public ActionResult MyTask()
        {
            ViewBag.ActiveType = "MyTask";
            return View();
        }
        public ActionResult TaskControl()
        {
            ViewBag.ActiveType = "TaskControl";
            return View();
        }
        public ActionResult UserControl()
        {
            ViewBag.ActiveType = "UserControl";
            return View();
        }
    }
}
