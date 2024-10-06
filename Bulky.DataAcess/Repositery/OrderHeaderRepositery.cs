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
}
