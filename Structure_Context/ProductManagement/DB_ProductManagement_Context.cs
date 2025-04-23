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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            
        base.OnModelCreating(modelBuilder);
    }

}
