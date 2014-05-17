using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TODO.Controllers
{
    using Custom;
    using Models;
    using DAO;
    using PersistObject.models;
    [AdminAuthorize]
    public class PriorityController : Controller
    {
        //
        // GET: /Priority/

        public ActionResult Index()
        {
            ViewBag.ActiveType = "Priority";
           
            TaskDataContext db = new TaskDataContext();
            var type_list = from u in db.TODO_Priority select u;

            return View(type_list.ToList());
        }

        //
        // GET: /Priority/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Priority/Create

        public ActionResult Create(TODO_Priority wtype)
        {
            ViewBag.ActiveType = "Priority";
            return View(wtype);
        } 

        //
        // POST: /Priority/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            ViewBag.ActiveType = "Priority";
            TODO_Priority wpriority = new TODO_Priority();
            try
            {
                wpriority.name = collection["PriorityName"];
                wpriority.priority = Convert.ToInt32(collection["PriorityCode"]);
                
                PriorityDAO.Instance.CreatePriority(wpriority);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                return View(wpriority);
            }
        }

        [NonAction]
        public bool IsExistUsn(string name, int wid)
        {
            ViewBag.ActiveType = "Priority";
            TaskDataContext db = new TaskDataContext();

            var wtype = db.TODO_Priority.Where(c => c.name == name);
            if (wid > 0)
            {
                wtype = wtype.Where(c => c.id != wid);
            }
            return wtype.Count() > 0;

        }
 
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveType = "Priority";
            TaskDataContext db = new TaskDataContext();
            var wtype = db.TODO_Priority.SingleOrDefault<TODO_Priority>(s => s.id == id);
            if (wtype == null)
                throw new Exception("该类型不存在！");
            return View(wtype);
        }

        //
        // POST: /Priority/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            ViewBag.ActiveType = "Priority";
            TaskDataContext db = new TaskDataContext();
            var wpriority = db.TODO_Priority.SingleOrDefault<TODO_Priority>(s => s.id == id);
            try
            {
                wpriority.name = collection["PriorityName"];
                wpriority.priority = Convert.ToInt32(collection["PriorityCode"]);

                db.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                return View(wpriority);
            }
        }

        //
        // GET: /Priority/Delete/5
 
        public ActionResult Delete(int id)
        {
            string delmsg = "";
            try
            {
                string msg = PriorityDAO.Instance.DeletePriority(id);
                if (msg != "success")
                {
                    delmsg = "删除失败！原因：" + msg;
                }
            }
            catch (Exception ex)
            {
                    delmsg = "删除失败！原因：" + ex.ToString();
            }
            return RedirectToAction("Index", new { operateMsg = delmsg });
        }

       
    }
}
