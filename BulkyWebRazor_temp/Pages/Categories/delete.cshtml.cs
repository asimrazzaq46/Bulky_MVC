using BulkyWebRazor_temp.data;
using BulkyWebRazor_temp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_temp.Pages.Categories
{
    public class deleteModel : PageModel
    {
        private readonly ApplicationContextDb _db;

        [BindProperty]
        public Category Category { get; set; }

        public deleteModel(ApplicationContextDb db)
        {
            _db = db;
        }
        public void OnGet(int? id)
        {
           Category = _db.Categories.FirstOrDefault(Category => Category.Id == id);
        }

        public IActionResult OnPost(Category obj) { 
        
            _db.Categories.Remove(obj);
            _db.SaveChanges();
            TempData["success"] = "Category deleted Successfully";
            return RedirectToPage("Index");
        }
    }
}
