﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TODO.Controllers
{
    using Custom;
    using Models;
    using PersistObject.models;
    [CustomAuthorize]
    public class MyTaskController : BaseController
    {
        //
        // GET: /MyTask/

        public ActionResult Index(string taskName, int statusList = 1)
        {
            ViewBag.ActiveType = "MyTask";
            List<SelectListItem> a = new List<SelectListItem>();
            a.Add(new SelectListItem { Text = "---请选择---", Value = "-2", Selected = true });
            a.Add(new SelectListItem { Text = "未完成", Value = "1" });
            a.Add(new SelectListItem { Text = "已完成", Value = "2" });
            a.Add(new SelectListItem { Text = "已废止", Value = "-1" });

            var StatusList = new SelectList(a, "Value", "Text", statusList);
            ViewData["StatusList"] = StatusList;
            var model = new  MyTaskListModel()
            {
                Status = statusList,
                TaskName = taskName,
                StatusList = a
            };
            TODO_Users user= Session["TODOUser"] as TODO_Users;
            TaskDataContext db = new TaskDataContext();
            var task_list = from u in db.TODO_Task_User select u;
            if (!string.IsNullOrEmpty(taskName))
                task_list = task_list.Where(u => u.TODO_Tasks.TaskName.Contains(model.TaskName));
            if (statusList >= -1)
                task_list = task_list.Where(u => u.Status == statusList);
            model.TaskList = task_list.ToList();
            model.UserId = user.ID;
            return View(model);
        }

        public ActionResult Details(int id)
        {
            ViewBag.ActiveType = "MyTask";
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
        public ActionResult Details(int id, FormCollection collection)
        {
            ViewBag.ActiveType = "TaskControl";

            TaskDataContext db = new TaskDataContext();

            var task_user = db.TODO_Task_User.SingleOrDefault<TODO_Task_User>(s => s.ID == id);
            if (task_user == null)
                throw new Exception("该任务不存在！");
            try
            {
                task_user.ComplatedComments = collection["ComplatedComments"];
                var nodes = Request.Form.GetValues("SelectNode");
                if (nodes != null)
                {
                    task_user.ComplatedNode = string.Join("^", nodes);

                }
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

        public ActionResult Done(int id)
        {

            // TODO: Add delete logic here
            string delmsg = "";
            try
            {
                using (TaskDataContext db = new TaskDataContext())
                {
                    var task_user = db.TODO_Task_User.SingleOrDefault<TODO_Task_User>(s => s.ID == id);
                    task_user.Status = 2;
                    task_user.CompleteDate = DateTime.Now;
                    var todo_task = task_user.TODO_Tasks;
                    if (todo_task.TODO_TaskNodes.Count > 0)
                    {
                        var nodes = from node in todo_task.TODO_TaskNodes select node.NodeNum.ToString();
                        task_user.ComplatedNode = task_user.ComplatedNode = string.Join("^", nodes);
                    }
                    var same_task = from t in todo_task.TODO_Task_User where t.Status != 2 select t;
                    if (same_task.Count()==0)
                        todo_task.TaskStatus = 2;
                    if (todo_task.Parent_Task != null)
                    {
                        var same_todo = from t in todo_task.Parent_Task.Child_Tasks where t.TaskStatus != 2 select t;
                        if (same_todo.Count() == 0)
                            todo_task.Parent_Task.TaskStatus = 2;
                    }
                    db.SubmitChanges();
                }
                
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                delmsg = "完成失败！原因：" + ex.ToString();
            }
            return RedirectToAction("Index", new { operateMsg = delmsg });

        }
    }
}
