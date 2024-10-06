using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using BulkyWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repositery;

public class OrderDetailRepositery : Repositery<OrderDetail>, IOrderDetailRepositery
{
    public ApplicationDbContext _db;

    public OrderDetailRepositery(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(OrderDetail obj)
    {
        _db.OrderDetails.Update(obj);
    }
}
