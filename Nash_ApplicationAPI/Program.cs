using Microsoft.EntityFrameworkCore;
using Structure_Base.BaseService;
using Structure_Base;
using Structure_Context;
using Structure_Core;
using Structure_Servicer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

// load configuration from appsettings.json
var configuration = builder.Configuration;

string connectionString = configuration.GetConnectionString("DB_Ecommerce_Rookie");
// Kiểm tra xem chuỗi kết nối có được đọc đúng không
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DB_Ecommerce_Rookie' not found in appsettings.json.");
}
else
{
    Console.WriteLine($"Connection String: {connectionString}"); // Ghi log để kiểm tra
}
// Dang ký databasecontext
builder.Services.AddDbContext<DB_Ecommerce_Rookie_Context>(options =>
    options.UseSqlServer(connectionString));

// add dbcontext
builder.Services.AddTransient<DB_Ecommerce_Rookie_Context>();
//add services to the container.
#region DI
//Brand
builder.Services.AddTransient<IBrandProvider, BrandProvider>();
builder.Services.AddTransient<ICRUD_Service<Brand, int>, BrandProvider>();
//ProductCategory
builder.Services.AddTransient<IProductCategoryProvider, ProductCategoryProvider>();
builder.Services.AddTransient<ICRUD_Service<ProductCategory, int>, ProductCategoryProvider>();
//UnitOfMeasure
builder.Services.AddTransient<IUnitOfMeasureProvider, UnitOfMeasureProvider>();
builder.Services.AddTransient<ICRUD_Service<UnitOfMeasure, int>, UnitOfMeasureProvider>();
//Product
builder.Services.AddTransient<IProductProvider, ProductProvider>();
builder.Services.AddTransient<ICRUD_Service<Product, int>, ProductProvider>();
#endregion

// Đăng ký dịch vụ CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed((host) => true)
              .AllowCredentials();
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
