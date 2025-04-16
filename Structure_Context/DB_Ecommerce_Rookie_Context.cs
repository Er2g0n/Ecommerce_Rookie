using Microsoft.EntityFrameworkCore;
using Structure_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Context;
public class DB_Ecommerce_Rookie_Context : DbContext
{
    public DB_Ecommerce_Rookie_Context()
    {
    }
    public DB_Ecommerce_Rookie_Context(DbContextOptions<DB_Ecommerce_Rookie_Context> options) : base(options)
    {

    }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    //public DbSet<Product> Products { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>()
           .Property(c => c.BrandCode)
           .IsRequired()
           .HasMaxLength(100);

        modelBuilder.Entity<Brand>()
            .Property(c => c.BrandName)
            .IsRequired()
            .HasMaxLength(100);


        modelBuilder.Entity<ProductCategory>()
            .Property(c => c.CategoryCode)
            .IsRequired()
            .HasMaxLength(100);
        modelBuilder.Entity<ProductCategory>()
            .Property(c => c.CategoryName)
            .IsRequired()
            .HasMaxLength(100);

        base.OnModelCreating(modelBuilder);
    }
}