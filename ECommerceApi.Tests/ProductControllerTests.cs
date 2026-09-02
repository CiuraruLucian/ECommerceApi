using ECommerceApi.Controllers;
using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Tests;

public class ProductControllerTests
{
    [Fact] public async Task GetAll_EmptyDatabase_ReturnsOk() { // Arrange
        using var db=new TestDb(); var controller=new ProductsController(db.Context);
        // Act
        var result=await controller.GetAll();
        // Assert
        var ok=Assert.IsType<OkObjectResult>(result); Assert.Empty((IEnumerable<Product>)ok.Value!); }

    [Fact] public async Task GetAll_WithProducts_ReturnsAllProducts() { // Arrange
        using var db=new TestDb(); db.Context.Products.AddRange(TestHelpers.Product(1,"Phone"),TestHelpers.Product(2,"Laptop")); await db.Context.SaveChangesAsync(); var controller=new ProductsController(db.Context);
        // Act
        var result=await controller.GetAll();
        // Assert
        var ok=Assert.IsType<OkObjectResult>(result); Assert.Equal(2,((IEnumerable<Product>)ok.Value!).Count()); }

    [Fact] public async Task GetById_ExistingProduct_ReturnsOk() { // Arrange
        using var db=new TestDb(); db.Context.Products.Add(TestHelpers.Product(1,"Phone")); await db.Context.SaveChangesAsync(); var controller=new ProductsController(db.Context);
        // Act
        var result=await controller.GetById(1);
        // Assert
        var ok=Assert.IsType<OkObjectResult>(result); Assert.Equal(1,((Product)ok.Value!).Id); }

    [Fact] public async Task GetById_MissingProduct_ReturnsNotFound() { // Arrange
        using var db=new TestDb(); var controller=new ProductsController(db.Context);
        // Act
        var result=await controller.GetById(999);
        // Assert
        Assert.IsType<NotFoundResult>(result); }

    [Fact] public async Task Search_ExactNormalizedName_ReturnsProduct() { // Arrange
        using var db=new TestDb(); db.Context.Products.Add(TestHelpers.Product(1,"Phone")); await db.Context.SaveChangesAsync(); var controller=new ProductsController(db.Context);
        // Act
        var result=await controller.Search("Phone");
        // Assert
        var ok=Assert.IsType<OkObjectResult>(result); Assert.Single((IEnumerable<Product>)ok.Value!); }

    [Fact] public async Task Search_IsCaseInsensitive() { // Arrange
        using var db=new TestDb(); db.Context.Products.Add(TestHelpers.Product(1,"Phone")); await db.Context.SaveChangesAsync(); var controller=new ProductsController(db.Context);
        // Act
        var result=await controller.Search("pHoNe");
        // Assert
        Assert.IsType<OkObjectResult>(result); }

    [Fact] public async Task Search_TrimsQuery() { // Arrange
        using var db=new TestDb(); db.Context.Products.Add(TestHelpers.Product(1,"Phone")); await db.Context.SaveChangesAsync(); var controller=new ProductsController(db.Context);
        // Act
        var result=await controller.Search("  phone  ");
        // Assert
        var ok=Assert.IsType<OkObjectResult>(result); Assert.Single((IEnumerable<Product>)ok.Value!); }

    [Fact] public async Task Search_PartialName_ReturnsMatches() { // Arrange
        using var db=new TestDb(); db.Context.Products.AddRange(TestHelpers.Product(1,"Gaming Laptop"),TestHelpers.Product(2,"Laptop Stand")); await db.Context.SaveChangesAsync(); var controller=new ProductsController(db.Context);
        // Act
        var result=await controller.Search("laptop");
        // Assert
        var ok=Assert.IsType<OkObjectResult>(result); Assert.Equal(2,((IEnumerable<Product>)ok.Value!).Count()); }

    [Fact] public async Task Search_NoMatches_ReturnsNotFound() { // Arrange
        using var db=new TestDb(); db.Context.Products.Add(TestHelpers.Product(1,"Phone")); await db.Context.SaveChangesAsync(); var controller=new ProductsController(db.Context);
        // Act
        var result=await controller.Search("tablet");
        // Assert
        Assert.IsType<NotFoundResult>(result); }

    [Fact] public async Task AddProduct_ValidDto_CreatesProduct() { // Arrange
        using var db=new TestDb(); var controller=new ProductsController(db.Context); var dto=new ProductDto{Name="Phone",Description="Desc",Price=10m,Stock=5};
        // Act
        var result=await controller.AddProduct(dto);
        // Assert
        Assert.IsType<CreatedResult>(result); Assert.Single(await db.Context.Products.ToListAsync()); }

    [Fact] public async Task AddProduct_NormalizesNameOnCreate() { // Arrange
        using var db=new TestDb(); var controller=new ProductsController(db.Context); var dto=new ProductDto{Name="  Phone  ",Description="Desc",Price=10m,Stock=5};
        // Act
        await controller.AddProduct(dto);
        // Assert
        var product=await db.Context.Products.SingleAsync(); Assert.Equal("phone",product.NormalizedName); }

