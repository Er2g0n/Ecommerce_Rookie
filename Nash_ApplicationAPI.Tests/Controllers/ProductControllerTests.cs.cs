using Microsoft.AspNetCore.Mvc;
using Moq;
using Nash_ApplicationAPI.Controllers.ProductManagement;
using Structure_Base.BaseService;
using Structure_Base.ProductManagement;
using Structure_Core.BaseClass;
using Structure_Core.ProductManagement;
using Structure_Servicer.ProductManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Nash_ApplicationAPI.Tests
{
    public class ProductControllerTests
    {
        private readonly Mock<ICRUD_Service<Product, int>> _mockCrudService;
        private readonly Mock<IProductProvider> _mockProductProvider;
        private readonly Mock<IImageProvider> _mockImageProvider;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            // Initialize mocks
            _mockCrudService = new Mock<ICRUD_Service<Product, int>>();
            _mockProductProvider = new Mock<IProductProvider>();
            _mockImageProvider = new Mock<IImageProvider>();

            // Initialize the controller with mocked dependencies
            _controller = new ProductController(_mockCrudService.Object, _mockProductProvider.Object, _mockImageProvider.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WhenProductsExist()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductCode = "PROD001", ProductName = "Product 1", CategoryCode = "CAT001", BrandCode = "BRD001", Description = "Description 1" },
                new Product { ProductCode = "PROD002", ProductName = "Product 2", CategoryCode = "CAT002", BrandCode = "BRD002", Description = "Description 2" }
            };
            var resultService = new ResultService<IEnumerable<Product>>
            {
                Code = "0",
                Message = "Success",
                Data = products
            };

            _mockCrudService.Setup(service => service.GetAll()).ReturnsAsync(resultService);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ResultService<IEnumerable<Product>>>(okResult.Value);
            Assert.Equal("0", returnValue.Code);
            Assert.Equal("Success", returnValue.Message);
            Assert.Equal(products, returnValue.Data);
        }

        [Fact]
        public async Task GetAll_ReturnsBadRequest_WhenNoProductsFound()
        {
            // Arrange
            _mockCrudService.Setup(service => service.GetAll()).ReturnsAsync((ResultService<IEnumerable<Product>>)null);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No products found", badRequestResult.Value);
        }
        [Fact]
        public async Task GetAll_ReturnsOkResult_WithEmptyList_WhenNoProductsExist()
        {
            // Arrange
            var resultService = new ResultService<IEnumerable<Product>>
            {
                Code = "1",
                Message = "No products found",
                Data = new List<Product>()
            };

            _mockCrudService.Setup(service => service.GetAll()).ReturnsAsync(resultService);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ResultService<IEnumerable<Product>>>(okResult.Value);
            Assert.Equal("1", returnValue.Code);
            Assert.Equal("No products found", returnValue.Message);
            Assert.Empty(returnValue.Data);
        }
        [Fact]
        public async Task GetAll_ReturnsBadRequest_WhenServiceThrowsException()
        {
            // Arrange
            _mockCrudService.Setup(service => service.GetAll()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No products found", badRequestResult.Value); // Since GetAll returns BadRequest on null
        }
    }
}