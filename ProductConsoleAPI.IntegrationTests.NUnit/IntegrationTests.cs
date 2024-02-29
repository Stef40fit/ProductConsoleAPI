using Microsoft.EntityFrameworkCore;
using ProductConsoleAPI.Business;
using ProductConsoleAPI.Business.Contracts;
using ProductConsoleAPI.Data.Models;
using ProductConsoleAPI.DataAccess;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;

namespace ProductConsoleAPI.IntegrationTests.NUnit
{
    public  class IntegrationTests
    {
        private TestProductsDbContext dbContext;
        private IProductsManager productsManager;

        [SetUp]
        public void SetUp()
        {
            this.dbContext = new TestProductsDbContext();
            this.productsManager = new ProductsManager(new ProductsRepository(this.dbContext));
        }


        [TearDown]
        public void TearDown()
        {
            this.dbContext.Database.EnsureDeleted();
            this.dbContext.Dispose();
        }


        //positive test
        [Test]
        public async Task AddProductAsync_ShouldAddNewProduct()
        {
            var newProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProduct",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };

            await productsManager.AddAsync(newProduct);

            var dbProduct = await this.dbContext.Products.FirstOrDefaultAsync(p => p.ProductCode == newProduct.ProductCode);

            Assert.NotNull(dbProduct);
            Assert.AreEqual(newProduct.ProductName, dbProduct.ProductName);
            Assert.AreEqual(newProduct.Description, dbProduct.Description);
            Assert.AreEqual(newProduct.Price, dbProduct.Price);
            Assert.AreEqual(newProduct.Quantity, dbProduct.Quantity);
            Assert.AreEqual(newProduct.OriginCountry, dbProduct.OriginCountry);
            Assert.AreEqual(newProduct.ProductCode, dbProduct.ProductCode);
        }

        //Negative test
        [Test]
        public async Task AddProductAsync_TryToAddProductWithInvalidCredentials_ShouldThrowException()
        {
            var newProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProduct",
                ProductCode = "AB12C",
                Price = -1m,
                Quantity = 100,
                Description = "Anything for description"
            };

            var exeption = Assert.ThrowsAsync<ValidationException>(async () => await productsManager.AddAsync(newProduct));
            var actual = await dbContext.Products.FirstOrDefaultAsync(c => c.ProductCode == newProduct.ProductCode);

            Assert.IsNull(actual);
            Assert.That(exeption.Message, Is.EqualTo("Invalid product!"));

        }

        [Test]
        public async Task DeleteProductAsync_WithValidProductCode_ShouldRemoveProductFromDb()
        {
            // Arrange
            var newProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProduct",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };

            await productsManager.AddAsync(newProduct);
            // Act
            await productsManager.DeleteAsync(newProduct.ProductCode);
            // Assert
            var productInDB = await dbContext.Products.FirstOrDefaultAsync(x => x.ProductCode == newProduct.ProductCode);
            Assert.IsNull(productInDB);
            
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task DeleteProductAsync_TryToDeleteWithNullOrWhiteSpaceProductCode_ShouldThrowException(string InvalideCode)
        {


            // Act
            var exeption = Assert.ThrowsAsync<ArgumentException>(() => productsManager.DeleteAsync(InvalideCode));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("Product code cannot be empty."));
        }

        [Test]
        public async Task GetAllAsync_WhenProductsExist_ShouldReturnAllProducts()
        {
            // Arrange
            var firsProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProduct",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };
            var secondProduct = new Product()
            {
                OriginCountry = "Germany",
                ProductName = "TestProduct",
                ProductCode = "DB12C",
                Price = 100m,
                Quantity = 200,
                Description = "Anything for description"
            };

            await productsManager.AddAsync(firsProduct);
            await productsManager.AddAsync(secondProduct);
            // Act
           var result =  await productsManager.GetAllAsync();
            // Assert
            Assert.IsNotNull(firsProduct);
            Assert.IsNotNull(secondProduct);
            Assert.That(result.Count, Is.EqualTo(2));
            //Assert.That();
        }

