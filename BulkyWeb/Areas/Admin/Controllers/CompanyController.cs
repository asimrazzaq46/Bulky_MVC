using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class CompanyController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public CompanyController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        
    }
    public IActionResult Index()
    {
        List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
        return View(objCompanyList);
    }


    ///upser means update or insert....with this controller we can create only one page for both functionality..
    ///create the Company and update the Company
    public IActionResult Upsert(int? id)
    {
     
        if (id is null || id == 0)
        {
            // Create
            return View(new Company());

        }
        else
        {
            // Update
             Company companyObj = _unitOfWork.Company.GetOne(u => u.Id == id);
            return View(companyObj);

        }
    }

    [HttpPost]
    public IActionResult Upsert(Company companyobj)
    {
        if (ModelState.IsValid)
        {
           

            if (companyobj.Id == 0)
            {

                _unitOfWork.Company.Add(companyobj);
            }
            else
            {
                _unitOfWork.Company.Update(companyobj);
                   
            }
            _unitOfWork.Save();
            TempData["success"] = "Company created Successfully";

            return RedirectToAction("Index");

        }
        else
        {
            return View(companyobj);

        }
    }



    #region API CALLS 

    [HttpGet]
    public IActionResult GetAll()
    {
        List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();

        return Json(new { data = objCompanyList });
    }

    [HttpDelete]
    public IActionResult Delete(int? id)
    {

        var CompanyToBeDeleted = _unitOfWork.Company.GetOne(u => u.Id == id);

        if (CompanyToBeDeleted is null)
        {
            return Json(new { success = false, message = "Error While deleteing" });
        }

        _unitOfWork.Company.Remove(CompanyToBeDeleted);
        _unitOfWork.Save();

        return Json(new { success = true, message = "deleted successfully" });

    }

    #endregion

}
