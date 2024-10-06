using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using BulkyWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repositery;

public class OrderHeaderRepositery : Repositery<OrderHeader>, IOrderHeaderRepositery
{
    public ApplicationDbContext _db;

    public OrderHeaderRepositery(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(OrderHeader obj)
    {
        _db.OrderHeaders.Update(obj);
    }

    public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
    {
        var orderFromDB = _db.OrderHeaders.FirstOrDefault(x=> x.Id == id);
        if (orderFromDB.OrderStatus != null) { 
            orderFromDB.OrderStatus = orderStatus;
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                orderFromDB.PaymentStatus = paymentStatus;
            }
        }
    }

    public void UpdateStripePaymentID(int id, string sessionID, string paymentIntentId)
    {
        var orderFromDB = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
        if (!String.IsNullOrEmpty(sessionID))
        {
            orderFromDB.SessionId = sessionID;
        }
        if (!String.IsNullOrEmpty(paymentIntentId))
        {
            orderFromDB.PaymentIntentId = paymentIntentId;
            orderFromDB.OrderDate = DateTime.Now;
        }
    }
}
