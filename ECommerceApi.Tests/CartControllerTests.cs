using ECommerceApi.Controllers;
using ECommerceApi.DTOs;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Tests;

public class CartControllerTests
{
    [Fact] public async Task GetCart_WithoutCart_CreatesCart() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        });
        var controller =new CartController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.GetCart();
        // Assert
        Assert.IsType<OkObjectResult>(result); Assert.Single(await db.Context.Carts.ToListAsync()); }

    [Fact] public async Task GetCart_ReturnsCurrentUserCart() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        });
        db.Context.Carts.Add(new Cart{UserId="u1"}); await db.Context.SaveChangesAsync(); var controller=new CartController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.GetCart();
        // Assert
        var ok=Assert.IsType<OkObjectResult>(result); Assert.Equal("u1",ok.Value!.GetType().GetProperty("UserId")!.GetValue(ok.Value)); }

    [Fact] public async Task GetCart_DoesNotReturnAnotherUsersCart() { // Arrange
        using var db=new TestDb(); db.Context.Users.AddRange(
    new User
    {
        Id = "u1",
        UserName = "u1",
        Email = "u1@example.com"
    },
    new User
    {
        Id = "u2",
        UserName = "u2",
        Email = "u2@example.com"
    }
); db.Context.Carts.Add(new Cart{UserId="u2"}); await db.Context.SaveChangesAsync(); var controller=new CartController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        await controller.GetCart();
        // Assert
        Assert.Equal(2,await db.Context.Carts.CountAsync()); Assert.Contains(await db.Context.Carts.ToListAsync(),c=>c.UserId=="u1"); }

    [Fact] public async Task AddItem_MissingProduct_ReturnsNotFound() { // Arrange
        using var db=new TestDb(); var controller=new CartController(db.Context); TestHelpers.SetUser(controller,"u1"); var dto=new AddCartItemDto{ProductId=99,Quantity=1};
        // Act
        var result=await controller.AddItem(dto);
        // Assert
        Assert.IsType<NotFoundResult>(result); }

    [Fact] public async Task AddItem_ValidProduct_CreatesCartItem() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        });
        db.Context.Products.Add(TestHelpers.Product(1,"Phone")); await db.Context.SaveChangesAsync(); var controller=new CartController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.AddItem(new AddCartItemDto{ProductId=1,Quantity=2});
        // Assert
        Assert.IsType<OkObjectResult>(result); var item=await db.Context.CartItems.SingleAsync(); Assert.Equal(2,item.Quantity); }

    [Fact] public async Task AddItem_SecondAddition_IncreasesQuantity() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        });
        db.Context.Products.Add(TestHelpers.Product(1,"Phone")); db.Context.Carts.Add(new Cart{UserId="u1",Items=new List<CartItem>{new(){ProductId=1,Quantity=2}}}); await db.Context.SaveChangesAsync(); var controller=new CartController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        await controller.AddItem(new AddCartItemDto{ProductId=1,Quantity=3});
        // Assert
        Assert.Equal(5,(await db.Context.CartItems.SingleAsync()).Quantity); }

    [Fact] public async Task AddItem_DifferentProduct_CreatesSecondItem() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        });
        db.Context.Products.AddRange(TestHelpers.Product(1,"Phone"),TestHelpers.Product(2,"Laptop")); await db.Context.SaveChangesAsync(); var controller=new CartController(db.Context); TestHelpers.SetUser(controller,"u1"); await controller.AddItem(new AddCartItemDto{ProductId=1,Quantity=1});
        // Act
        await controller.AddItem(new AddCartItemDto{ProductId=2,Quantity=1});
        // Assert
        Assert.Equal(2,await db.Context.CartItems.CountAsync()); }

    [Fact]
    public async Task AddItem_CreatesCartForUserWhenMissing()
    { // Arrange
        using var db = new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        }); db.Context.Products.Add(TestHelpers.Product(1, "Phone")); await db.Context.SaveChangesAsync(); var controller = new CartController(db.Context); TestHelpers.SetUser(controller, "u1");
        // Act
        var result = await controller.AddItem(
        new AddCartItemDto
        {
            ProductId = 1,
            Quantity = 1
        });
        // Assert
        Assert.Single(await db.Context.Carts.ToListAsync());
    }

    [Fact] public async Task AddItem_ZeroQuantity_ReturnsBadRequestWhenModelStateInvalid() { // Arrange
        using var db=new TestDb(); db.Context.Products.Add(TestHelpers.Product(1,"Phone")); await db.Context.SaveChangesAsync(); var controller=new CartController(db.Context); TestHelpers.SetUser(controller,"u1"); controller.ModelState.AddModelError("Quantity","Quantity must be positive");
        // Act
        var result=await controller.AddItem(new AddCartItemDto{ProductId=1,Quantity=0});
        // Assert
        Assert.IsType<BadRequestObjectResult>(result); }

    [Fact] public async Task RemoveItem_MissingItem_ReturnsNotFound() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        });
        db.Context.Carts.Add(new Cart{UserId="u1"}); await db.Context.SaveChangesAsync(); var controller=new CartController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.RemoveItem(1);
        // Assert
        Assert.IsType<NotFoundResult>(result); }

    [Fact] public async Task RemoveItem_ExistingItem_RemovesItem() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        });
        db.Context.Products.Add(TestHelpers.Product(1,"Phone")); db.Context.Carts.Add(new Cart{UserId="u1",Items=new List<CartItem>{new(){ProductId=1,Quantity=2}}}); await db.Context.SaveChangesAsync(); var controller=new CartController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.RemoveItem(1);
        // Assert
        Assert.IsType<OkObjectResult>(result); Assert.Empty(await db.Context.CartItems.ToListAsync()); }

    [Fact] public async Task RemoveItem_OnlyRemovesRequestedProduct() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        });
        db.Context.Products.AddRange(TestHelpers.Product(1,"Phone"),TestHelpers.Product(2,"Laptop")); db.Context.Carts.Add(new Cart{UserId="u1",Items=new List<CartItem>{new(){ProductId=1,Quantity=2},new(){ProductId=2,Quantity=4}}}); await db.Context.SaveChangesAsync(); var controller=new CartController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        await controller.RemoveItem(1);
        // Assert
        var item=await db.Context.CartItems.SingleAsync(); Assert.Equal(2,item.ProductId); }

    [Fact] public async Task RemoveItem_DoesNotRemoveOtherUsersItem() { // Arrange
        using var db=new TestDb(); db.Context.Users.AddRange(
    new User { Id = "u1", UserName = "u1", Email = "u1@example.com" },
    new User { Id = "u2", UserName = "u2", Email = "u2@example.com" }
);
        db.Context.Products.Add(TestHelpers.Product(1,"Phone")); db.Context.Carts.Add(new Cart{UserId="u2",Items=new List<CartItem>{new(){ProductId=1,Quantity=2}}}); await db.Context.SaveChangesAsync(); var controller=new CartController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.RemoveItem(1);
        // Assert
        Assert.IsType<NotFoundResult>(result); Assert.Single(await db.Context.CartItems.ToListAsync()); }

    [Fact] public async Task GetCart_IncludesExistingItems() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        });
        db.Context.Products.Add(TestHelpers.Product(1,"Phone")); db.Context.Carts.Add(new Cart{UserId="u1",Items=new List<CartItem>{new(){ProductId=1,Quantity=3}}}); await db.Context.SaveChangesAsync(); var controller=new CartController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.GetCart();
        // Assert
        var ok=Assert.IsType<OkObjectResult>(result); var items=ok.Value!.GetType().GetProperty("Items")!.GetValue(ok.Value) as IEnumerable<object>; Assert.NotNull(items); }

    [Fact] public async Task AddItem_QuantityOne_IsStoredAsOne() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        });
        db.Context.Products.Add(TestHelpers.Product(1,"Phone")); await db.Context.SaveChangesAsync(); var controller=new CartController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        await controller.AddItem(new AddCartItemDto{ProductId=1,Quantity=1});
        // Assert
        Assert.Equal(1,(await db.Context.CartItems.SingleAsync()).Quantity); }

    [Fact] public async Task AddItem_UsesAuthenticatedUserId() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "abc",
            UserName = "abc",
            Email = "abc@example.com"
        });
        db.Context.Products.Add(TestHelpers.Product(1,"Phone")); await db.Context.SaveChangesAsync(); var controller=new CartController(db.Context); TestHelpers.SetUser(controller,"abc");
        // Act
        await controller.AddItem(new AddCartItemDto{ProductId=1,Quantity=1});
        // Assert
        Assert.Equal("abc",(await db.Context.Carts.SingleAsync()).UserId); }
}
