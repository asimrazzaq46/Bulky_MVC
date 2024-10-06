using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using BulkyWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repositery;

public class ShoppingCartRepositery : Repositery<ShoppingCart>, IShoppingCartRepositery
{
    public ApplicationDbContext _db;

    public ShoppingCartRepositery(ApplicationDbContext db):base(db)
    {
        _db = db;
    }

    public void Update(ShoppingCart obj)
    {
        _db.ShoppingCarts.Update(obj);
    }


    
}
