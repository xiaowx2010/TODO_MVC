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
    public class TaskController : BaseController
    {
        //
        // GET: /Task/
        public ActionResult Index(string taskName, int statusList = -2, string delayList = "", string worktypelist = "", int userlist = 0)
        {
            ViewBag.ActiveType = "TaskControl";
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

            var tasklist = from t in db.TODO_Tasks where t.ParentTaskID == null && t.TaskStatus<99 select t;
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
        // GET: /Task/Create

        public ActionResult Create()
        {
            ViewBag.ActiveType = "TaskControl";

            TaskDataContext db = new TaskDataContext();
            var userlist = from u in db.TODO_Users where u.IsAvailable select u;

            List<SelectListItem> c = new List<SelectListItem>();
            var typelist = from t in db.TODO_WorkType select t;
            c.Add(new SelectListItem { Text = "-请选择工作类别-", Value = "", Selected = true });
            foreach (var u in typelist)
            {
                c.Add(new SelectListItem { Text = u.name, Value = u.name });
            }
            ViewData["WorkType"] = new SelectList(c, "Value", "Text", "");

            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new SelectListItem { Text = "-请选择汇报领导-", Value = "", Selected = true });
            userlist.Where(u => u.UserRole == 1).ToList().ForEach(leader => {
                l.Add(new SelectListItem { Text = leader.PersonName, Value = leader.PersonName });
            });
            ViewData["LeaderList"] = new SelectList(l, "Value", "Text", "");

            List<SelectListItem> p = new List<SelectListItem>();
            var plist = from ps in db.TODO_Priority select ps;
            foreach (var i in plist)
            {
                p.Add(new SelectListItem { Text = i.priority.ToString() + '-' + i.name, Value = i.id.ToString() });
            }
            ViewData["PriorityType"] = new SelectList(p, "Value", "Text", "");


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
                newtask.WorkType = collection["WorkType"];
                newtask.Leader = collection["LeaderList"];
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
        public ActionResult Detail(int id, int IsHistory = 0)
        {
            if (IsHistory == 0)
                ViewBag.ActiveType = "TaskControl";
            else
                ViewBag.ActiveType = "HistoryTask";
            TaskDataContext db = new TaskDataContext();
            DataLoadOptions ds = new DataLoadOptions();
            ds.LoadWith<TODO_Tasks>(t => t.TODO_TaskNodes);
            ds.LoadWith<TODO_TaskNodes>(n => n.TODO_Task_User_Node);
            db.LoadOptions = ds;
            var task = db.TODO_Tasks.SingleOrDefault<TODO_Tasks>(s => s.ID == id);
            if (task == null)
                throw new Exception("该任务不存在！");

            //Init_detail_date(db, task);
            return View(task);

        }
        public JsonResult GetNodeInfo(int task_user_id, int node_id)
        {
            try
            {
                using (TaskDataContext db = new TaskDataContext())
                {
                    var user_node = db.TODO_Task_User_Node.SingleOrDefault<TODO_Task_User_Node>(n => n.Task_User == task_user_id && n.Task_Node == node_id);
                    if (user_node == null)
                        return Json(new { status = "error", data = "获取节点数据失败，请刷新页面！" }, JsonRequestBehavior.AllowGet);

                    return Json(new
                    {
                        status = "success",
                        data = new
                        {
                            status = user_node.GetUserNodeStatusStr(),
                            cdate = user_node.ComplateDate.HasValue ? user_node.ComplateDate.Value.ToShortDateString() : "",
                            adate = user_node.ApprovalDate.HasValue ? user_node.ApprovalDate.Value.ToShortDateString() : "",
                            ccount = user_node.ChangeCount,
                            dcount = user_node.DelayCount,
                            mark = user_node.Mark
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", data = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetLastNodeLog(int task_user_id, int node_id)
        {
            try
            {
                using (TaskDataContext db = new TaskDataContext())
                {
                    var user_node = db.TODO_Task_User_Node.SingleOrDefault<TODO_Task_User_Node>(n => n.Task_User == task_user_id && n.Task_Node == node_id);
                    if (user_node == null)
                        return Json(new { status = "success", data = "" }, JsonRequestBehavior.AllowGet);

                    //var logs = from l in db.TODO_User_Node_Logs where l.TODO_Task_User_Node == user_node select new { l.LogType, l.Comments, l.CreateBy, CreateDate = l.CreateDate.ToShortDateString() };
                    var log = db.TODO_User_Node_Logs
                        .OrderByDescending(x => x.CreateDate)
                        .Where(x => x.User_Node == user_node.id)
                        .First();
                    //var last_log = logs.Last();
                    return Json(new { status = "success", data = new { log.LogType, log.Comments, log.CreateBy, CreateDate = log.CreateDate.ToShortDateString(), Color=log.TypeColor()} }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", data = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult RejectNode(int id, string comment)
        {
            try
            {
                using (TaskDataContext db = new TaskDataContext())
                {
                    var user_node = db.TODO_Task_User_Node.SingleOrDefault<TODO_Task_User_Node>(n => n.id == id);
                    if(user_node==null)
                        throw new Exception("没找到这个节点");

                    user_node.IsDone = 0;
                    user_node.TODO_Task_User.Status = 1;
                    user_node.TODO_Task_User.TODO_Tasks.TaskStatus = 1;
                    if (user_node.TODO_Task_User.TODO_Tasks.Parent_Task != null)
                        user_node.TODO_Task_User.TODO_Tasks.Parent_Task.TaskStatus = 1;

                    var log = new TODO_User_Node_Logs();
                    log.TODO_Task_User_Node = user_node;
                    log.LogType = "退回";
                    log.Comments = comment;
                    log.CreateBy = (Session["TODOUser"] as TODO_Users).PersonName;
                    log.CreateDate = DateTime.Now;

                    db.TODO_User_Node_Logs.InsertOnSubmit(log);

                    db.SubmitChanges();

                    return Json(new { status = "success", data = "" }, JsonRequestBehavior.DenyGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", data = ex.Message });
            }
        }
        public JsonResult DenyNode(int task_user_id, int node_id, string comment)
        {
            try
            {
                using (TaskDataContext db = new TaskDataContext())
                {
                    var user_node = db.TODO_Task_User_Node.SingleOrDefault<TODO_Task_User_Node>(n => n.Task_User == task_user_id && n.Task_Node == node_id);
                    if (user_node == null)
                        throw new Exception("没找到这个节点");

                    user_node.IsDone = 0;
                    user_node.Mark = 0;
                    user_node.TODO_Task_User.Status = 1;
                    user_node.TODO_Task_User.TODO_Tasks.TaskStatus = 1;
                    if (user_node.TODO_Task_User.TODO_Tasks.Parent_Task != null)
                        user_node.TODO_Task_User.TODO_Tasks.Parent_Task.TaskStatus = 1;

                    var log = new TODO_User_Node_Logs();
                    log.TODO_Task_User_Node = user_node;
                    log.LogType = "驳回";
                    log.Comments = comment;
                    log.CreateBy = (Session["TODOUser"] as TODO_Users).PersonName;
                    log.CreateDate = DateTime.Now;
                    db.TODO_User_Node_Logs.InsertOnSubmit(log);

                    db.SubmitChanges();

                    return Json(new { status = "success", data = "" }, JsonRequestBehavior.DenyGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", data = ex.Message });
            }
        }
        public JsonResult ApproveNode(int task_user_id, int node_id, int mark, string comment)
        {
            try
            {
                using (TaskDataContext db = new TaskDataContext())
                {
                    var user_node = db.TODO_Task_User_Node.SingleOrDefault<TODO_Task_User_Node>(n => n.Task_User == task_user_id && n.Task_Node == node_id);
                    if (user_node == null)
                        throw new Exception("没找到这个节点");

                    user_node.IsDone = 2;
                    user_node.ApprovalDate = DateTime.Now;
                    user_node.Mark = mark;

                    var log = new TODO_User_Node_Logs();
                    log.TODO_Task_User_Node = user_node;
                    log.LogType = "通过";
                    log.Comments = comment;
                    log.CreateBy = (Session["TODOUser"] as TODO_Users).PersonName;
                    log.CreateDate = DateTime.Now;
                    db.TODO_User_Node_Logs.InsertOnSubmit(log);

                    db.SubmitChanges();

                    var task_user = user_node.TODO_Task_User;
                    task_user.Status = 3;
                    var todo_task = task_user.TODO_Tasks;
                    if (task_user.TODO_Task_User_Node.Where(n => n.IsDone == 2).Count() == todo_task.TODO_TaskNodes.Count)
                    {
                        var same_task = from t in todo_task.TODO_Task_User where t.Status != 3 select t;
                        if (same_task.Count() == 0)
                            todo_task.TaskStatus = 3;
                        if (todo_task.Parent_Task != null)
                        {
                            var same_todo = from t in todo_task.Parent_Task.Child_Tasks where t.TaskStatus != 3 select t;
                            if (same_todo.Count() == 0)
                                todo_task.Parent_Task.TaskStatus = 3;
                        }
                        db.SubmitChanges();

                    }
                    
                    return Json(new { status = "success", data = "" }, JsonRequestBehavior.DenyGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", data = ex.Message });
            }
        }
        public JsonResult AjaxDelay(int task_user_id, int node_id, string delaydate, string reason, bool realdelay)
        {
            try
            {
                using (TaskDataContext db = new TaskDataContext())
                {
                    var user_node = db.TODO_Task_User_Node.SingleOrDefault<TODO_Task_User_Node>(n => n.Task_User == task_user_id && n.Task_Node == node_id);
                    if (user_node == null)
                    {
                        user_node = new TODO_Task_User_Node();
                        user_node.Task_Node = node_id;
                        user_node.Task_User = task_user_id;
                        user_node.IsDone = 0;
                        user_node.DelayCount = user_node.ChangeCount = 0;
                        user_node.ComplateDate = null;
                        user_node.ApprovalDate = null;
                        db.TODO_Task_User_Node.InsertOnSubmit(user_node);
                        db.SubmitChanges();
                    }

                    var ddate = Convert.ToDateTime(delaydate);
                    var task = user_node.TODO_Task_User.TODO_Tasks;
                    DateTime pervDate = task.TaskDeadLine.Value;
                    task.TaskDeadLine = ddate;
                    int delaytype = realdelay ? 1 : 0;
                   
                    TODO_DelayLog log = new TODO_DelayLog();

                    log.DelayDate = ddate;
                    log.PreviousDate = pervDate;
                    log.Reason = reason;
                    log.TODO_Tasks = task;
                    log.Creator = (Session["TODOUser"] as TODO_Users).UserName;
                    log.DelayType = delaytype;
                    task.TODO_DelayLog.Add(log);
                     
                    
                    string LogType = "";
                    if (realdelay)
                    {
                        LogType = "延期";
                        user_node.DelayCount = user_node.DelayCount + 1;
                    }
                    else
                    {
                        LogType = "续期";
                        user_node.ChangeCount = user_node.ChangeCount + 1;
                    }

                    
                    var nlog = new TODO_User_Node_Logs();
                    nlog.TODO_Task_User_Node = user_node;
                    nlog.LogType = LogType;
                    nlog.Comments = reason;
                    nlog.CreateBy = (Session["TODOUser"] as TODO_Users).PersonName;
                    nlog.CreateDate = DateTime.Now;
                    db.TODO_User_Node_Logs.InsertOnSubmit(nlog);

                    if (task.Parent_Task != null && task.TaskDeadLine.HasValue && task.TaskDeadLine.Value > task.TaskDeadLine.Value)
                        task.Parent_Task.TaskDeadLine = task.TaskDeadLine.Value;
                    db.SubmitChanges();

                    return Json(new { status = "success", data = "" }, JsonRequestBehavior.DenyGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", data = ex.Message });
            }
        }

        public JsonResult GetDescList(string startsWith, string type)
        {
            try
            {
                using (TaskDataContext db = new TaskDataContext())
                {
                    var desclist = db.TODO_Desc.Where<TODO_Desc>(d => d.in_use && d.type == type && d.description.Contains(startsWith)).Select(d => d.description).ToList<string>();
                    return Json(desclist, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", data = ex.Message }, JsonRequestBehavior.AllowGet);
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

            List<SelectListItem> c = new List<SelectListItem>();
            var typelist = from t in db.TODO_WorkType select t;
            c.Add(new SelectListItem { Text = "-请选择工作类别-", Value = "", Selected = true });
            foreach (var u in typelist)
            {
                c.Add(new SelectListItem { Text = u.name, Value = u.name });
            }
            ViewData["WorkType"] = new SelectList(c, "Value", "Text", task.WorkType);

            List<SelectListItem> p = new List<SelectListItem>();
            var plist = from ps in db.TODO_Priority select ps;
            foreach (var i in plist)
            {
                p.Add(new SelectListItem { Text = i.priority.ToString() + '-' + i.name, Value = i.id.ToString() });
            }
            ViewData["PriorityType"] = new SelectList(p, "Value", "Text", task.Priority);

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


            List<SelectListItem> l = new List<SelectListItem>();
            l.Add(new SelectListItem { Text = "-请选择汇报领导-", Value = "" });
            userlist.Where(u => u.UserRole == 1).ToList().ForEach(leader =>
            {
                l.Add(new SelectListItem { Text = leader.PersonName, Value = leader.PersonName });
            });
            ViewData["LeaderList"] = new SelectList(l, "Value", "Text", task.Leader);
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
                task.WorkType = collection["WorkType"];
                task.TaskDeadLine = DateTime.Parse(collection["TaskDeadLine"]);
                task.TaskRemark = collection["TaskRemark"];
                task.TaskStatus = Convert.ToInt32(collection["TaskStatus"]);
                task.Leader = collection["LeaderList"];
                if (!task.TaskAssignDate.HasValue && task.TaskStatus == 1){
                    task.TaskAssignDate = DateTime.Now;
                    if (task.Parent_Task != null)
                    {
                        task.Parent_Task.TaskStatus = 1;
                    }
                }
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
                if (task.Parent_Task != null && task.TaskDeadLine.HasValue && task.TaskDeadLine.Value > task.TaskDeadLine.Value)
                    task.Parent_Task.TaskDeadLine = task.TaskDeadLine.Value;
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

            if (task.TaskStatus == 1 && task.TODO_TaskNodes.Count == 0)
            {
                TODO_TaskNodes node = new TODO_TaskNodes();
                node.NodeName = task.TaskRemark;
                node.NodeNum = 1;
                node.TODO_Tasks = task;
                task.TODO_TaskNodes.Add(node);
            }
        }
        public ActionResult Copy(int id)
        {
            string copymsg = "";
            try
            {
                string msg = TaskDAO.Instance.Copy(id);
                if (msg != "success")
                {
                    copymsg = "生成子任务失败！原因：" + msg;
                }
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                copymsg = "复制失败！原因：" + ex.ToString();
            }
            return RedirectToAction("Index", new { operateMsg = copymsg });
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
                task.DeleteDate = DateTime.Now;
                foreach (var t in task.Child_Tasks)
                {
                    foreach (var ta in t.TODO_Task_User)
                    {
                        ta.Status = -1;
                    }
                    t.TaskStatus = -1;
                    t.DeleteDate = DateTime.Now;
                }
                foreach (var t in task.TODO_Task_User)
                {
                    t.Status = -1;
                }
                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                delmsg = "删除失败！原因：" + ex.ToString();
            }
            return RedirectToAction("Index", new { operateMsg = delmsg });

        }

        public ActionResult Archive(int id)
        {
            string delmsg = "";
            TaskDataContext db = new TaskDataContext();
            var task = db.TODO_Tasks.SingleOrDefault<TODO_Tasks>(s => s.ID == id);
            try
            {
                if (task == null)
                    throw new Exception("该任务不存在！");
                task.TaskStatus = 99;
                task.ArchiveDate = DateTime.Now;
                foreach (var t in task.Child_Tasks)
                {
                    foreach (var ta in t.TODO_Task_User)
                    {
                        ta.Status = 99;
                    }
                    t.TaskStatus = 99;
                    t.ArchiveDate = DateTime.Now;
                }
                foreach (var t in task.TODO_Task_User)
                {
                    t.Status = 99;
                }
                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                delmsg = "归档失败！原因：" + ex.ToString();
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
                //if (realdelay == 1)
                //{
                    TODO_DelayLog log = new TODO_DelayLog();

                    log.DelayDate = delaydate;
                    log.PreviousDate = pervDate;
                    log.Reason = collection["Reason"];
                    log.TODO_Tasks = task;
                    log.Creator = (Session["TODOUser"] as TODO_Users).UserName;
                    log.DelayType = realdelay;
                    task.TODO_DelayLog.Add(log);
                //}
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
