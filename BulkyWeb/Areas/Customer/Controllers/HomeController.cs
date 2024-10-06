using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        ///Getting AlL Products
        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties:"Category");
            return View(productList);
        }


        ///Detail of the Product
        public IActionResult Details(int productId)
        {

            ShoppingCart cart = new()
            {
                Product = _unitOfWork.Product.GetOne(u => u.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId,
            };
        
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {

            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            string userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            cart.UserId = userId;

            ShoppingCart shopingCartFromDb = _unitOfWork.ShoppingCart.GetOne(u=>u.UserId == userId && u.ProductId == cart.ProductId);

            if (shopingCartFromDb != null)
            {
                shopingCartFromDb.Count += cart.Count; 
            _unitOfWork.ShoppingCart.Update(shopingCartFromDb);

            }
            else
            {
            _unitOfWork.ShoppingCart.Add(cart);

            }
            _unitOfWork.Save();
            TempData["success"] = "Cart updated successfully";
            return RedirectToAction("Index");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
