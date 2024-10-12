using Bulky.Models.Models;
using Bulky.Utility;
using BulkyWeb.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.DbInitializer;

public class DbInitializer : IDbInitializer
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;
    public DbInitializer(UserManager<IdentityUser> userManager,
                         RoleManager<IdentityRole> roleManager,
                         ApplicationDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }


    public void Initalize()
    {
        //Migrations if they are not applied

        try
        {
            if (_db.Database.GetPendingMigrations().Count() > 0)
            {
                _db.Database.Migrate();
            }

        }
        catch (Exception e)
        {
        }

        //Create Roles if they are not created

        if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
        {
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();

            //if roles are not created, then we create Admin user as well

            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "admin1@hotmail.com",
                Email = "admin1@hotmail.com",
                Name = "Admin by Db",
                PhoneNumber = "1234567890",
                StreetAddress = "via milano",
                State = "America",
                PostalCode = "23895",
                City = "Chicago",
            }, "Admin1@").GetAwaiter().GetResult();

            ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin1@hotmail.com");
            _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();

        }
        return;


    }
}
