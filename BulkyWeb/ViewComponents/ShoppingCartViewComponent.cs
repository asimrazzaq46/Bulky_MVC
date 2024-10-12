using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace BulkyWeb.ViewComponents;

public class ShoppingCartViewComponent :ViewComponent
{
    private readonly IUnitOfWork _unitOfWork;

    public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
        Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        if (claim != null)
        {
            if(HttpContext.Session.GetInt32(SD.Session_Cart) is null)
            {
                HttpContext.Session.SetInt32(SD.Session_Cart,
              _unitOfWork.ShoppingCart.GetAll(u => u.UserId == claim.Value).Count());
            }
          
            return View(HttpContext.Session.GetInt32(SD.Session_Cart));
        }
        else
        {
             HttpContext.Session.Clear();
            return View(0);
        }
    }
}
