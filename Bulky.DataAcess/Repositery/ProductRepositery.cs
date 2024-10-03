using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using BulkyWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repositery;

public class ProductRepositery :Repositery<Product> , IProductRepositery
{
    public ApplicationDbContext _db;

    public ProductRepositery(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void update(Product obj)
    {
        _db.Products.Update(obj);  
    }
}
