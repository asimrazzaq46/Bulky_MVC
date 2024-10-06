using Bulky.DataAcess.Repositery.IRepositery;
using BulkyWeb.Data;
using BulkyWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repositery;

public class UnitOfWork : IUnitOfWork
{

   public ApplicationDbContext _db;
    public ICategoryRepositery Category { get; private set; }

    public IProductRepositery Product { get; private set; }

    public ICompanyRepositery Company { get; private set; }

    public IShoppingCartRepositery ShoppingCart { get; private set; }

    public IApplicationUserRepositery User { get; private set; }

    public IOrderDetailRepositery OrderDetail { get; private set; }

    public IOrderHeaderRepositery OrderHeader { get; private set; }
    public UnitOfWork(ApplicationDbContext db) { 
        _db = db;
        Category = new CategoryRepositery(_db);
        Product = new ProductRepositery(_db);
        Company = new CompanyRepositer(_db);
        ShoppingCart = new ShoppingCartRepositery(_db);
        User = new ApplicationUserRepositery(_db);
        OrderDetail = new OrderDetailRepositery(_db);
        OrderHeader = new OrderHeaderRepositery(_db);
    }


    public void Save()
    {

        _db.SaveChanges();
    }
}
