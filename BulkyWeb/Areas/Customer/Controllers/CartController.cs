using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// /Bind Property will automatically bind this property with all the places where
    /// we are using ShoppingCartVm like in get and post methods
    /// </summary>
    [BindProperty]
    public ShoppingCartVM ShoppingCartVM { get; set; }

    public CartController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {

        ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
        string UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartVM = new()
        {
            ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.UserId == UserId, includeProperties: "Product").ToList(),
            OrderHeader = new()
        };

        foreach (var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
        }

        return View(ShoppingCartVM);
    }

    public IActionResult Summary()
    {

        ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
        string UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartVM = new()
        {
            ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.UserId == UserId, includeProperties: "Product").ToList(),
            //ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(includeProperties: "Product").ToList(),
            OrderHeader = new()
        };

        OrderHeader cartOrderHeader = ShoppingCartVM.OrderHeader;

        cartOrderHeader.User = _unitOfWork.User.GetOne(u => u.Id == UserId);

        ApplicationUser CartUSer = ShoppingCartVM.OrderHeader.User;

        cartOrderHeader.Name = CartUSer.Name;
        cartOrderHeader.PhoneNumber = CartUSer.PhoneNumber;
        cartOrderHeader.City = CartUSer.City;
        cartOrderHeader.State = CartUSer.State;
        cartOrderHeader.PhoneNumber = CartUSer.PhoneNumber;
        cartOrderHeader.PostalCode = CartUSer.PostalCode;
        cartOrderHeader.StrretAddress = CartUSer.StreetAddress;

        foreach (var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
        }

        return View(ShoppingCartVM);
    }

    [HttpPost]
    [ActionName("Summary")]
    public IActionResult SummaryPost()
    {

        ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
        string UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.UserId == UserId, includeProperties: "Product").ToList();
        //ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(includeProperties: "Product").ToList(),


        OrderHeader cartOrderHeader = ShoppingCartVM.OrderHeader;

        ApplicationUser UserDb = _unitOfWork.User.GetOne(u => u.Id == UserId);

        ApplicationUser CartUSer = ShoppingCartVM.OrderHeader.User;

        cartOrderHeader.OrderDate = DateTime.Now;
        cartOrderHeader.UserID = UserId;

        //0a464e12 - cffa - 408e-bc87 - 3e117676b075


        foreach (var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
        }

        if (UserDb.CompanyId.GetValueOrDefault() == 0)
        {
            //it is a regular client
            cartOrderHeader.PaymentStatus = SD.Payment_Status_Pending;
            cartOrderHeader.OrderStatus = SD.Status_Pending;
        }
        else
        {
            //it's a company user...so he can pay within 30 days after recieving Shippment
            cartOrderHeader.PaymentStatus = SD.Payment_Status_DelayedPayment;
            cartOrderHeader.OrderStatus = SD.Status_Approved;
        }

        _unitOfWork.OrderHeader.Add(cartOrderHeader);
        _unitOfWork.Save();


        // NOW CREATING ORDER DETAIL 

        foreach (var cart in ShoppingCartVM.ShoppingCartList)
        {
            OrderDetail orderDetail = new()
            {
                ProductId = cart.ProductId,
                OrderHeaderId = cartOrderHeader.Id,
                Price = cart.Price,
                Count = cart.Count,
            };

            _unitOfWork.OrderDetail.Add(orderDetail);
            _unitOfWork.Save();
        }

        if (UserDb.CompanyId.GetValueOrDefault() == 0)
        {
            //it is a regular client
            //Stripe Logic will come here

            string domain = "https://localhost:7211";

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"/customer/cart/OrderConfirmation?id={cartOrderHeader.Id}",
                CancelUrl = domain + "/customer/cart/Index",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in ShoppingCartVM.ShoppingCartList)
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
            _unitOfWork.OrderHeader.UpdateStripePaymentID(cartOrderHeader.Id,session.Id,session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location",session.Url);
            return new StatusCodeResult(303);


        }

        return RedirectToAction("OrderConfirmation", new { id = cartOrderHeader.Id });
    }


    public IActionResult OrderConfirmation(int id)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeader.GetOne(u=>u.Id == id,includeProperties:"User");
        if(orderHeader.PaymentStatus != SD.Payment_Status_DelayedPayment)
        {
            //this is an order by a customer
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if(session.PaymentStatus.ToLower() == "paid")
            {
				_unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                _unitOfWork.OrderHeader.UpdateStatus(id,SD.Status_Approved,SD.Payment_Status_Approved);
                _unitOfWork.Save();
			}
		}
        List<ShoppingCart> shoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.UserId == orderHeader.UserID).ToList();
        _unitOfWork.ShoppingCart.RemoveRange(shoppingCartList);
        _unitOfWork.Save();

        return View(id);
    }


    private double GetPriceBasedOnQuantity(ShoppingCart cart)
    {
        if (cart.Count <= 50)
        {
            return cart.Product.Price;
        }
        else
        {
            if (cart.Count <= 100)
            {
                return cart.Product.Price50;
            }
            else
            {
                return cart.Product.Price100;

            }
        }
    }


    public IActionResult Minus(int id)
    {
        ShoppingCart shoppingCartFromDb = _unitOfWork.ShoppingCart.GetOne(u => u.Id == id);

        if (shoppingCartFromDb.Count <= 1)
        {
            _unitOfWork.ShoppingCart.Remove(shoppingCartFromDb);
        }
        else
        {
            shoppingCartFromDb.Count -= 1;
            _unitOfWork.ShoppingCart.Update(shoppingCartFromDb);


        }
        _unitOfWork.Save();
        return RedirectToAction("Index");
    }

    public IActionResult Plus(int CartId)
    {
        var shoppingCartFromDb = _unitOfWork.ShoppingCart.GetOne(u => u.Id == CartId);


        shoppingCartFromDb.Count += 1;


        _unitOfWork.ShoppingCart.Update(shoppingCartFromDb);
        _unitOfWork.Save();
        return RedirectToAction("Index");
    }

    public IActionResult Remove(int CartId)
    {
        ShoppingCart shoppingCartFromDb = _unitOfWork.ShoppingCart.GetOne(u => u.Id == CartId);

        _unitOfWork.ShoppingCart.Remove(shoppingCartFromDb);
        _unitOfWork.Save();
        return RedirectToAction("Index");
    }





}
