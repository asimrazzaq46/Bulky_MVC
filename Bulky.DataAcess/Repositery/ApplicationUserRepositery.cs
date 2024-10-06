using Bulky.DataAcess.Repositery.IRepositery;
using Bulky.Models.Models;
using BulkyWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repositery;

public class ApplicationUserRepositery:Repositery<ApplicationUser>,IApplicationUserRepositery
{
    public ApplicationDbContext _db;
    public ApplicationUserRepositery(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }
}
