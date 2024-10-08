using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("admin")]
[Authorize]
public class OrderController : Controller
{
	private readonly IUnitOfWork _unitOfWork;

    [BindProperty]
	public OrderVM OrderVm { get; set; }

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
        OrderVm = new()
        {
            OrderHeader = _unitOfWork.OrderHeader.GetOne(u => u.Id == orderId, includeProperties: "User"),
            OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
        };
        return View(OrderVm);
    }

    [ActionName("Detail")]
    [HttpPost]
    public IActionResult Detail_Pay_Now()
    {

        OrderHeader orderHeader = _unitOfWork.OrderHeader.GetOne(u => u.Id == OrderVm.OrderHeader.Id,includeProperties:"User");
        IEnumerable<OrderDetail> orderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == OrderVm.OrderHeader.Id, includeProperties: "Product");

        string domain = "https://localhost:7211";

        var options = new Stripe.Checkout.SessionCreateOptions
        {
            SuccessUrl = domain + $"/admin/order/PaymentConfirmation?orderHeaderId={OrderVm.OrderHeader.Id}",
            CancelUrl = domain + $"/admin/order/detail?orderId={OrderVm.OrderHeader.Id}",
            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
            Mode = "payment",
        };

        foreach (var item in orderDetail)
        {
            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product.Title
                    },
                },
                Quantity = item.Count
            };

            options.LineItems.Add(sessionLineItem);
        }

        var service = new Stripe.Checkout.SessionService();
        Session session = service.Create(options);
        _unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVm.OrderHeader.Id, session.Id, session.PaymentIntentId);
        _unitOfWork.Save();
        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);


    }

    [HttpPost]
    [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
    public IActionResult UpdateOrderDetail()
    {
        var orderHeaderFromDb = _unitOfWork.OrderHeader.GetOne(u => u.Id == OrderVm.OrderHeader.Id);
        orderHeaderFromDb.Name = OrderVm.OrderHeader.Name;
        orderHeaderFromDb.PhoneNumber = OrderVm.OrderHeader.PhoneNumber;
        orderHeaderFromDb.StrretAddress = OrderVm.OrderHeader.StrretAddress;
        orderHeaderFromDb.City = OrderVm.OrderHeader.City;
        orderHeaderFromDb.State = OrderVm.OrderHeader.State;
        orderHeaderFromDb.PostalCode = OrderVm.OrderHeader.PostalCode;
        if (!String.IsNullOrEmpty(OrderVm.OrderHeader.Carrier))
        {
            orderHeaderFromDb.Carrier = OrderVm.OrderHeader.Carrier;
        }
        if (!String.IsNullOrEmpty(OrderVm.OrderHeader.TrackingNumber))
        {
            orderHeaderFromDb.Carrier = OrderVm.OrderHeader.TrackingNumber;
        }
        _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
        _unitOfWork.Save();
        TempData["success"] = "Order Updated Successfully";

        return RedirectToAction(nameof(Detail),new { orderId = orderHeaderFromDb.Id});
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult StartProcessing()
    {
        _unitOfWork.OrderHeader.UpdateStatus(OrderVm.OrderHeader.Id,SD.Status_Processing);
        _unitOfWork.Save();
        TempData["success"] = "Order Updated Successfully";

        return RedirectToAction(nameof(Detail), new { orderId = OrderVm.OrderHeader.Id });
    }


    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult ShipOrder()
    {

        OrderHeader orderHeader = _unitOfWork.OrderHeader.GetOne(u=>u.Id == OrderVm.OrderHeader.Id);

        orderHeader.Carrier = OrderVm.OrderHeader.Carrier;
        orderHeader.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
        orderHeader.OrderStatus =SD.Status_Shipped;
        orderHeader.ShippingDate = DateTime.Now;
        if(orderHeader.PaymentStatus == SD.Payment_Status_DelayedPayment)
        {
            orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
        }

        _unitOfWork.OrderHeader.Update(orderHeader);
        _unitOfWork.Save();
        TempData["success"] = "Order Shipped Successfully";

        return RedirectToAction(nameof(Detail), new { orderId = OrderVm.OrderHeader.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult CancelOrder()
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeader.GetOne(u => u.Id == OrderVm.OrderHeader.Id);

        if (orderHeader.PaymentStatus == SD.Payment_Status_Approved) {

            var option = new RefundCreateOptions
            {
                Reason = RefundReasons.RequestedByCustomer,
                PaymentIntent = orderHeader.PaymentIntentId,
            };

            RefundService service = new RefundService();
            Refund refund = service.Create(option);

            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.Status_Cancelled, SD.Status_Refunded);
        }
        else
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.Status_Cancelled, SD.Status_Cancelled);

        }

        _unitOfWork.Save();
        TempData["success"] = "Order Cancelled Successfully";



        return RedirectToAction(nameof(Detail), new { orderId = OrderVm.OrderHeader.Id });

    }
    //PaymentConfirmation

    public IActionResult PaymentConfirmation(int orderHeaderId)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeader.GetOne(u => u.Id == orderHeaderId);
        if (orderHeader.PaymentStatus == SD.Payment_Status_DelayedPayment)
        {
            //This is an order by a company
            SessionService service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if(session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId,session.Id,session.PaymentIntentId);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId,orderHeader.OrderStatus,SD.Payment_Status_Approved);
                _unitOfWork.Save();
            }

        }

            return View(orderHeaderId);
    }

    #region API CALLS 

    [HttpGet]
    [Authorize]
	public IActionResult GetAll(string status)
	{
        IEnumerable<OrderHeader> objOrderHeaders;


        if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
        {
            objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "User").ToList();
        }
        else
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            string userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
          objOrderHeaders = _unitOfWork.OrderHeader.GetAll(u=>u.UserID == userId,includeProperties: "User").ToList();

        }

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
