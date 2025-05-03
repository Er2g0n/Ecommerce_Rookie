using Microsoft.EntityFrameworkCore;
using Structure_Base.BaseService;
using Structure_Core.ProductClassification;
using Structure_Core.ProductManagement;
using Structure_Servicer.ProductClassification;
using Structure_Servicer.ProductManagement;
using Structure_Base.ProductClassification;
using Structure_Base.ProductManagement;
using Structure_Context.ProductClassification;
using Structure_Context.ProductManagement;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Nash_ApplicationAPI.Extensions;

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
builder.Services.AddDbContext<DB_ProductClassification_Context>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<DB_ProductManagement_Context>(options =>
    options.UseSqlServer(connectionString));
// add dbcontext
builder.Services.AddTransient<DB_ProductClassification_Context>();
builder.Services.AddTransient<DB_ProductManagement_Context>();
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
//Image
builder.Services.AddTransient<IImageProvider, CloudinaryImageProvider>();
builder.Services.AddTransient<IProductImageProvider, ProductImageProvider>();
//Price
builder.Services.AddTransient<IPriceProvider, PriceProvider>();
builder.Services.AddTransient<ICRUD_Service<Price, int>, PriceProvider>();
#endregion

// Đăng ký dịch vụ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy.WithOrigins("http://localhost:5173") // Specify the frontend origin
              .AllowAnyMethod()
              .AllowAnyHeader());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen();
//------------------------------------------------------------
//Add Authentication to Swagger GEN
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.AddAppAuthetication();

builder.Services.AddAuthorization();
//------------------------------------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseCors("CorsPolicy");
app.MapControllers();

app.Run();
