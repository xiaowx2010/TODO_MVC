using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace TODO.Custom
{
    public class BaseController : Controller
    {
        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            filterContext.Result = View();

            filterContext.Result = Redirect("/Error/ServerError.html");
            filterContext.ExceptionHandled = true;
            //把异常写入日志
            Common.WriteFile(filterContext.Exception.Message, Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
        }
    }
}
