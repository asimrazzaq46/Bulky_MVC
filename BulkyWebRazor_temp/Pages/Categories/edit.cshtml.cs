using BulkyWebRazor_temp.data;
using BulkyWebRazor_temp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_temp.Pages.Categories
{
    public class EditModel(ApplicationContextDb db) : PageModel
    {

        private readonly ApplicationContextDb _db = db;

        [BindProperty]
        public Category? Category { get; set; }

        public void OnGet(int? id)
        {
            if (id is not null || id == 0)
            {
                Category = _db.Categories.Find(id);
            }

        }

        public IActionResult OnPost()
        {
           
            _db.Categories.Update(Category);
            _db.SaveChanges();
            TempData["success"] = "Category Edit Successfully";
            return RedirectToPage("Index");
           

        }
    }
}
