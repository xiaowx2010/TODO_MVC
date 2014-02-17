using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace TODO.Models
{
    using PersistObject.models;

    public class UserListModel
    {
        /// <summary>
        /// 查询条件：姓名
        /// </summary>
        public string PersonName { get; set; }
        /// <summary>
        /// 查询条件：是否启用
        /// </summary>
        public bool IsAvailable { get; set; }
        /// <summary>
        /// 绑定列表的员工集合
        /// </summary>
        public List<TODO_Users> UserList { get; set; } 
        
    }
    public class MyTaskListModel
    {
        /// <summary>
        /// 查询条件：任务名
        /// </summary>
        public string TaskName { get; set; }
        /// <summary>
        /// 查询条件：是否完成
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 绑定列表的员工集合
        /// </summary>
        public List<TODO_Task_User> TaskList { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
        public int UserId { get; set; }
       
    }
    public class TaskListModel
    {
        /// <summary>
        /// 查询条件：任务名
        /// </summary>
        public string TaskName { get; set; }
        /// <summary>
        /// 查询条件：任务状态
        /// </summary>
        public int TaskStatus { get; set; }
        /// <summary>
        /// 绑定列表的任务集合
        /// </summary>
        public List<TODO_Tasks> TaskList { get; set; }

        public IEnumerable<SelectListItem> StatusList { get; set; } 
    }
    public class TaskMarkListModel
    {
        /// <summary>
        /// 查询条件：任务名
        /// </summary>
        public string TaskName { get; set; }
        /// <summary>
        /// 查询条件：是否完成
        /// </summary>
        public int IsDone { get; set; }
        /// <summary>
        /// 绑定列表的员工集合
        /// </summary>
        public List<TODO_Task_User> TaskList { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; } 

    }
}
