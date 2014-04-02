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
    using System.Data.Linq;
    [CustomAuthorize]
    public class MyTaskController : BaseController
    {
        //
        // GET: /MyTask/

        public ActionResult Index(string taskName, int statusList = 1, string delayList = "", string worktypelist = "", int userlist = -1)
        {
            TODO_Users user = Session["TODOUser"] as TODO_Users;
            if (userlist == -1)
                userlist = user.ID;
            ViewBag.ActiveType = "MyTask";
            List<SelectListItem> a = new List<SelectListItem>();
            a.Add(new SelectListItem { Text = "---请选择---", Value = "-2", Selected = true });
            a.Add(new SelectListItem { Text = "未完成", Value = "1" });
            a.Add(new SelectListItem { Text = "已完成", Value = "2" });
            a.Add(new SelectListItem { Text = "已废止", Value = "-1" });

            var StatusList = new SelectList(a, "Value", "Text", statusList);
            ViewData["StatusList"] = StatusList;

            List<SelectListItem> b = new List<SelectListItem>();
            b.Add(new SelectListItem { Text = "-请选择有无延期-", Value = "", Selected = true });
            b.Add(new SelectListItem { Text = "有", Value = "有" });
            b.Add(new SelectListItem { Text = "无", Value = "无" });
            ViewData["DelayList"] = new SelectList(b, "Value", "Text", delayList);
            TaskDataContext db = new TaskDataContext();
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

            var model = new  MyTaskListModel()
            {
                Status = statusList,
                TaskName = taskName,
                StatusList = a,

                IsDelay = delayList,
                DelayList = b,
                WorkType = worktypelist,
                WorkTypeList = c,
                User = userlist,
                UserList = userList
            };
            
            var task_list = from u in db.TODO_Task_User select u;
            if (!string.IsNullOrEmpty(taskName))
                task_list = task_list.Where(u => u.TODO_Tasks.TaskName.Contains(model.TaskName));
            if (statusList >= -1)
                task_list = task_list.Where(u => u.Status == statusList);
            if (!string.IsNullOrWhiteSpace(worktypelist))
                task_list = task_list.Where(u => u.TODO_Tasks.WorkType == model.WorkType);

            if (userlist > 0)
            {
                task_list = task_list.Where(u => u.UserID == userlist);
            }
            if (!string.IsNullOrWhiteSpace(delayList))
            {
                var tt = task_list.ToList().Where(u => u.TODO_Tasks.DelayStr() == delayList);
                task_list = tt.AsQueryable<TODO_Task_User>();
            }
            model.TaskList = task_list.ToList<TODO_Task_User>();
            model.UserId = user.ID;
            return View(model);
        }

        public ActionResult Details(int id)
        {
            ViewBag.ActiveType = "MyTask";
            TaskDataContext db = new TaskDataContext();
            DataLoadOptions ds = new DataLoadOptions();
            ds.LoadWith<TODO_Task_User>(t => t.TODO_Tasks);
            ds.LoadWith<TODO_Tasks>(t => t.TODO_TaskNodes);
            ds.LoadWith<TODO_TaskNodes>(n => n.TODO_Task_User_Node);
            db.LoadOptions = ds;
            var task_user = db.TODO_Task_User.SingleOrDefault<TODO_Task_User>(s => s.ID == id);
            if (task_user == null)
                throw new Exception("该任务不存在！");

            InitNodes(db, task_user);
            return View(task_user);
        }

        private void InitNodes(TaskDataContext db, TODO_Task_User task_user)
        {
            var nodelist = (from n in db.TODO_TaskNodes
                            join un in
                                (from u in db.TODO_Task_User_Node where u.Task_User == task_user.ID select u) on n.ID equals un.Task_Node into t
                     from rt in t.DefaultIfEmpty()
                            where n.TaskID == task_user.TODO_Tasks.ID && (rt == null || rt.Task_User == task_user.ID)
                     select new { node = n, user_node = rt }).Select(item => Tuple.Create(item.node, item.user_node));

            //var nodelist = (from un in db.TODO_Task_User_Node
            //                join n in db.TODO_TaskNodes on un.Task_Node equals n.ID into t
            //                from rt in t.DefaultIfEmpty()
            //                where rt.TaskID == task_user.TODO_Tasks.ID && un.Task_User == task_user.ID
            //                select new { node = rt, user_node = un }).Select(item => Tuple.Create(item.node, item.user_node));
            //select Tuple.Create(n, un);
            ViewData["nodelist"] = nodelist;
            //ViewData["nodelist"] = nodelist.ToList<TODO_TaskNodes>();
        }
        //private void InitNodes(TaskDataContext db, TODO_Task_User task_user)
        //{
        //    if (task_user.ComplatedNode != null)
        //    {
        //        var selectnodes = task_user.ComplatedNode.Split('^').ToList<string>();
        //        ViewBag.SelectNodes = selectnodes;
        //    }
        //    var nodelist = from n in db.TODO_TaskNodes where n.TaskID == task_user.TODO_Tasks.ID select n;
        //    ViewData["nodelist"] = nodelist.ToList<TODO_TaskNodes>();
        //}

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
                var user_nodes = (db.TODO_Task_User_Node.Where<TODO_Task_User_Node>(n => n.Task_User==id)).ToList<TODO_Task_User_Node>();
                user_nodes.ForEach(n => { if (n.IsDone < 2)  n.IsDone = 0;  });
                if (nodes != null)
                {
                    nodes.ToList().ForEach(n => { 
                        var user_node = user_nodes.FirstOrDefault(un => un.Task_Node.ToString()==n);
                        if (user_node != null)
                        {
                            user_node.IsDone = 1;
                        }
                        else
                        {
                            user_node = new TODO_Task_User_Node();
                            user_node.Task_Node = Convert.ToInt32(n);
                            user_node.Task_User = id;
                            user_node.IsDone = 1;
                            user_node.DelayCount = user_node.ChangeCount = 0;
                            user_node.ComplateDate = DateTime.Now;
                            user_node.ApprovalDate = null;
                            db.TODO_Task_User_Node.InsertOnSubmit(user_node);
                        }
                    });
                    //user_nodes.Where(n => nodes.Contains<string>(n.id.ToString())).ToList().ForEach(n => n.IsDone = 1);
                    //task_user.ComplatedNode = string.Join("^", nodes);

                }
                db.SubmitChanges();
                if (task_user.TODO_Task_User_Node != null && task_user.TODO_Task_User_Node.Where(n=>n.IsDone>0).Count() == task_user.TODO_Tasks.TODO_TaskNodes.Count)
                {
                    return Done(id);
                }
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
                        var nodes = from node in todo_task.TODO_TaskNodes select node.ID;
                        var user_nodes = (db.TODO_Task_User_Node.Where<TODO_Task_User_Node>(n => n.Task_User == id)).ToList<TODO_Task_User_Node>();
                        nodes.ToList().ForEach(n =>
                        {
                            var user_node = user_nodes.FirstOrDefault(un => un.Task_Node == n);
                            if (user_node != null)
                            {
                                if (user_node.IsDone < 1)
                                    user_node.IsDone = 1;
                            }
                            else
                            {
                                user_node = new TODO_Task_User_Node();
                                user_node.Task_Node = n;
                                user_node.Task_User = id;
                                user_node.IsDone = 1;
                                user_node.DelayCount = user_node.ChangeCount = 0;
                                user_node.ComplateDate = DateTime.Now;
                                user_node.ApprovalDate = null;
                                db.TODO_Task_User_Node.InsertOnSubmit(user_node);
                            }
                        });
                        //task_user.ComplatedNode = task_user.ComplatedNode = string.Join("^", nodes);
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

        public JsonResult GetNodeLogs(int id)
        {
            try
            {
                using (TaskDataContext db = new TaskDataContext())
                {

                    var logs = from l in db.TODO_User_Node_Logs where l.User_Node == id select new { l.LogType, l.Comments, l.CreateBy, CreateDate = l.CreateDate.ToShortDateString() };
                    
                    return Json(new { status = "success", data = logs.ToList() }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", data = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SubmitNode(int id, int nid, int unid, string comment)
        {
            try
            {
                using (TaskDataContext db = new TaskDataContext())
                {
                    var task_user = db.TODO_Task_User.SingleOrDefault<TODO_Task_User>(s => s.ID == id);
                    if (task_user == null)
                        throw new Exception("该任务不存在！");

                    var user_node = db.TODO_Task_User_Node.SingleOrDefault(un => un.id == unid);
                    if (user_node != null)
                    {
                        user_node.IsDone = 1;
                    }
                    else
                    {
                        user_node = new TODO_Task_User_Node();
                        user_node.Task_Node = unid;
                        user_node.Task_User = id;
                        user_node.IsDone = 1;
                        user_node.DelayCount = user_node.ChangeCount = 0;
                        user_node.ComplateDate = DateTime.Now;
                        user_node.ApprovalDate = null;
                        db.TODO_Task_User_Node.InsertOnSubmit(user_node);
                    }

                    var nlog = new TODO_User_Node_Logs();
                    nlog.TODO_Task_User_Node = user_node;
                    nlog.LogType = "完成";
                    nlog.Comments = comment;
                    nlog.CreateBy = (Session["TODOUser"] as TODO_Users).PersonName;
                    nlog.CreateDate = DateTime.Now;
                    db.TODO_User_Node_Logs.InsertOnSubmit(nlog);

                    return Json(new { status = "success", data = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", data = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
