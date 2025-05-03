using Microsoft.EntityFrameworkCore;
using Structure_Core.ProductManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Context.ProductManagement;
public class DB_ProductManagement_Context : DbContext
{
    public DB_ProductManagement_Context()
    {
    }
    public DB_ProductManagement_Context(DbContextOptions<DB_ProductManagement_Context> options) : base(options)
    {
    }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Price> Prices{ get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Product
        modelBuilder.Entity<Product>()
            .Property(c => c.ProductCode)
            .IsRequired()
            .HasMaxLength(100);
        modelBuilder.Entity<Product>()
            .Property(c => c.ProductName)
            .IsRequired()
            .HasMaxLength(100);
        
        modelBuilder.Entity<Product>()
            .Property(c => c.CategoryCode)
            .IsRequired()
            .HasMaxLength(100);
        modelBuilder.Entity<Product>()
            .Property(c => c.BrandCode)
            .IsRequired()
            .HasMaxLength(100);
        modelBuilder.Entity<Product>()
            .Property(c => c.UoMCode)
            .IsRequired()
            .HasMaxLength(100);
        modelBuilder.Entity<Product>()
            .Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(100);

        //Product Image
        modelBuilder.Entity<ProductImage>()
            .Property(c => c.RefProductCode)
            .IsRequired()
            .HasMaxLength(100);
        modelBuilder.Entity<ProductImage>()
            .Property(c => c.ImagePath)
            .IsRequired()
            .HasMaxLength(200);
        modelBuilder.Entity<ProductImage>()
            .Property(c => c.IsPrimary)
            .IsRequired();

        // Price
        modelBuilder.Entity<Price>()
            .Property(p => p.PriceCode)
            .IsRequired()
            .HasMaxLength(50);
        modelBuilder.Entity<Price>()
            .Property(p => p.ProductCode)
            .IsRequired()
            .HasMaxLength(50);
        modelBuilder.Entity<Price>()
            .Property(p => p.PriceCost)
            .IsRequired(false);
        modelBuilder.Entity<Price>()
            .Property(p => p.PriceSale)
            .IsRequired(false);
        modelBuilder.Entity<Price>()
            .Property(p => p.PriceVAT)
            .IsRequired(false);
        modelBuilder.Entity<Price>()
            .Property(p => p.TotalAmount)
            .IsRequired(false);
        modelBuilder.Entity<Price>()
            .Property(p => p.StartDate)
            .IsRequired(false);
        modelBuilder.Entity<Price>()
            .Property(p => p.EndDate)
            .IsRequired(false);
        modelBuilder.Entity<Price>()
            .Property(p => p.ApplyDate)
            .IsRequired(false);

        base.OnModelCreating(modelBuilder);
    }

}