    [Fact] public async Task AddProduct_TrimsNameForDuplicateCheck() { // Arrange
        using var db=new TestDb(); db.Context.Products.Add(TestHelpers.Product(1,"phone")); await db.Context.SaveChangesAsync(); var controller=new ProductsController(db.Context); var dto=new ProductDto{Name=" phone ",Description="New",Price=20m,Stock=2};
        // Act
        var result=await controller.AddProduct(dto);
        // Assert
        var response = Assert.IsType<ObjectResult>(result);
        Assert.Contains("This product already exists", response.Value?.ToString());

    }

    [Fact] public async Task AddProduct_PreservesPrice() { // Arrange
        using var db=new TestDb(); var controller=new ProductsController(db.Context); var dto=new ProductDto{Name="Phone",Description="Desc",Price=99.99m,Stock=5};
        // Act
        await controller.AddProduct(dto);
        // Assert
        Assert.Equal(99.99m,(await db.Context.Products.SingleAsync()).Price); }

    [Fact] public async Task AddProduct_PreservesStock() { // Arrange
        using var db=new TestDb(); var controller=new ProductsController(db.Context); var dto=new ProductDto{Name="Phone",Description="Desc",Price=10m,Stock=17};
        // Act
        await controller.AddProduct(dto);
        // Assert
        Assert.Equal(17,(await db.Context.Products.SingleAsync()).Stock); }

    [Fact] public async Task Put_MissingProduct_ReturnsNotFound() { // Arrange
        using var db=new TestDb(); var controller=new ProductsController(db.Context); var dto=new ProductDto{Name="Phone",Description="Desc",Price=10m,Stock=5};
        // Act
        var result=await controller.Put(99,dto);
        // Assert
        Assert.IsType<NotFoundResult>(result); }

    [Fact] public async Task Put_UpdatesNameAndNormalizedName() { // Arrange
        using var db=new TestDb(); db.Context.Products.Add(TestHelpers.Product(1,"Phone")); await db.Context.SaveChangesAsync(); var controller=new ProductsController(db.Context); var dto=new ProductDto{Name="Laptop",Description="",Price=-1,Stock=-1};
        // Act
        await controller.Put(1,dto);
        // Assert
        var product=await db.Context.Products.FindAsync(1); Assert.Equal("Laptop",product!.Name); Assert.Equal("laptop",product.NormalizedName); }

    [Fact] public async Task Put_UpdatesDescriptionWhenProvided() { // Arrange
        using var db=new TestDb(); db.Context.Products.Add(TestHelpers.Product(1,"Phone")); await db.Context.SaveChangesAsync(); var controller=new ProductsController(db.Context); var dto=new ProductDto{Name="",Description="Updated",Price=-1,Stock=-1};
        // Act
        await controller.Put(1,dto);
        // Assert
        Assert.Equal("Updated",(await db.Context.Products.FindAsync(1))!.Description); }

    [Fact] public async Task Put_DoesNotUpdatePriceWhenNegative() { // Arrange
        using var db=new TestDb(); db.Context.Products.Add(TestHelpers.Product(1,"Phone",20m)); await db.Context.SaveChangesAsync(); var controller=new ProductsController(db.Context); var dto=new ProductDto{Name="",Description="",Price=-5m,Stock=-1};
        // Act
        await controller.Put(1,dto);
        // Assert
        Assert.Equal(20m,(await db.Context.Products.FindAsync(1))!.Price); }

    [Fact] public async Task Put_DoesNotUpdateStockWhenNegative() { // Arrange
        using var db=new TestDb(); db.Context.Products.Add(TestHelpers.Product(1,"Phone",20m,8)); await db.Context.SaveChangesAsync(); var controller=new ProductsController(db.Context); var dto=new ProductDto{Name="",Description="",Price=-1,Stock=-3};
        // Act
        await controller.Put(1,dto);
        // Assert
        Assert.Equal(8,(await db.Context.Products.FindAsync(1))!.Stock); }

    [Fact] public async Task Delete_MissingProduct_ReturnsNotFound() { // Arrange
        using var db=new TestDb(); var controller=new ProductsController(db.Context);
        // Act
        var result=await controller.Delete(100);
        // Assert
        Assert.IsType<NotFoundResult>(result); }

    [Fact] public async Task Delete_ExistingProduct_RemovesProduct() { // Arrange
        using var db=new TestDb(); db.Context.Products.Add(TestHelpers.Product(1,"Phone")); await db.Context.SaveChangesAsync(); var controller=new ProductsController(db.Context);
        // Act
        var result=await controller.Delete(1);
        // Assert
        Assert.IsType<OkObjectResult>(result); Assert.Empty(await db.Context.Products.ToListAsync()); }
}
