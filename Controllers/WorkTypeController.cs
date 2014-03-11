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
    public class WorkTypeController : Controller
    {
        //
        // GET: /WorkType/

        public ActionResult Index()
        {
            ViewBag.ActiveType = "WorkType";
           
            TaskDataContext db = new TaskDataContext();
            var type_list = from u in db.TODO_WorkType select u;

            return View(type_list.ToList());
        }

        //
        // GET: /WorkType/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /WorkType/Create

        public ActionResult Create(TODO_WorkType wtype)
        {
            ViewBag.ActiveType = "WorkType";
            return View(wtype);
        } 

        //
        // POST: /WorkType/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            ViewBag.ActiveType = "WorkType";
            TODO_WorkType wtype = new TODO_WorkType();
            try
            {
                wtype.name = collection["TypeName"];

                if (IsExistUsn(collection["TypeName"], 0))
                {
                    ViewData["ErrStr"] = "保存失败，已存在相同的类别名!";
                    return View(wtype);
                }
                WorkTypeDAO.Instance.CreateWorkType(wtype);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                return View(wtype);
            }
        }

        [NonAction]
        public bool IsExistUsn(string name, int wid)
        {
            ViewBag.ActiveType = "WorkType";
            TaskDataContext db = new TaskDataContext();

            var wtype = db.TODO_WorkType.Where(c => c.name == name);
            if (wid > 0)
            {
                wtype = wtype.Where(c => c.id != wid);
            }
            return wtype.Count() > 0;

        }
 
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveType = "WorkType";
            TaskDataContext db = new TaskDataContext();
            var wtype = db.TODO_WorkType.SingleOrDefault<TODO_WorkType>(s => s.id == id);
            if (wtype == null)
                throw new Exception("该类型不存在！");
            return View(wtype);
        }

        //
        // POST: /WorkType/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            ViewBag.ActiveType = "WorkType";
            TaskDataContext db = new TaskDataContext();
            var wtype = db.TODO_WorkType.SingleOrDefault<TODO_WorkType>(s => s.id == id);
            try
            {
                wtype.name = collection["TypeName"];

                if (IsExistUsn(collection["TypeName"], id))
                {
                    ViewData["ErrStr"] = "保存失败，已存在相同类型名!";
                    return View(wtype);
                }
                db.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                return View(wtype);
            }
        }

        //
        // GET: /WorkType/Delete/5
 
        public ActionResult Delete(int id)
        {
            string delmsg = "";
            try
            {
                string msg = WorkTypeDAO.Instance.DeleteWorkType(id);
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
