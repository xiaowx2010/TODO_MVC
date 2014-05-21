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
    public class DescController : Controller
    {
        //
        // GET: /Desc/

        public ActionResult Index()
        {
            ViewBag.ActiveType = "Desc";
           
            TaskDataContext db = new TaskDataContext();
            var type_list = from u in db.TODO_Desc select u;

            return View(type_list.ToList());
        }

        //
        // GET: /Desc/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Desc/Create
        [NonAction]
        private void Init_Type(string type)
        {
            List<SelectListItem> c = new List<SelectListItem>();
            c.Add(new SelectListItem { Text = "任务名", Value = "任务名", Selected = true });
            c.Add(new SelectListItem { Text = "工作要求", Value = "工作要求" });

            ViewData["DescType"] = new SelectList(c, "Value", "Text", type);
        }

        public ActionResult Create(TODO_Desc wtype)
        {
            ViewBag.ActiveType = "Desc";

            Init_Type("");
            return View(wtype);
        } 

        //
        // POST: /Desc/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            ViewBag.ActiveType = "Desc";
            TODO_Desc wdesc = new TODO_Desc();
            try
            {
                wdesc.type = collection["DescType"];
                wdesc.in_use = collection["IN_USE"] == "on";
                wdesc.description = collection["Desc"];

                DescDAO.Instance.CreateDesc(wdesc);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                Init_Type(wdesc.type);
                return View(wdesc);
            }
        }

        
        public ActionResult Edit(int id)
        {
            ViewBag.ActiveType = "Desc";
            TaskDataContext db = new TaskDataContext();
            var wdesc = db.TODO_Desc.SingleOrDefault<TODO_Desc>(s => s.id == id);
            if (wdesc == null)
                throw new Exception("该类型不存在！");
            Init_Type(wdesc.type);
            return View(wdesc);
        }

        //
        // POST: /Desc/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            ViewBag.ActiveType = "Desc";
            TaskDataContext db = new TaskDataContext();
            var wdesc = db.TODO_Desc.SingleOrDefault<TODO_Desc>(s => s.id == id);
            try
            {
                wdesc.type = collection["DescType"];
                wdesc.in_use = collection["IN_USE"] == "on";
                wdesc.description = collection["Desc"];
                db.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");

                Init_Type(wdesc.type);
                return View(wdesc);
            }
        }

        //
        // GET: /Desc/Delete/5
 
        public ActionResult Delete(int id)
        {
            string delmsg = "";
            try
            {
                string msg = DescDAO.Instance.DeleteDesc(id);
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

        public JsonResult SaveDesc(string desc, string type)
        {
            try
            {
                TODO_Desc wdesc = new TODO_Desc();
                wdesc.type = type;
                wdesc.in_use = true;
                wdesc.description = desc;

                DescDAO.Instance.CreateDesc(wdesc);
                return Json(new { status = "success", data = "" }, JsonRequestBehavior.DenyGet);
                
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", data = ex.Message });
            }
        }

       
    }
}
