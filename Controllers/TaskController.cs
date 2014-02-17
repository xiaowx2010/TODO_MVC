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
    [AdminAuthorize]
    public class TaskController : BaseController
    {
        //
        // GET: /Task/
        public ActionResult Index(string taskName, int statusList = -2)
        {
            ViewBag.ActiveType = "TaskControl";
            List<SelectListItem> a = new List<SelectListItem>();
            a.Add(new SelectListItem { Text = "---请选择---", Value = "-2", Selected = true });
            a.Add(new SelectListItem { Text = "未分配", Value = "0" });
            a.Add(new SelectListItem { Text = "已分配", Value = "1" });
            a.Add(new SelectListItem { Text = "已完成", Value = "2" });
            a.Add(new SelectListItem { Text = "已废止", Value = "-1" });

            var StatusList = new SelectList(a, "Value", "Text", statusList);
            ViewData["StatusList"] = StatusList;
           
            var model = new TaskListModel()
            {
                TaskName = taskName,
                TaskStatus = statusList,
                StatusList = a
            };
            TaskDataContext db = new TaskDataContext();

            var tasklist = from t in db.TODO_Tasks where t.ParentTaskID==null select t;
            if (!string.IsNullOrWhiteSpace(taskName))
                tasklist = tasklist.Where(u => u.TaskName.Contains(model.TaskName));
            if (statusList >= -1)
                tasklist = tasklist.Where(u => u.TaskStatus == model.TaskStatus);
            model.TaskList = tasklist.ToList<TODO_Tasks>();
            return View(model);
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
                if (!string.IsNullOrWhiteSpace(collection["TaskDeadLine"]))
                    newtask.TaskDeadLine = DateTime.Parse(collection["TaskDeadLine"]);
                newtask.TaskRemark = collection["TaskRemark"];
                newtask.TaskStatus = Convert.ToInt32(collection["TaskStatus"]);
                if (newtask.TaskStatus == 1)
                    newtask.TaskAssignDate = DateTime.Now;

                newtask.TaskCreator = "";

                CreateNodes(newtask);
                var executors = Request.Form.GetValues("TaskExecutor");
                if (executors != null && executors.Count() > 0)
                {
                    foreach (var user in executors)
                    {
                        TODO_Task_User task_user = new TODO_Task_User();
                        task_user.ComplatedNode = "";
                        task_user.UserID = Convert.ToInt32(user);
                        task_user.Status = newtask.TaskStatus;
                        task_user.NodesCount = newtask.TODO_TaskNodes.Count;
                        newtask.TODO_Task_User.Add(task_user);

                    }
                }
                TaskDAO.Instance.CreateTask(newtask);

                return RedirectToAction("Index");
            }

            catch (Exception ex)
            {

                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                return View();
            }
            
        }
        public ActionResult Detail(int id)
        {
            ViewBag.ActiveType = "TaskControl";
            TaskDataContext db = new TaskDataContext();
            var task = db.TODO_Tasks.SingleOrDefault<TODO_Tasks>(s => s.ID == id);
            if (task == null)
                throw new Exception("该任务不存在！");

            Init_detail_date(db, task);
            return View(task);

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
                if (!task.TaskAssignDate.HasValue && task.TaskStatus == 1)
                    task.TaskAssignDate = DateTime.Now;

                db.TODO_TaskNodes.DeleteAllOnSubmit(task.TODO_TaskNodes);
                task.TODO_TaskNodes.Clear();
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
                            task_user.ComplatedNode = "";
                            task_user.UserID = uid;
                            task_user.Status = task.TaskStatus;
                            task_user.NodesCount = task.TODO_TaskNodes.Count;
                            task.TODO_Task_User.Add(task_user);
                        }
                        else
                        {
                            var modified_user = task.TODO_Task_User.Single(u => u.UserID == uid);
                            if (modified_user != null)
                            {
                                modified_user.Status = task.TaskStatus;
                                modified_user.NodesCount = task.TODO_TaskNodes.Count;
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
            catch (Exception ex)
            {
                Init_detail_date(db, task);
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
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
            string delmsg = "";
            TaskDataContext db = new TaskDataContext();
            var task = db.TODO_Tasks.SingleOrDefault<TODO_Tasks>(s => s.ID == id);
            try
            {
                if (task == null)
                    throw new Exception("该任务不存在！");
                task.TaskStatus = -1;
                foreach (var t in task.Child_Tasks)
                {
                    foreach (var ta in t.TODO_Task_User)
                    {
                        ta.Status = -1;
                    }
                    t.TaskStatus = -1;
                }
                foreach (var t in task.TODO_Task_User)
                {
                    t.Status = -1;
                }
                db.SubmitChanges();
                //string msg = TaskDAO.Instance.DeleteTask(id);
                //if (msg != "success")
                //{
                //    delmsg = "删除失败！原因：" + msg;
                //}
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                delmsg = "删除失败！原因：" + ex.ToString();
            }
            return RedirectToAction("Index", new { operateMsg = delmsg });

        }

        [HttpPost]
        public ActionResult Delay(FormCollection collection)
        {

            var id = Convert.ToInt32(collection["DelayTask"]);
            var delaydate = Convert.ToDateTime(collection["DelayDate"]);
            var realdelay = Convert.ToInt32(collection["RealDelay"]);
            try
            {
                TaskDataContext db = new TaskDataContext();

                var task = db.TODO_Tasks.SingleOrDefault<TODO_Tasks>(s => s.ID == id);
                DateTime pervDate = task.TaskDeadLine.Value;
                task.TaskDeadLine = delaydate;
                if (realdelay == 1)
                {
                    TODO_DelayLog log = new TODO_DelayLog();

                    log.DelayDate = delaydate;
                    log.PreviousDate = pervDate;
                    log.Reason = collection["Reason"];
                    log.TODO_Tasks = task;
                    log.Creator = (Session["TODOUser"] as TODO_Users).UserName;

                    task.TODO_DelayLog.Add(log);
                }
                db.SubmitChanges();


            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CreateChild(int id, FormCollection collection)
        {

            // TODO: Add creat logic here
            string creatmsg = "";
            int count = Convert.ToInt32(collection["child_count"]);
            try
            {
                string msg = TaskDAO.Instance.CreateChildren(id, count);
                if (msg != "success")
                {
                    creatmsg = "生成子任务失败！原因：" + msg;
                }
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                creatmsg = "生成子任务失败！原因：" + ex.ToString();
            }
            return RedirectToAction("Index", new { operateMsg = creatmsg });

        }
    }
}
