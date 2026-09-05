using ECommerceApi.Controllers;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Tests;

public class OrderControllerTests
{
    [Fact] public async Task GetMyOrders_NoOrders_ReturnsNotFound() { // Arrange
        using var db=new TestDb(); var controller=new OrderController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.GetMyOrders();
        // Assert
        Assert.IsType<NotFoundResult>(result); }

    [Fact] public async Task GetMyOrders_ReturnsOnlyCurrentUsersOrders() { // Arrange
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
); db.Context.Orders.AddRange(new Order{UserId="u1",Total=10,Status="Pending"},new Order{UserId="u2",Total=20,Status="Paid"}); await db.Context.SaveChangesAsync(); var controller=new OrderController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.GetMyOrders();
        // Assert
        var ok=Assert.IsType<OkObjectResult>(result); Assert.Single((IEnumerable<object>)ok.Value!); }

    [Fact] public async Task GetMyOrders_ReturnsTotal() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        });

        db.Context.Carts.Add(new Cart
        {
            UserId = "u1"
        });
        db.Context.Orders.Add(new Order{UserId="u1",Total=55.50m,Status="Paid"}); await db.Context.SaveChangesAsync(); var controller=new OrderController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.GetMyOrders();
        // Assert
        var ok=Assert.IsType<OkObjectResult>(result); var first=((IEnumerable<object>)ok.Value!).Single(); Assert.Equal(55.50m,first.GetType().GetProperty("Total")!.GetValue(first)); }

    [Fact] public async Task GetMyOrders_ReturnsStatus() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        });

        db.Context.Carts.Add(new Cart
        {
            UserId = "u1"
        });
        db.Context.Orders.Add(new Order{UserId="u1",Total=10,Status="Pending"}); await db.Context.SaveChangesAsync(); var controller=new OrderController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.GetMyOrders();
        // Assert
        var first=((IEnumerable<object>)Assert.IsType<OkObjectResult>(result).Value!).Single(); Assert.Equal("Pending",first.GetType().GetProperty("Status")!.GetValue(first)); }

    [Fact] public async Task GetOrderById_ExistingOwnOrder_ReturnsOk() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        });

        db.Context.Carts.Add(new Cart
        {
            UserId = "u1"
        }); db.Context.Orders.Add(new Order{Id=1,UserId="u1",Total=10,Status="Pending"}); await db.Context.SaveChangesAsync(); var controller=new OrderController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.GetOrderById(1);
        // Assert
        Assert.IsType<OkObjectResult>(result); Assert.Single((IEnumerable<object>)((OkObjectResult)result).Value!); }

    [Fact] public async Task GetOrderById_OtherUsersOrder_ReturnsOkWithEmptyCollection_CurrentBehavior() { // Arrange
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
); db.Context.Orders.Add(new Order{Id=1,UserId="u2",Total=10,Status="Pending"}); await db.Context.SaveChangesAsync(); var controller=new OrderController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.GetOrderById(1);
        // Assert
        var ok=Assert.IsType<OkObjectResult>(result); Assert.Empty((IEnumerable<object>)ok.Value!); }

    [Fact] public async Task GetOrderById_MissingOrder_ReturnsOkWithEmptyCollection_CurrentBehavior() { // Arrange
        using var db=new TestDb(); var controller=new OrderController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.GetOrderById(999);
        // Assert
        var ok=Assert.IsType<OkObjectResult>(result); Assert.Empty((IEnumerable<object>)ok.Value!); }

    [Fact] public async Task GetOrderById_DoesNotExposeAnotherUsersOrderData() { // Arrange
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
); db.Context.Orders.Add(new Order{Id=1,UserId="u2",Total=999,Status="Paid"}); await db.Context.SaveChangesAsync(); var controller=new OrderController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.GetOrderById(1);
        // Assert
        Assert.Empty((IEnumerable<object>)Assert.IsType<OkObjectResult>(result).Value!); }

    [Fact] public async Task Checkout_WithoutCart_ReturnsBadRequest() { // Arrange
        using var db=new TestDb(); var controller=new OrderController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.Checkout();
        // Assert
        var bad=Assert.IsType<BadRequestObjectResult>(result); Assert.Equal("Cart is empty.",bad.Value!.GetType().GetProperty("error")!.GetValue(bad.Value)); }

    [Fact] public async Task Checkout_WithEmptyCart_ReturnsBadRequest() { // Arrange
        using var db=new TestDb(); db.Context.Users.Add(new User
        {
            Id = "u1",
            UserName = "u1",
            Email = "u1@example.com"
        });

        db.Context.Carts.Add(new Cart
        {
            UserId = "u1"
        });

        await db.Context.SaveChangesAsync();
        db.Context.Carts.Add(new Cart{UserId="u1"}); await db.Context.SaveChangesAsync(); var controller=new OrderController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.Checkout();
        // Assert
        Assert.IsType<BadRequestObjectResult>(result); }

    [Fact] public async Task ConfirmPayment_MissingOrder_ReturnsNotFound() { // Arrange
        using var db=new TestDb(); var controller=new OrderController(db.Context); TestHelpers.SetUser(controller,"u1");
        // Act
        var result=await controller.ConfirmPayment(999);
        // Assert
        Assert.IsType<NotFoundResult>(result); }
}
