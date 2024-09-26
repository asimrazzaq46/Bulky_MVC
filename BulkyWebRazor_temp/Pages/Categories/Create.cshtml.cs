using BulkyWebRazor_temp.data;
using BulkyWebRazor_temp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_temp.Pages.Categories
{
    public class CreateModel : PageModel
    {

        private readonly ApplicationContextDb _db;

        [BindProperty]
        public Category Category { get; set; }

        public CreateModel(ApplicationContextDb db)
        {
            _db = db;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost(Category category) {

              _db.Categories.Add(category);
                _db.SaveChanges();
            TempData["success"] = "Category created Successfully";
            return RedirectToPage("Index");
            
         
        }
    }
}
