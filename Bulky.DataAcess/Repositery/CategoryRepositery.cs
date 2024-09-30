using Bulky.DataAcess.Repositery.IRepositery;
using BulkyWeb.Data;
using BulkyWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repositery;

public class CategoryRepositery :Repositery<Category> ,ICategoryRepositery
{
    public ApplicationDbContext _db;

    public CategoryRepositery(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(Category obj)
    {
        _db.Categories.Update(obj);
    }
}
