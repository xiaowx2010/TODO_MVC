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
                newtask.TaskStatus = Convert.ToInt32(collection["TaskStatus"]);

                newtask.TaskCreator = "";

                CreateNodes(newtask);
                var executors = Request.Form.GetValues("TaskExecutor");
                if (executors != null && executors.Count() > 0)
                {
                    foreach (var user in executors)
                    {
                        TODO_Task_User task_user = new TODO_Task_User();
                        task_user.UserID = Convert.ToInt32(user);
                        task_user.IsAssigned = newtask.TaskStatus==1;
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
            ViewBag.ActiveType = "TaskControl";

            TaskDataContext db = new TaskDataContext();
            var task = db.TODO_Tasks.SingleOrDefault<TODO_Tasks>(s => s.ID == id);
            if (task == null)
                throw new Exception("该任务不存在！");
            Init_detail_date(db, task);
            return View(task);
        }

        private void Init_detail_date(TaskDataContext db, TODO_Tasks task)
        {
            var userlist = from u in db.TODO_Users where u.IsAvailable select u;

            var selectUser = from u in db.TODO_Task_User where u.TaskID == task.ID select u.UserID;
            var selectList = new MultiSelectList(userlist, "ID", "PersonName", selectUser);
            var nodelist = from n in db.TODO_TaskNodes where n.TaskID == task.ID select n;
            ViewData["TaskExecutor"] = selectList;
            ViewData["nodelist"] = nodelist.ToList<TODO_TaskNodes>();
        }

        //
        // POST: /Task/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            ViewBag.ActiveType = "TaskControl";

            TaskDataContext db = new TaskDataContext();
            var task = db.TODO_Tasks.SingleOrDefault<TODO_Tasks>(s => s.ID == id);
            try
            {
                if (task == null)
                    throw new Exception("该任务不存在！");

                task.TaskName = collection["TaskName"];
                task.Priority = Convert.ToInt32(collection["Priority"]);
                task.TaskType = collection["TaskType"];
                task.TaskDeadLine = DateTime.Parse(collection["TaskDeadLine"]);
                task.TaskRemark = collection["TaskRemark"];
                task.TaskStatus = Convert.ToInt32(collection["TaskStatus"]);

                db.TODO_TaskNodes.DeleteAllOnSubmit(task.TODO_TaskNodes);
                CreateNodes(task);
                
                var now_executors = Request.Form.GetValues("TaskExecutor");
                if (now_executors != null && now_executors.Count() > 0)
                {
                    var all_users = from u in db.TODO_Task_User select u;
                    var previous_executor_ids = (from u in all_users where u.TaskID == task.ID select u.UserID).ToList<int>();
                    foreach (var user in now_executors)
                    {
                        var uid = Convert.ToInt32(user);
                        if (!previous_executor_ids.Contains(uid))
                        {
                            TODO_Task_User task_user = new TODO_Task_User();
                            task_user.UserID = uid;
                            task.TODO_Task_User.Add(task_user);
                            task_user.IsAssigned = task.TaskStatus == 1;
                        }
                        else
                        {
                            var modified_user = task.TODO_Task_User.Single(u => u.UserID == uid);
                            if (modified_user != null)
                            {
                                modified_user.IsAssigned = task.TaskStatus == 1;
                            }
                        }
                    }
                    foreach (var user in previous_executor_ids)
                    {
                        if (!now_executors.Contains(user.ToString()))
                        {
                            var rm_user = task.TODO_Task_User.Single(u => u.UserID == user);
                            if (rm_user != null)
                            {
                                db.TODO_Task_User.DeleteOnSubmit(rm_user);
                            }
                        }
                    }
                }
                else
                {
                    db.TODO_Task_User.DeleteAllOnSubmit(task.TODO_Task_User);
                }
                db.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                Init_detail_date(db, task);
                return View(task);
            }
        }

        private void CreateNodes(TODO_Tasks task)
        {
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
                        node.TODO_Tasks = task;
                        task.TODO_TaskNodes.Add(node);
                    }
                }

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

        [HttpPost]
        public ActionResult Delay(FormCollection collection)
        {

            // TODO: Add delete logic here
            var id = Convert.ToInt32(collection["DelayTask"]);
            TaskDataContext db = new TaskDataContext();
            var task = db.TODO_Tasks.SingleOrDefault<TODO_Tasks>(s => s.ID == id);
            task.TaskDeadLine = Convert.ToDateTime(collection["DelayDate"]);
            db.SubmitChanges();
            return RedirectToAction("Index");

        }
    }
}
