using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using BulkyWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repositery;

public class CompanyRepositer : Repositery<Company>, ICompanyRepositery
{

    public ApplicationDbContext _db;

    public CompanyRepositer(ApplicationDbContext db):base(db) 
    {
        _db = db;
    }

    public void Update(Company obj)
    {
        _db.Companies.Update(obj);
    }
}
