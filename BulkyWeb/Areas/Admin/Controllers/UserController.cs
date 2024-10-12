using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class UserController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }
    public IActionResult Index()
    {

        return View();
    }

    [HttpGet]
    public IActionResult ManageRoles(string id)
    {
        string roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == id).RoleId;

        RoleManagmentVM roleVM = new RoleManagmentVM()
        {
            ApplicationUser = _db.ApplicationUsers.Include(u=>u.Company).FirstOrDefault(u=>u.Id == id), 
            RoleList = _db.Roles.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Name
            }),
            CompnayList = _db.Companies.Select(i => new SelectListItem
              {
                  Text = i.Name,
                  Value = i.Id.ToString()
              })
        };

        roleVM.ApplicationUser.role = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;
        
        return View(roleVM);
    }


    [HttpPost]
    public IActionResult ManageRoles(RoleManagmentVM roleVM)
    {
        string roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == roleVM.ApplicationUser.Id).RoleId;

        string oldRole = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;

        if (!(roleVM.ApplicationUser.role == oldRole)) {
            // A role was updated
            ApplicationUser applicationUser = _db.ApplicationUsers.FirstOrDefault(u=> u.Id == roleVM.ApplicationUser.Id);
            if (roleVM.ApplicationUser.role == SD.Role_Company) { 
                    applicationUser.CompanyId = roleVM.ApplicationUser.CompanyId;
            }
            if (oldRole == SD.Role_Company) { 
                applicationUser.CompanyId = null;
            }

            _db.SaveChanges();
            _userManager.RemoveFromRoleAsync(applicationUser,oldRole).GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(applicationUser,roleVM.ApplicationUser.role).GetAwaiter().GetResult();
        }
        

        return RedirectToAction("Index");
    }


    #region API CALLS 

    [HttpGet]
    public IActionResult GetAll()
    {
        List<ApplicationUser> objUserList = _db.ApplicationUsers.Include(u=>u.Company).ToList();

        var userRoles = _db.UserRoles.ToList();
        var roles = _db.Roles.ToList();

        foreach (var user in objUserList)
        {

            var roleId = userRoles.FirstOrDefault(u=>u.UserId == user.Id).RoleId;
            user.role = roles.FirstOrDefault(u=>u.Id== roleId).Name;

            

            if (user.Company is null)
            {
                user.Company = new() { Name = "" };
            }
        }

        return Json(new { data = objUserList });
    }

    [HttpPost]
    public IActionResult LockUnlock([FromBody]string id)
    {

        var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);

        if (objFromDb == null) 
        {
            return Json(new { success = false, message = "Error while locking/unlocking " });
        }

        if (objFromDb.LockoutEnd is not null && objFromDb.LockoutEnd > DateTime.Now)
        { 
            //User is currently locked and we need to unlock the user
            objFromDb.LockoutEnd = DateTime.Now;
        }
        else
        {
            objFromDb.LockoutEnd = DateTime.Now.AddYears(100);
        }
        _db.SaveChanges();
        return Json(new { success = true, message = "Operation successfull" });

    }

    #endregion

}
