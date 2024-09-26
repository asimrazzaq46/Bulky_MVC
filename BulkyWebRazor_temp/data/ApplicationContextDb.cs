using BulkyWebRazor_temp.Model;
using Microsoft.EntityFrameworkCore;

namespace BulkyWebRazor_temp.data;

public class ApplicationContextDb:DbContext
{

    public ApplicationContextDb(DbContextOptions<ApplicationContextDb> options): base(options)
    {
        
    }

    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category {Id=1, Name="Drama",DisplayOrder=1},
            new Category {Id=2, Name="Horror",DisplayOrder=2},
            new Category {Id=3, Name="Romance",DisplayOrder=3}
            );
    }

}
