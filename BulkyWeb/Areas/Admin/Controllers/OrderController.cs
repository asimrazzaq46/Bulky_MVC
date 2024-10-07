using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("admin")]
//[Authorize(Roles =SD.Role_Admin)]
public class OrderController : Controller
{
	private readonly IUnitOfWork _unitOfWork;

	public OrderController(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public IActionResult Index()
	{
		return View();
	}


    public IActionResult Detail(int orderId)
    {
        OrderVM orderVm = new()
        {
            OrderHeader = _unitOfWork.OrderHeader.GetOne(u => u.Id == orderId, includeProperties: "User"),
            OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
        };
        return View(orderVm);
    }


    #region API CALLS 

    [HttpGet]
	public IActionResult GetAll(string status)
	{
		IEnumerable<OrderHeader> objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "User").ToList();

        switch (status)
        {
            case "pending":
                objOrderHeaders = objOrderHeaders.Where(u=>u.PaymentStatus == SD.Payment_Status_DelayedPayment);
                break;
            case "inprocess":
                objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.Status_Processing);
                break;
            case "completed":
                objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.Status_Shipped);
                break;
            case "approved":
                objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.Status_Approved);
                break;
            default:
                break;

        }

        return Json(new { data = objOrderHeaders });
	}


	#endregion

}
