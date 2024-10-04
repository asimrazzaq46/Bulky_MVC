using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class ProductController : Controller
{

    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
    }
    public IActionResult Index()
    {
        List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
        return View(objProductList);
    }


    ///upser means update or insert....with this controller we can create only one page for both functionality..
    ///create the product and update the product
    public IActionResult Upsert(int? id)
    {
        IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(u =>
      new SelectListItem { Text = u.Name, Value = u.Id.ToString() }
      );

        ProductVM productVM = new()
        {
            CategoryList = CategoryList,
            Product = new Product()
        };

        if(id is null || id == 0)
        {
            // Create
        return View(productVM);

        }
        else
        {
            // Update
            productVM.Product = _unitOfWork.Product.GetOne(u => u.Id == id);
            return View(productVM);

        }
    }

    [HttpPost]
    public IActionResult Upsert(ProductVM productVm,IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if(file is not null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\product");

                // If url of image in product is not empty in database
                if (!string.IsNullOrEmpty(productVm.Product.ImageUrl))
                {
                    //Delete the old image
                    var oldImagePath = Path.Combine(wwwRootPath, productVm.Product.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                //// if there was an url image we have delete in above code and we will update the new url in database
                //// if there is not any url of image inside database then we create a new one
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    };

                    productVm.Product.ImageUrl = @"\images\product\" + fileName;
                
            }

            if (productVm.Product.Id==0)
            {

            _unitOfWork.Product.Add(productVm.Product);
            }
            else
            {
                _unitOfWork.Product.update(productVm.Product);

            }
            _unitOfWork.Save();
            TempData["success"] = "Product created Successfully"; 

            return RedirectToAction("Index");

        }
        else
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(u =>
                                new SelectListItem { Text = u.Name, Value = u.Id.ToString() }
                                 );


            productVm.CategoryList = CategoryList;



            return View(productVm);

        }
    }


    //public IActionResult Delete(int? id)
    //{

    //    if (id is null || id == 0)
    //    {
    //        return NotFound();
    //    }

    //    Product obj = _unitOfWork.Product.GetOne(u => u.Id == id);

    //    return View(obj);
    //}

    //[HttpPost]
    //public IActionResult Delete(Product obj)
    //{
    //    _unitOfWork.Product.Remove(obj);
    //    _unitOfWork.Save();
    //    TempData["success"] = "Product deleted Successfully";

    //    return RedirectToAction("Index");
    //}

    #region API CALLS 

    [HttpGet]
    public IActionResult GetAll()
    {
        List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

        return Json(new { data = objProductList });
    }

    [HttpDelete]
    public IActionResult Delete(int? id)
    {

        var wwwRootPath = _webHostEnvironment.WebRootPath;
        var productToBeDeleted = _unitOfWork.Product.GetOne(u => u.Id == id);

        if(productToBeDeleted is null)
        {
            return Json(new { success=false, message="Error While deleteing"});
        }

        var oldImagePath = Path.Combine(wwwRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));
        if (System.IO.File.Exists(oldImagePath))
        {
            System.IO.File.Delete(oldImagePath);
        }


        _unitOfWork.Product.Remove(productToBeDeleted);
        _unitOfWork.Save();

        return Json(new { success = true, message = "deleted successfully" });

    }

    #endregion

}
