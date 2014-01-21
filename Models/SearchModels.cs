using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace TODO.Models
{
    using PersistObject.models;

    //public class ChangePasswordModel
    //{
    //    [Required]
    //    [DataType(DataType.Password)]
    //    [Display(Name = "Current password")]
    //    public string OldPassword { get; set; }

    //    [Required]
    //    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    //    [DataType(DataType.Password)]
    //    [Display(Name = "New password")]
    //    public string NewPassword { get; set; }

    //    [DataType(DataType.Password)]
    //    [Display(Name = "Confirm new password")]
    //    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    //    public string ConfirmPassword { get; set; }
    //}

    //public class LogOnModel
    //{
    //    [Required]
    //    [Display(Name = "User name")]
    //    public string UserName { get; set; }

    //    [Required]
    //    [DataType(DataType.Password)]
    //    [Display(Name = "Password")]
    //    public string Password { get; set; }

    //    [Display(Name = "Remember me?")]
    //    public bool RememberMe { get; set; }
    //}

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
        ///// <summary>
        ///// 查询条件：种族编号
        ///// 用于绑定DepartmentList
        ///// </summary>
        //public int? DepartmentID { get; set; }
        ///// <summary>
        ///// 绑定数据库中的种族集合
        ///// </summary>
        //public IEnumerable<SelectListItem> DepartmentList { get; set; } 
    }
}
