using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TODO.Controllers
{
    using Custom;
    using Models;
    using PersistObject.models;
    using DAO;
    [AdminAuthorize]
    public class TaskMarkingController : Controller
    {
        //
        // GET: /TaskMarking/

        public ActionResult Index(string taskName, int statusList = 0)
        {
            ViewBag.ActiveType = "TaskMarking";
            List<SelectListItem> a = new List<SelectListItem>();
            a.Add(new SelectListItem { Text = "---请选择---", Value = "-1", Selected = true });
            a.Add(new SelectListItem { Text = "未打分", Value = "0" });
            a.Add(new SelectListItem { Text = "已打分", Value = "1" });
            var StatusList = new SelectList(a, "Value", "Text", statusList);
            ViewData["StatusList"] = StatusList;

            var model = new TaskMarkListModel
            {
                TaskName = taskName,
                IsDone = statusList,
                StatusList = a
            };
            TaskDataContext db = new TaskDataContext();

            var tasklist = from t in db.TODO_Task_User where t.Status == 2 select t;
            if (!string.IsNullOrWhiteSpace(taskName))
                tasklist = tasklist.Where(u => u.TODO_Tasks.TaskName.Contains(model.TaskName));
            if (statusList == 1)
                tasklist = tasklist.Where(u => u.Score.HasValue);
            else if (statusList == 0)
                tasklist = tasklist.Where(u => !u.Score.HasValue);
            model.TaskList = tasklist.ToList<TODO_Task_User>();
            return View(model);
        }


        public ActionResult Mark(int id)
        {
            ViewBag.ActiveType = "TaskMarking";
            TaskDataContext db = new TaskDataContext();
            var task_user = db.TODO_Task_User.SingleOrDefault<TODO_Task_User>(s => s.ID == id);
            if (task_user == null)
                throw new Exception("该任务不存在！");

            InitNodes(db, task_user);
            return View(task_user);
        }

        private void InitNodes(TaskDataContext db, TODO_Task_User task_user)
        {
            if (task_user.ComplatedNode != null)
            {
                var selectnodes = task_user.ComplatedNode.Split('^').ToList<string>();
                ViewBag.SelectNodes = selectnodes;
            }
            var nodelist = from n in db.TODO_TaskNodes where n.TaskID == task_user.TODO_Tasks.ID select n;
            ViewData["nodelist"] = nodelist.ToList<TODO_TaskNodes>();
        }

        [HttpPost]
        public ActionResult Mark(int id, FormCollection collection)
        {
            ViewBag.ActiveType = "TaskMarking";

            TaskDataContext db = new TaskDataContext();

            var task_user = db.TODO_Task_User.SingleOrDefault<TODO_Task_User>(s => s.ID == id);
            if (task_user == null)
                throw new Exception("该任务不存在！");
            try
            {
                task_user.Score = Convert.ToDouble(collection["Score"]);
                task_user.LeaderComments = collection["LeaderComments"];
                db.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                InitNodes(db, task_user);
                return View(task_user);
            }
        }
    }
}
