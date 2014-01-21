using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TODO.Controllers
{
    using Custom;
    [CustomAuthorize]
    public class HomeController : BaseController
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

        public ActionResult MyTask()
        {
            ViewBag.ActiveType = "MyTask";
            return View();
        }
        [AdminAuthorize]
        public ActionResult TaskControl()
        {
            ViewBag.ActiveType = "TaskControl";
            return View();
        }
        [AdminAuthorize]
        public ActionResult UserControl()
        {
            ViewBag.ActiveType = "UserControl";
            return View();
        }
    }
}
