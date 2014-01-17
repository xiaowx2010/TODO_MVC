using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Transactions;

namespace TODO.Controllers
{
    using PersistObject.models;
    using DAO;
    public class TaskController : Controller
    {
        //
        // GET: /Task/
        public ActionResult Index()
        {
            ViewBag.ActiveType = "TaskControl";
            TaskDataContext db = new TaskDataContext();

            var tasklist = from t in db.TODO_Tasks select t;
            
            return View(tasklist.ToList<TODO_Tasks>());
        }

        //
        // GET: /Task/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Task/Create

        public ActionResult Create()
        {
            ViewBag.ActiveType = "TaskControl";

            TaskDataContext db = new TaskDataContext();
            var userlist = from u in db.TODO_Users where u.IsAvailable select u;
            return View(userlist.ToList<TODO_Users>());
        } 

        //
        // POST: /Task/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            ViewBag.ActiveType = "TaskControl";
            try
            {
                TODO_Tasks newtask = new TODO_Tasks();
                newtask.TaskName = collection["TaskName"];
                newtask.Priority = Convert.ToInt32(collection["Priority"]);
                newtask.TaskType = collection["TaskType"];
                newtask.TaskDeadLine = DateTime.Parse(collection["TaskDeadLine"]);
                newtask.TaskRemark = collection["TaskRemark"];

                newtask.TaskCreator = "";

                var nodesname = Request.Form.GetValues("NodeRemark").ToList<string>();
                if (nodesname.Count > 0)
                {
                    int num = 1;
                    foreach (var name in nodesname)
                    {
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            TODO_TaskNodes node = new TODO_TaskNodes();
                            node.NodeName = name;
                            node.NodeNum = num++;
                            node.TODO_Tasks = newtask;
                            newtask.TODO_TaskNodes.Add(node);
                        }
                    }

                }
                var executors = Request.Form.GetValues("TaskExecutor").ToList<string>();
                if (executors.Count > 0)
                {
                    foreach (var user in executors)
                    {
                        TODO_Task_User task_user = new TODO_Task_User();
                        task_user.UserID = Convert.ToInt32(user);
                        newtask.TODO_Task_User.Add(task_user);

                    }
                }
                TaskDAO.Instance.CreateTask(newtask);

                return RedirectToAction("Index");
            }

            catch
            {
                return View();
            }
            
        }
        
        //
        // GET: /Task/Edit/5
 
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Task/Edit/5

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

        public ActionResult Delete(int id)
        {

            // TODO: Add delete logic here
            string delmsg = "";
            try
            {
                string msg = TaskDAO.Instance.DeleteTask(id);
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
