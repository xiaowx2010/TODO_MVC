using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Transactions;

namespace TODO.Controllers
{
    using Custom;
    using Models;
    using PersistObject.models;
    using DAO;
    using System.Data.Linq;
    [AdminAuthorize]
    public class HistoryController : Controller
    {
        //
        // GET: /History/

        public ActionResult Index(string taskName, int statusList = -2, string delayList = "", string worktypelist = "", int userlist = 0)
        {
            ViewBag.ActiveType = "HistoryTask";
            List<SelectListItem> a = new List<SelectListItem>();
            a.Add(new SelectListItem { Text = "-请选择任务状态-", Value = "-2", Selected = true });
            a.Add(new SelectListItem { Text = "未分配", Value = "0" });
            a.Add(new SelectListItem { Text = "已分配", Value = "1" });
            a.Add(new SelectListItem { Text = "已完成", Value = "2" });
            a.Add(new SelectListItem { Text = "已通过", Value = "3" });
            a.Add(new SelectListItem { Text = "已废止", Value = "-1" });

            var StatusList = new SelectList(a, "Value", "Text", statusList);
            ViewData["StatusList"] = StatusList;

            List<SelectListItem> b = new List<SelectListItem>();
            b.Add(new SelectListItem { Text = "-请选择有无延期-", Value = "", Selected = true });
            b.Add(new SelectListItem { Text = "有", Value = "有" });
            b.Add(new SelectListItem { Text = "无", Value = "无" });
            ViewData["DelayList"] = new SelectList(b, "Value", "Text", delayList);
            TaskDataContext db = new TaskDataContext();


            DataLoadOptions ds = new DataLoadOptions();
            ds.LoadWith<TODO_Tasks>(t => t.TODO_Task_User);
            //ds.LoadWith<TODO_Tasks>(t => t.TODO_DelayLog);
            ds.LoadWith<TODO_Task_User>(u => u.TODO_Task_User_Node);
            db.LoadOptions = ds;

            List<SelectListItem> c = new List<SelectListItem>();
            var typelist = from t in db.TODO_WorkType select t;
            c.Add(new SelectListItem { Text = "-请选择工作类别-", Value = "", Selected = true });
            foreach (var u in typelist)
            {
                c.Add(new SelectListItem { Text = u.name, Value = u.name });
            }
            //c.Add(new SelectListItem { Text = "信息中心", Value = "信息中心" });
            //c.Add(new SelectListItem { Text = "信息化建设与应用工作", Value = "信息化建设与应用工作" });
            //c.Add(new SelectListItem { Text = "工程建设", Value = "工程建设" });
            //c.Add(new SelectListItem { Text = "安保工作", Value = "安保工作" });
            //c.Add(new SelectListItem { Text = "政工工作", Value = "政工工作" });
            //c.Add(new SelectListItem { Text = "其他工作", Value = "其他工作" });
            ViewData["WorkTypeList"] = new SelectList(c, "Value", "Text", worktypelist);


            var users = from u in db.TODO_Users where u.IsAvailable select u;
            List<SelectListItem> userList = new List<SelectListItem>();
            userList.Add(new SelectListItem { Text = "-请选择责任人-", Value = "0", Selected = true });
            foreach (var u in users)
            {
                userList.Add(new SelectListItem { Text = u.PersonName, Value = u.ID.ToString() });
            }
            ViewData["UserList"] = new SelectList(userList, "Value", "Text", userlist);
            var model = new TaskListModel()
            {
                TaskName = taskName,
                TaskStatus = statusList,
                StatusList = a,
                IsDelay = delayList,
                DelayList = b,
                WorkType = worktypelist,
                WorkTypeList = c,
                User = userlist,
                UserList = userList
            };

            var tasklist = from t in db.TODO_Tasks where t.ParentTaskID == null && t.TaskStatus == 99 select t;
            if (!string.IsNullOrWhiteSpace(taskName))
                tasklist = tasklist.Where(u => u.TaskName.Contains(model.TaskName));

            if (!string.IsNullOrWhiteSpace(worktypelist))
                tasklist = tasklist.Where(u => u.WorkType == model.WorkType);

            if (statusList >= -1)
            {
                var tt = tasklist.ToList().Where(u => u.HasStatus(statusList));
                tasklist = tt.AsQueryable<TODO_Tasks>();
            }


            if (!string.IsNullOrWhiteSpace(delayList))
            {
                var tt = tasklist.ToList().Where(u => u.DelayStr() == delayList);
                tasklist = tt.AsQueryable<TODO_Tasks>();
            }
            if (userlist > 0)
            {
                var tt = tasklist.ToList().Where(u => u.HasUser(userlist));
                tasklist = tt.AsQueryable<TODO_Tasks>();
            }

            model.TaskList = tasklist.ToList<TODO_Tasks>();
            return View(model);

        }

        //
        // GET: /History/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /History/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /History/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        
        //
        // GET: /History/Edit/5
 
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /History/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /History/Delete/5
 
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /History/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
