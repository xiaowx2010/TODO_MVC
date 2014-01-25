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
    public class UserController : BaseController
    {
        //
        // GET: /User/

        public ActionResult Index(string personName, bool? isAvailable=null)
        {
            ViewBag.ActiveType = "UserControl";
            var model = new UserListModel()
            {
                IsAvailable = isAvailable.HasValue ? isAvailable.Value : false,
                PersonName = personName
            };

            TaskDataContext db = new TaskDataContext();
            var user_list = from u in db.TODO_Users select u;
            if (!string.IsNullOrEmpty(personName))
                user_list = user_list.Where(u => u.PersonName.Contains(model.PersonName));
            if (isAvailable.HasValue && isAvailable.Value)
                user_list = user_list.Where(u => u.IsAvailable == model.IsAvailable);
            model.UserList = user_list.ToList();
            return View(model);
        }


        //
        // GET: /User/Create

        public ActionResult Create(TODO_Users user)
        {
            ViewBag.ActiveType = "UserControl";
            return View(user);
        }

        //
        // POST: /User/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            ViewBag.ActiveType = "UserControl";
            TODO_Users user = new TODO_Users();
            try
            {
                user.UserName = collection["UserName"];
                user.UserRole = Convert.ToInt32(collection["UserRole"]);
                user.PersonName = collection["PersonName"];
                user.Password = collection["Password"];
                user.IsAvailable = collection["IsAvailable"] == "on";
                if (IsExistUsn(collection["UserName"], 0))
                {
                    ViewData["ErrStr"] = "保存失败，已存在相同用户名!";
                    return View(user);
                }
                UserDAO.Instance.CreateUser(user);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                return View(user);
            }
        }

        //
        // GET: /User/Edit/5

        public ActionResult Edit(int id)
        {
            ViewBag.ActiveType = "UserControl";
            TaskDataContext db = new TaskDataContext();
            var user = db.TODO_Users.SingleOrDefault<TODO_Users>(s => s.ID == id);
            if (user == null)
                throw new Exception("该人员不存在！");
            return View(user);
        }

        //
        // POST: /User/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            ViewBag.ActiveType = "UserControl";
            TaskDataContext db = new TaskDataContext();
            var user = db.TODO_Users.SingleOrDefault<TODO_Users>(s => s.ID == id);
            try
            {
                user.UserName = collection["UserName"];
                user.UserRole = Convert.ToInt32(collection["UserRole"]);
                user.PersonName = collection["PersonName"];
                user.Password = collection["Password"];
                user.IsAvailable = collection["IsAvailable"] == "on";
                if (IsExistUsn(collection["UserName"], id))
                {
                    ViewData["ErrStr"] = "保存失败，已存在相同用户名!";
                    return View(user);
                }
                db.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Common.WriteFile(ex.ToString(), Server.MapPath("/") + DateTime.Now.ToString("yyyy-MM-dd") + "_Exception.log");
                return View(user);
            }
        }


        public ActionResult Delete(int id)
        {
            string delmsg = "";
            try
            {
                string msg = UserDAO.Instance.DeleteUser(id);
                if (msg != "success")
                {
                    delmsg = "删除失败！原因：" + msg;
                }
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("DELETE 语句与 REFERENCE 约束"))
                {
                    delmsg = "删除失败！原因： 该用户有任务关联！";
                }
                else
                    delmsg = "删除失败！原因：" + ex.ToString();
            }
            return RedirectToAction("Index", new { operateMsg = delmsg });
        }

        [NonAction]
        public bool IsExistUsn(string usn, int uid)
        {

            TaskDataContext db = new TaskDataContext();

            var users = db.TODO_Users.Where(c => c.UserName == usn);
            if (uid > 0)
            {
                users = users.Where(c => c.ID != uid);
            }
            return users.Count() > 0;

        }
    }
}
