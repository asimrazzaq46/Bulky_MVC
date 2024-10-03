using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers;
[Area("Admin")]
public class ProductController : Controller
{

    private readonly IUnitOfWork _unitOfWork;
    public ProductController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public IActionResult Index()
    {
        List<Product> objCategoryList = _unitOfWork.Product.GetAll().ToList();
        return View(objCategoryList);
    }
    
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Product obj)
    {
        if (ModelState.IsValid) { 
        _unitOfWork.Product.Add(obj);
        _unitOfWork.Save();
            TempData["success"] = "Product created Successfully";

            return RedirectToAction("Index");

        }
        return View();
    }

    public IActionResult Update(int? id)
    {

        if (id is null || id == 0)
        {
            return NotFound();
        }

        Product product = _unitOfWork.Product.GetOne(u=> u.Id == id);

        return View(product);
    }

    [HttpPost]
    public IActionResult Update(Product obj)
    {


        if (ModelState.IsValid)
        {
            _unitOfWork.Product.update(obj);
            _unitOfWork.Save();
            TempData["success"] = "Product Updated Successfully";
            return RedirectToAction("Index");

        }
        return View();
    }

    public IActionResult Delete(int? id)
    {

        if(id is null || id == 0)
        {
            return NotFound();
        }

        Product obj = _unitOfWork.Product.GetOne(u => u.Id == id);

        return View(obj);
    }

    [HttpPost]
    public IActionResult Delete(Product obj)
    {
        _unitOfWork.Product.Remove(obj);
        _unitOfWork.Save();
        TempData["success"] = "Product deleted Successfully";

        return RedirectToAction("Index");
    }
}