        [Test]
        public async Task GetAllAsync_WhenNoProductsExist_ShouldThrowKeyNotFoundException()
        {


            var exeption = Assert.ThrowsAsync<KeyNotFoundException>(() => productsManager.GetAllAsync());
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("No product found."));
        }

        [Test]
        public async Task SearchByOriginCountry_WithExistingOriginCountry_ShouldReturnMatchingProducts()
        {
            // Arrange
            var secondProduct = new Product()
            {
                OriginCountry = "Germany",
                ProductName = "TestProduct",
                ProductCode = "DB12C",
                Price = 100m,
                Quantity = 200,
                Description = "Anything for description"
            };

            await productsManager.AddAsync(secondProduct);
            // Act
            var result = await productsManager.SearchByOriginCountry(secondProduct.OriginCountry);
            // Assert
            var resultPoduct = result.First();
            Assert.That(resultPoduct.OriginCountry, Is.EqualTo(secondProduct.OriginCountry));
           
        }

        [Test]
        
        public async Task SearchByOriginCountryAsync_WithNonExistingOriginCountry_ShouldThrowKeyNotFoundException()
        {

            var exeption = Assert.ThrowsAsync<KeyNotFoundException>(() => productsManager.SearchByOriginCountry("fff"));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("No product found with the given first name."));
        }
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]

        public async Task SearchByOriginCountryAsync_WithNInvalidOriginCountry_ShouldThrowArgumentException(string InvalidCode)
        {

            var exeption = Assert.ThrowsAsync<ArgumentException>(() => productsManager.SearchByOriginCountry(InvalidCode));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("Country name cannot be empty."));
        }

        [Test]
        public async Task GetSpecificAsync_WithValidProductCode_ShouldReturnProduct()
        {
            // Arrange
            var secondProduct = new Product()
            {
                OriginCountry = "Germany",
                ProductName = "TestProduct",
                ProductCode = "DB12C",
                Price = 100m,
                Quantity = 200,
                Description = "Anything for description"
            };

            await productsManager.AddAsync(secondProduct);
            // Act
            var result = await productsManager.GetSpecificAsync(secondProduct.ProductCode);
            // Assert
            Assert.That(result.ProductCode, Is.EqualTo(secondProduct.ProductCode));
        }

        [Test]
        public async Task GetSpecificAsync_WithInvalidProductCode_ShouldThrowKeyNotFoundException()
        {

           

            //Act
            var exeption = Assert.ThrowsAsync<KeyNotFoundException>(() => productsManager.GetSpecificAsync("notCode"));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo($"No product found with product code: {"notCode"}"));
        }
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task GetSpecificAsync_WithInvalidOrNullProductCode_ShouldThrowArgumentException(string InvalidCode)
        {

            //Act
            var exeption = Assert.ThrowsAsync<ArgumentException>(() => productsManager.GetSpecificAsync(InvalidCode));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("Product code cannot be empty."));
        }











        [Test]
        public async Task UpdateAsync_WithValidProduct_ShouldUpdateProduct()
        {
            // Arrange
            var secondProduct = new Product()
            {
                OriginCountry = "Germany",
                ProductName = "TestProduct",
                ProductCode = "DB12C",
                Price = 100m,
                Quantity = 200,
                Description = "Anything for description"
            };

            await productsManager.AddAsync(secondProduct);
            // Act
            secondProduct.ProductName = "Update_Name";
            await productsManager.UpdateAsync(secondProduct);

            // Assert
            var result = await productsManager.GetSpecificAsync(secondProduct.ProductCode);
            Assert.That(result.ProductName, Is.EqualTo(secondProduct.ProductName));
        }

        [Test]
        public async Task UpdateAsync_WithInvalidProduct_ShouldThrowValidationException()
        {

            // Arrange
            var invalidProduct = new Product()
            {
                OriginCountry = "Germany",
                ProductName = "TestProduct",
                ProductCode = "DB12C",
                Price = -100m,
                Quantity = 200,
                Description = "Anything for description"
            };

            
            // Act & Assert
            
            var exeption = Assert.ThrowsAsync<ValidationException>(() => productsManager.UpdateAsync(invalidProduct));
            
            Assert.That(exeption.Message, Is.EqualTo("Invalid prduct!"));
        }
        [Test]
        public async Task UpdateAsync_WithNullProduct_ShouldThrowValidationException()
        {

            

            // Act & Assert

            var exeption = Assert.ThrowsAsync<ValidationException>(() => productsManager.UpdateAsync(null));

            Assert.That(exeption.Message, Is.EqualTo("Invalid prduct!"));
        }
    }
}
