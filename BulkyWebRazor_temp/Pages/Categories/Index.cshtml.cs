using BulkyWebRazor_temp.data;
using BulkyWebRazor_temp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_temp.Pages.Categories
{
    public class IndexModel : PageModel
    {

        private readonly ApplicationContextDb _db;
        public List<Category> categoriesList { get; set; }
        public IndexModel(ApplicationContextDb db)
        {
            _db = db;
        }

        public void OnGet()
        {
            categoriesList = _db.Categories.ToList();

        }
    }
}
