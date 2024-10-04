using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using BulkyWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repositery;

public class ProductRepositery : Repositery<Product>, IProductRepositery
{
    public ApplicationDbContext _db;

    public ProductRepositery(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void update(Product obj)
    {
        Product productFromDb = _db.Products.FirstOrDefault(u => u.Id == obj.Id);
        if (productFromDb != null)
        {
            productFromDb.Title = obj.Title;
            productFromDb.ISBN = obj.ISBN;
            productFromDb.Author = obj.Author;
            productFromDb.Price100 = obj.Price100;
            productFromDb.Price = obj.Price;
            productFromDb.Price50 = obj.Price50;
            productFromDb.ListPrice = obj.ListPrice;
            productFromDb.Description = obj.Description;
            productFromDb.CategoryId = obj.CategoryId;

            if (productFromDb.ImageUrl != null)
            {
                productFromDb.ImageUrl = obj.ImageUrl;
            }

        }
        
    }
}
